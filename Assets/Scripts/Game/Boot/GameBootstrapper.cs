using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;
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
    ///   1. Build infrastructure (managers, services, UIFactory)
    ///   2. Load the initial screen (MainMenu)
    ///   3. Find and initialize the active SceneController
    ///   4. Await RunAsync() — only returns when navigation is decided
    ///   5. Navigate to the returned screen; repeat
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private WorldData _worldData;

        private ScreenManager<ScreenId> _screenManager;
        private PopupManager<PopupId> _popupManager;
        private UIFactory _uiFactory;
        private ProgressionService _progressionService;
        private GameSessionService _sessionService;
        private HeartService _heartService;
        private IMetaSaveService _metaSaveService;
        private MetaProgressionService _metaProgressionService;
        private GoldenPieceService _goldenPieceService;

        private async UniTaskVoid Start()
        {
            Debug.Log("[GameBootstrapper] Boot sequence started.");

            // --- Build services ---
            var gameService = new GameService();
            _progressionService = new ProgressionService();
            _sessionService = new GameSessionService();
            _heartService = new HeartService();
            _metaSaveService = new PlayerPrefsMetaSaveService();
            _metaProgressionService = new MetaProgressionService(_worldData, _metaSaveService);
            _goldenPieceService = new GoldenPieceService(_metaSaveService);

            // --- Build infrastructure ---
            var inputBlocker = FindFirstObjectByType<UnityInputBlocker>();
            var transitionPlayer = FindFirstObjectByType<UnityTransitionPlayer>(FindObjectsInactive.Include);
            var popupContainer = FindFirstObjectByType<UnityPopupContainer>();
            var sceneLoader = new UnitySceneLoader();

            _popupManager = new PopupManager<PopupId>(popupContainer, inputBlocker);
            _screenManager = new ScreenManager<ScreenId>(sceneLoader, transitionPlayer, inputBlocker,
                onBeforeSceneUnload: _popupManager.DismissAllAsync);
            _uiFactory = new UIFactory(gameService, _progressionService, _sessionService,
                                       _heartService, _metaProgressionService, _goldenPieceService);

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
                        var ctrl = FindFirstObjectByType<MainMenuSceneController>();
                        if (ctrl == null)
                        {
                            Debug.LogError("[GameBootstrapper] MainMenuSceneController not found in scene.");
                            return;
                        }
                        ctrl.Initialize(_uiFactory, _popupManager, _metaProgressionService);
                        var next = await ctrl.RunAsync();
                        await _screenManager.ShowScreenAsync(next);
                        break;
                    }
                    case ScreenId.Settings:
                    {
                        var ctrl = FindFirstObjectByType<SettingsSceneController>();
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
                        var ctrl = FindFirstObjectByType<InGameSceneController>();
                        if (ctrl == null)
                        {
                            Debug.LogError("[GameBootstrapper] InGameSceneController not found in scene.");
                            return;
                        }
                        ctrl.Initialize(_uiFactory, _progressionService, _sessionService,
                                       _popupManager, _goldenPieceService, _heartService);
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
    }
}
