using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;
using SimpleGame.Core.Unity;
using SimpleGame.Core.Unity.PopupManagement;
using SimpleGame.Core.Unity.ScreenManagement;
using SimpleGame.Core.Unity.TransitionManagement;
using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;
using UnityEngine;

namespace SimpleGame.Game.Boot
{
    /// <summary>
    /// Bootstraps infrastructure and drives the top-level navigation loop.
    /// Constructs all services and managers, then delegates all scene-level
    /// control flow to SceneControllers.
    ///
    /// Boot sequence:
    ///   1. PlayFab anonymous login (LoginWithCustomID — non-fatal if offline)
    ///   2. Cloud save pull + take-max merge into local PlayerPrefs
    ///   3. Build infrastructure (managers, services, UIFactory)
    ///   4. Load the initial screen (MainMenu)
    ///   5. Find and initialize the active SceneController
    ///   6. Await RunAsync() — only returns when navigation is decided
    ///   7. Navigate to the returned screen; repeat
    ///
    /// OnApplicationPause(true): pushes current MetaSaveData to cloud.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private WorldData _worldData;
        [SerializeField] private UnityInputBlocker _inputBlocker;
        [SerializeField] private UnityTransitionPlayer _transitionPlayer;
        [SerializeField] private UnityViewContainer _viewContainer;
        [SerializeField] private UnityCurrencyOverlay _currencyOverlay;

        private ScreenManager<ScreenId> _screenManager;
        private PopupManager<PopupId> _popupManager;
        private UIFactory _uiFactory;
        private ProgressionService _progressionService;
        private GameSessionService _sessionService;
        private HeartService _heartService;
        private IMetaSaveService _metaSaveService;
        private MetaProgressionService _metaProgressionService;
        private GoldenPieceService _goldenPieceService;
        private CoinsService _coinsService;
        private IPlayFabAuthService _authService;
        private ICloudSaveService _cloudSaveService;
        private IPlatformLinkService _platformLinkService;
        private IAnalyticsService _analyticsService;
        private IRemoteConfigService _remoteConfigService;
        private IAdService _adService;

        private async UniTaskVoid Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Debug.Log("[GameBootstrapper] Boot sequence started.");

            // --- PlayFab: authenticate before anything else ---
            _authService = new PlayFabAuthService();
            try
            {
                await _authService.LoginAsync();
            }
            catch (PlayFabLoginException ex)
            {
                // Non-fatal: game continues in offline mode without cloud features.
                Debug.LogWarning($"[GameBootstrapper] PlayFab login failed — continuing offline. Reason: {ex.Message}");
            }

            // --- Cloud save: pull and merge before loading local services ---
            _metaSaveService = new PlayerPrefsMetaSaveService();
            _cloudSaveService = new PlayFabCloudSaveService(_authService);

            var cloudData = await _cloudSaveService.PullAsync();
            if (cloudData != null)
            {
                var localData = _metaSaveService.Load();
                var merged = MetaSaveMerge.TakeMax(localData, cloudData);
                _metaSaveService.Save(merged);
                Debug.Log("[GameBootstrapper] Cloud save merged into local data.");
            }

            // --- Platform link service: refresh status after login ---
            _platformLinkService = new PlayFabPlatformLinkService(_authService);
            await _platformLinkService.RefreshLinkStatusAsync();

            // --- Analytics: session start ---
            _analyticsService = new PlayFabAnalyticsService(_authService);
            _analyticsService.TrackSessionStart();

            // --- Remote config: fetch before constructing gameplay services ---
            _remoteConfigService = new PlayFabRemoteConfigService(_authService);
            await _remoteConfigService.FetchAsync();

            // --- Build services ---
            var gameService = new GameService();
            _progressionService = new ProgressionService();
            _sessionService = new GameSessionService();
            _heartService = new HeartService();
            // _metaSaveService already initialized above (before cloud pull)
            _metaProgressionService = new MetaProgressionService(_worldData, _metaSaveService);
            _goldenPieceService = new GoldenPieceService(_metaSaveService, _analyticsService);
            _coinsService = new CoinsService(_metaSaveService, _analyticsService);

            // --- Build infrastructure ---
            var inputBlocker = _inputBlocker;
            var transitionPlayer = _transitionPlayer;
            var popupContainer = _viewContainer;
            var sceneLoader = new UnitySceneLoader();

            // --- Ads: initialize before navigation loop ---
            var unityAdService = new UnityAdService();
            unityAdService.SetAnalytics(_analyticsService);
            // TODO(M017): Replace with real App Key from LevelPlay dashboard once
            // com.unity.services.levelplay is installed and LEVELPLAY_ENABLED is set.
            // Install guide in UnityAdService.cs header comment.
            unityAdService.Initialize(appKey: "YOUR_LEVELPLAY_APP_KEY");
            _adService = unityAdService;

            _popupManager = new PopupManager<PopupId>(popupContainer, inputBlocker);
            _screenManager = new ScreenManager<ScreenId>(sceneLoader, transitionPlayer, inputBlocker,
                onBeforeSceneUnload: _popupManager.DismissAllAsync);
            _uiFactory = new UIFactory(gameService, _progressionService, _sessionService,
                                       _heartService, _metaProgressionService, _goldenPieceService,
                                       _coinsService);

            Debug.Log("[GameBootstrapper] Infrastructure ready. Starting navigation loop.");

            // --- First-launch platform link prompt ---
            if (_authService.IsLoggedIn && PlatformLinkPresenter.ShouldShow(_platformLinkService))
            {
                // The PlatformLink popup view must be pre-instantiated in the Boot scene.
                // If not found, the prompt is skipped silently.
                var linkPopupView = FindFirstObjectInBootScene<IPlatformLinkView>();
                if (linkPopupView != null)
                {
                    var linkPresenter = new PlatformLinkPresenter(linkPopupView, _platformLinkService);
                    linkPresenter.Initialize();
                    try
                    {
                        await _popupManager.ShowPopupAsync(PopupId.PlatformLink);
                        await linkPresenter.WaitForResult();
                    }
                    finally
                    {
                        await _popupManager.DismissPopupAsync();
                        linkPresenter.Dispose();
                    }
                }
                else
                {
                    // No view in scene — mark as seen so we don't prompt again next session
                    PlatformLinkPresenter.MarkSeen();
                    Debug.Log("[GameBootstrapper] PlatformLink popup view not found in Boot scene — skipping first-launch prompt.");
                }
            }

            // Determine initial screen
            var initialScreen = DetectAlreadyLoadedScreen();
            if (initialScreen.HasValue)
            {
                Debug.Log($"[GameBootstrapper] Adopting already-loaded screen: {initialScreen.Value}");
                _screenManager.AdoptScreen(initialScreen.Value);
            }
            else
            {
                await _screenManager.ShowScreenAsync(ScreenId.MainMenu);
            }

            // --- Navigation loop ---
            while (true)
            {
                var current = _screenManager.CurrentScreen;
                if (!current.HasValue)
                {
                    Debug.LogError("[GameBootstrapper] No current screen — cannot find SceneController.");
                    break;
                }

                switch (current.Value)
                {
                    case ScreenId.MainMenu:
                    {
                        var ctrl = FindSceneController<MainMenuSceneController>(current.Value.ToString());
                        if (ctrl == null)
                        {
                            Debug.LogError("[GameBootstrapper] MainMenuSceneController not found in scene.");
                            return;
                        }
                        ctrl.Initialize(_uiFactory, _popupManager, _metaProgressionService,
                                       _progressionService, _goldenPieceService, _coinsService, popupContainer);
                        var next = await ctrl.RunAsync();
                        await _screenManager.ShowScreenAsync(next);
                        break;
                    }
                    case ScreenId.Settings:
                    {
                        var ctrl = FindSceneController<SettingsSceneController>(current.Value.ToString());
                        if (ctrl == null)
                        {
                            Debug.LogError("[GameBootstrapper] SettingsSceneController not found in scene.");
                            return;
                        }
                        ctrl.Initialize(_uiFactory, _platformLinkService);
                        var next = await ctrl.RunAsync();
                        await _screenManager.ShowScreenAsync(next);
                        break;
                    }
                    case ScreenId.InGame:
                    {
                        var ctrl = FindSceneController<InGameSceneController>(current.Value.ToString());
                        if (ctrl == null)
                        {
                            Debug.LogError("[GameBootstrapper] InGameSceneController not found in scene.");
                            return;
                        }
                        ctrl.Initialize(_uiFactory, _progressionService, _sessionService,
                                       _popupManager, _goldenPieceService, _heartService, _coinsService, popupContainer, _currencyOverlay,
                                       onSessionEnd: async () =>
                                       {
                                           var data = _metaSaveService.Load();
                                           await _cloudSaveService.PushAsync(data);
                                       },
                                       analytics: _analyticsService,
                                       remoteConfig: _remoteConfigService.Config,
                                       adService: _adService);
                        var next = await ctrl.RunAsync();
                        await _screenManager.ShowScreenAsync(next);
                        break;
                    }
                    default:
                        Debug.LogWarning($"[GameBootstrapper] Unhandled ScreenId: {current.Value}");
                        return;
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause) return;
            _analyticsService?.TrackSessionEnd();
            // Push to cloud when app is backgrounded. Fire-and-forget — pause may be brief.
            var data = _metaSaveService?.Load();
            if (data != null)
                _cloudSaveService?.PushAsync(data).Forget();
        }

        private void OnApplicationQuit()
        {
            _analyticsService?.TrackSessionEnd();
        }

        private static ScreenId? DetectAlreadyLoadedScreen()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                if (scene.name == nameof(ScreenId.MainMenu)) return ScreenId.MainMenu;
                if (scene.name == nameof(ScreenId.Settings)) return ScreenId.Settings;
                if (scene.name == nameof(ScreenId.InGame)) return ScreenId.InGame;
            }
            return null;
        }

        private static T FindSceneController<T>(string sceneName) where T : Component
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponent<T>();
                if (controller != null) return controller;
            }
            return null;
        }

        /// <summary>
        /// Finds the first component implementing <typeparamref name="T"/> across all
        /// loaded scenes. Used to locate the PlatformLink popup view in the Boot scene.
        /// </summary>
        private static T FindFirstObjectInBootScene<T>() where T : class
        {
            for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var comp = root.GetComponentInChildren<T>(includeInactive: true);
                    if (comp != null) return comp;
                }
            }
            return null;
        }
    }
}
