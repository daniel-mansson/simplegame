using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;

namespace SimpleGame.Game.MainMenu
{
    public class MainMenuPresenter : Presenter<IMainMenuView>
    {
        private readonly ProgressionService _progression;
        private readonly GameSessionService _session;
        private UniTaskCompletionSource<MainMenuAction> _actionTcs;

        public MainMenuPresenter(IMainMenuView view, ProgressionService progression, GameSessionService session)
            : base(view)
        {
            _progression = progression;
            _session = session;
        }

        public override void Initialize()
        {
            View.OnSettingsClicked += HandleSettingsClicked;
            View.OnPopupClicked += HandlePopupClicked;
            View.OnPlayClicked += HandlePlayClicked;
            View.UpdateTitle("Main Menu");
            View.UpdateLevelDisplay($"Level {_progression.CurrentLevel}");
        }

        public override void Dispose()
        {
            View.OnSettingsClicked -= HandleSettingsClicked;
            View.OnPopupClicked -= HandlePopupClicked;
            View.OnPlayClicked -= HandlePlayClicked;
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

        private void HandlePlayClicked()
        {
            _session.ResetForNewGame(_progression.CurrentLevel);
            _actionTcs?.TrySetResult(MainMenuAction.Play);
        }
    }
}
