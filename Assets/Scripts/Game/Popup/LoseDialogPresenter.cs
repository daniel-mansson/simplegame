using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the LoseDialog popup. Shows score and level,
    /// resolves WaitForChoice with Retry or Back.
    /// </summary>
    public class LoseDialogPresenter : Presenter<ILoseDialogView>
    {
        private UniTaskCompletionSource<LoseDialogChoice> _choiceTcs;

        public LoseDialogPresenter(ILoseDialogView view) : base(view) { }

        /// <summary>
        /// Initializes the popup with the score and level to display.
        /// </summary>
        public void Initialize(int score, int level)
        {
            View.OnRetryClicked += HandleRetry;
            View.OnBackClicked += HandleBack;
            View.UpdateScore($"Score: {score}");
            View.UpdateLevel($"Level {level}");
        }

        public override void Dispose()
        {
            View.OnRetryClicked -= HandleRetry;
            View.OnBackClicked -= HandleBack;
            _choiceTcs?.TrySetCanceled();
            _choiceTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with the player's choice: Retry or Back.
        /// </summary>
        public UniTask<LoseDialogChoice> WaitForChoice()
        {
            _choiceTcs?.TrySetCanceled();
            _choiceTcs = new UniTaskCompletionSource<LoseDialogChoice>();
            return _choiceTcs.Task;
        }

        private void HandleRetry() => _choiceTcs?.TrySetResult(LoseDialogChoice.Retry);
        private void HandleBack() => _choiceTcs?.TrySetResult(LoseDialogChoice.Back);
    }
}
