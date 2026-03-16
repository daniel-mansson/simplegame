using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the WinDialog popup. Shows score and level achieved,
    /// resolves WaitForContinue when the player clicks Continue.
    /// </summary>
    public class WinDialogPresenter : Presenter<IWinDialogView>
    {
        private UniTaskCompletionSource _continueTcs;

        public WinDialogPresenter(IWinDialogView view) : base(view) { }

        /// <summary>
        /// Initializes the popup with the score and level to display.
        /// Call this instead of the base Initialize() to pass data.
        /// </summary>
        public void Initialize(int score, int level)
        {
            View.OnContinueClicked += HandleContinue;
            View.UpdateScore($"Score: {score}");
            View.UpdateLevel($"Level {level} Complete!");
        }

        public override void Dispose()
        {
            View.OnContinueClicked -= HandleContinue;
            _continueTcs?.TrySetCanceled();
            _continueTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the player clicks Continue.
        /// </summary>
        public UniTask WaitForContinue()
        {
            _continueTcs?.TrySetCanceled();
            _continueTcs = new UniTaskCompletionSource();
            return _continueTcs.Task;
        }

        private void HandleContinue() => _continueTcs?.TrySetResult();
    }
}
