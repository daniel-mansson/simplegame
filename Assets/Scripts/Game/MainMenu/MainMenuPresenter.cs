using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.MainMenu
{
    public class MainMenuPresenter : Presenter<IMainMenuView>
    {
        private UniTaskCompletionSource<MainMenuAction> _actionTcs;

        public MainMenuPresenter(IMainMenuView view) : base(view) { }

        public override void Initialize()
        {
            View.OnSettingsClicked += HandleSettingsClicked;
            View.OnPopupClicked += HandlePopupClicked;
            View.UpdateTitle("Main Menu");
        }

        public override void Dispose()
        {
            View.OnSettingsClicked -= HandleSettingsClicked;
            View.OnPopupClicked -= HandlePopupClicked;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with the action the user took.
        /// Each call resets the completion source — any previous pending task is cancelled.
        /// </summary>
        public UniTask<MainMenuAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<MainMenuAction>();
            return _actionTcs.Task;
        }

        private void HandleSettingsClicked() => _actionTcs?.TrySetResult(MainMenuAction.Settings);
        private void HandlePopupClicked() => _actionTcs?.TrySetResult(MainMenuAction.Popup);
    }
}
