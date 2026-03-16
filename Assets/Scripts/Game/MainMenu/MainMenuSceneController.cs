using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Popup;
using UnityEngine;

namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// SceneController for the MainMenu scene. Owns the MainMenuPresenter and
    /// ConfirmDialogPresenter lifetimes. RunAsync loops until the user chooses
    /// to navigate away, handling popups inline without yielding control externally.
    /// Returns the ScreenId to navigate to next.
    ///
    /// ConfirmDialogView lives in Boot scene; it is discovered at runtime via
    /// FindFirstObjectByType when handling the popup, so cross-scene SerializeField
    /// wiring is not required.
    /// </summary>
    public class MainMenuSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private MainMenuView _mainMenuView;

        // Optional — wired in Boot scene. Discovered at runtime if null.
        [SerializeField] private ConfirmDialogView _confirmDialogView;

        // Allow test/editor code to supply mock views without a Unity scene.
        private IMainMenuView _mainMenuViewOverride;
        private IConfirmDialogView _confirmDialogViewOverride;

        private IMainMenuView ActiveMainMenuView => _mainMenuViewOverride != null ? _mainMenuViewOverride : _mainMenuView;

        private IConfirmDialogView ActiveConfirmDialogView
        {
            get
            {
                if (_confirmDialogViewOverride != null) return _confirmDialogViewOverride;
                if (_confirmDialogView != null) return _confirmDialogView;
                // Runtime fallback: ConfirmDialogView lives in the Boot scene.
                var found = FindFirstObjectByType<ConfirmDialogView>(FindObjectsInactive.Include);
                if (found == null)
                    Debug.LogError("[MainMenuSceneController] ConfirmDialogView not found in any loaded scene.");
                return found;
            }
        }

        private UIFactory _uiFactory;
        private PopupManager<PopupId> _popupManager;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, PopupManager<PopupId> popupManager)
        {
            _uiFactory = uiFactory;
            _popupManager = popupManager;
        }

        /// <summary>
        /// For editor / test use: supply mock views that override the serialized fields.
        /// </summary>
        public void SetViewsForTesting(IMainMenuView mainMenuView, IConfirmDialogView confirmDialogView)
        {
            _mainMenuViewOverride = mainMenuView;
            _confirmDialogViewOverride = confirmDialogView;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            var mainMenuPresenter = _uiFactory.CreateMainMenuPresenter(ActiveMainMenuView);
            mainMenuPresenter.Initialize();
            try
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    var action = await mainMenuPresenter.WaitForAction();

                    if (action == MainMenuAction.Settings)
                        return ScreenId.Settings;

                    if (action == MainMenuAction.Play)
                        return ScreenId.InGame;

                    if (action == MainMenuAction.Popup)
                        await HandleConfirmPopupAsync(ct);
                }
            }
            finally
            {
                mainMenuPresenter.Dispose();
            }
        }

        private async UniTask HandleConfirmPopupAsync(CancellationToken ct)
        {
            var confirmView = ActiveConfirmDialogView;
            if (confirmView == null) return; // error already logged

            var confirmPresenter = _uiFactory.CreateConfirmDialogPresenter(confirmView);
            confirmPresenter.Initialize();
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.ConfirmDialog, ct);
                await confirmPresenter.WaitForConfirmation();
                await _popupManager.DismissPopupAsync(ct);
            }
            finally
            {
                confirmPresenter.Dispose();
            }
        }
    }
}
