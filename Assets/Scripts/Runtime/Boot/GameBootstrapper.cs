using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;
using SimpleGame.Core.Services;
using SimpleGame.Runtime.MVP;
using SimpleGame.Runtime.PopupManagement;
using SimpleGame.Runtime.ScreenManagement;
using SimpleGame.Runtime.TransitionManagement;
using UnityEngine;

namespace SimpleGame.Runtime.Boot
{
    /// <summary>
    /// Composes the full dependency chain at play-mode startup:
    ///   GameService → UnitySceneLoader → ScreenManager + PopupManager
    ///   → UIFactory → first navigation → presenter lifecycle.
    ///
    /// Boot sequence is traced via Debug.Log — each phase is individually
    /// identifiable so a NullReferenceException pinpoints which wire-up failed.
    ///
    /// Post-navigation pattern: after each screen transition, find the new
    /// view MonoBehaviour in the loaded scene, create its presenter via
    /// UIFactory, initialize it, and dispose the previous presenter.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private ScreenManager _screenManager;
        private PopupManager _popupManager;
        private UIFactory _uiFactory;

        // Track the active presenter so it can be disposed on the next navigation.
        // Using the abstract base type so we can call Dispose() without knowing
        // the concrete presenter type.
        private IView _activeScreenView;
        private IView _activePopupView;

        // Hold concrete presenter references for disposal (interface via Presenter<T>).
        private object _activeScreenPresenter;
        private object _activePopupPresenter;

        private async UniTaskVoid Start()
        {
            Debug.Log("[GameBootstrapper] Boot sequence started.");

            // (a) Create domain service
            var gameService = new GameService();
            Debug.Log("[GameBootstrapper] GameService created.");

            // (b-d) Locate infrastructure MonoBehaviours from Boot scene
            var inputBlocker = FindFirstObjectByType<UnityInputBlocker>();
            Debug.Log($"[GameBootstrapper] UnityInputBlocker found: {inputBlocker != null}");

            var transitionPlayer = FindFirstObjectByType<UnityTransitionPlayer>();
            Debug.Log($"[GameBootstrapper] UnityTransitionPlayer found: {transitionPlayer != null}");

            var popupContainer = FindFirstObjectByType<UnityPopupContainer>();
            Debug.Log($"[GameBootstrapper] UnityPopupContainer found: {popupContainer != null}");

            // (e) Create scene loader
            var sceneLoader = new UnitySceneLoader();
            Debug.Log("[GameBootstrapper] UnitySceneLoader created.");

            // (f) Create ScreenManager
            _screenManager = new ScreenManager(sceneLoader, transitionPlayer, inputBlocker);
            Debug.Log("[GameBootstrapper] ScreenManager created.");

            // (g) Create PopupManager
            _popupManager = new PopupManager(popupContainer, inputBlocker);
            Debug.Log("[GameBootstrapper] PopupManager created.");

            // (h) Create UIFactory with navigation/popup callbacks
            _uiFactory = new UIFactory(
                gameService,
                navigateCallback: screenId => NavigateAndWirePresenter(screenId).Forget(),
                showPopupCallback: popupId => ShowPopupAndWirePresenter(popupId).Forget(),
                goBackCallback: GoBackAndWirePresenter,
                dismissPopupCallback: DismissPopupAndDisposePresenter
            );
            Debug.Log("[GameBootstrapper] UIFactory created.");

            // (i) Navigate to MainMenu first
            Debug.Log("[GameBootstrapper] Navigating to MainMenu...");
            await _screenManager.ShowScreenAsync(ScreenId.MainMenu);
            Debug.Log("[GameBootstrapper] MainMenu scene loaded.");

            // (j) Wire the MainMenu view and presenter after scene load
            WireMainMenuPresenter();

            Debug.Log("[GameBootstrapper] Boot sequence complete. Ready.");
        }

        // ── Navigation helpers ────────────────────────────────────────────────

        private async UniTask NavigateAndWirePresenter(ScreenId screenId)
        {
            DisposeScreenPresenter();
            await _screenManager.ShowScreenAsync(screenId);

            switch (screenId)
            {
                case ScreenId.MainMenu:
                    WireMainMenuPresenter();
                    break;
                case ScreenId.Settings:
                    WireSettingsPresenter();
                    break;
            }
        }

        private async UniTask GoBackAndWirePresenter()
        {
            DisposeScreenPresenter();
            await _screenManager.GoBackAsync();

            var current = _screenManager.CurrentScreen;
            if (current == ScreenId.MainMenu)
                WireMainMenuPresenter();
            else if (current == ScreenId.Settings)
                WireSettingsPresenter();
        }

        private async UniTask ShowPopupAndWirePresenter(PopupId popupId)
        {
            await _popupManager.ShowPopupAsync(popupId);
            WireConfirmDialogPresenter();
        }

        private async UniTask DismissPopupAndDisposePresenter()
        {
            DisposePopupPresenter();
            await _popupManager.DismissPopupAsync();
        }

        // ── Presenter wiring ─────────────────────────────────────────────────

        private void WireMainMenuPresenter()
        {
            var view = FindFirstObjectByType<MainMenuView>();
            if (view == null)
            {
                Debug.LogError("[GameBootstrapper] MainMenuView not found after scene load.");
                return;
            }

            var presenter = _uiFactory.CreateMainMenuPresenter(view);
            presenter.Initialize();
            _activeScreenPresenter = presenter;
            Debug.Log("[GameBootstrapper] MainMenuPresenter initialized.");
        }

        private void WireSettingsPresenter()
        {
            var view = FindFirstObjectByType<SettingsView>();
            if (view == null)
            {
                Debug.LogError("[GameBootstrapper] SettingsView not found after scene load.");
                return;
            }

            var presenter = _uiFactory.CreateSettingsPresenter(view);
            presenter.Initialize();
            _activeScreenPresenter = presenter;
            Debug.Log("[GameBootstrapper] SettingsPresenter initialized.");
        }

        private void WireConfirmDialogPresenter()
        {
            var view = FindFirstObjectByType<ConfirmDialogView>();
            if (view == null)
            {
                Debug.LogError("[GameBootstrapper] ConfirmDialogView not found after popup show.");
                return;
            }

            var presenter = _uiFactory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();
            _activePopupPresenter = presenter;
            Debug.Log("[GameBootstrapper] ConfirmDialogPresenter initialized.");
        }

        private void DisposeScreenPresenter()
        {
            if (_activeScreenPresenter is MainMenuPresenter mmp) mmp.Dispose();
            else if (_activeScreenPresenter is SettingsPresenter sp) sp.Dispose();
            _activeScreenPresenter = null;
        }

        private void DisposePopupPresenter()
        {
            if (_activePopupPresenter is ConfirmDialogPresenter cdp) cdp.Dispose();
            _activePopupPresenter = null;
        }
    }
}
