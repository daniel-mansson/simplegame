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

            // --- Build services ---
            var gameService = new GameService();
            _progressionService = new ProgressionService();
            _sessionService = new GameSessionService();
            _heartService = new HeartService();
            // _metaSaveService already initialized above (before cloud pull)
            _metaProgressionService = new MetaProgressionService(_worldData, _metaSaveService);
            _goldenPieceService = new GoldenPieceService(_metaSaveService);
            _coinsService = new CoinsService(_metaSaveService);

            // --- Build infrastructure ---
            var inputBlocker = _inputBlocker;
            var transitionPlayer = _transitionPlayer;
            var popupContainer = _viewContainer;
            var sceneLoader = new UnitySceneLoader();

            _popupManager = new PopupManager<PopupId>(popupContainer, inputBlocker);
            _screenManager = new ScreenManager<ScreenId>(sceneLoader, transitionPlayer, inputBlocker,
                onBeforeSceneUnload: _popupManager.DismissAllAsync);
            _uiFactory = new UIFactory(gameService, _progressionService, _sessionService,
                                       _heartService, _metaProgressionService, _goldenPieceService,
                                       _coinsService);

            Debug.Log("[GameBootstrapper] Infrastructure ready. Starting navigation loop.");

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
                        ctrl.Initialize(_uiFactory);
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
                                       });
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
            // Push to cloud when app is backgrounded. Fire-and-forget — pause may be brief.
            var data = _metaSaveService?.Load();
            if (data != null)
                _cloudSaveService?.PushAsync(data).Forget();
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
    }
}
