using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the LevelFailed popup. Shows score and level,
    /// resolves WaitForChoice with Retry, WatchAd, or Quit.
    /// </summary>
    public class LevelFailedPresenter : Presenter<ILevelFailedView>
    {
        private const int ContinueCost = 100;

        private UniTaskCompletionSource<LevelFailedChoice> _choiceTcs;

        public LevelFailedPresenter(ILevelFailedView view) : base(view) { }

        /// <summary>
        /// Initializes the popup with the score and level to display.
        /// </summary>
        public void Initialize(int score, int level)
        {
            View.OnRetryClicked += HandleRetry;
            View.OnWatchAdClicked += HandleWatchAd;
            View.OnQuitClicked += HandleQuit;
            View.OnContinueClicked += HandleContinue;
            View.UpdateScore($"Score: {score}");
            View.UpdateLevel($"Level {level}");
            View.UpdateContinueCost($"Continue ({ContinueCost} coins)");
        }

        public override void Dispose()
        {
            View.OnRetryClicked -= HandleRetry;
            View.OnWatchAdClicked -= HandleWatchAd;
            View.OnQuitClicked -= HandleQuit;
            View.OnContinueClicked -= HandleContinue;
            _choiceTcs?.TrySetCanceled();
            _choiceTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with the player's choice.
        /// </summary>
        public UniTask<LevelFailedChoice> WaitForChoice()
        {
            _choiceTcs?.TrySetCanceled();
            _choiceTcs = new UniTaskCompletionSource<LevelFailedChoice>();
            return _choiceTcs.Task;
        }

        private void HandleRetry()    => _choiceTcs?.TrySetResult(LevelFailedChoice.Retry);
        private void HandleWatchAd()  => _choiceTcs?.TrySetResult(LevelFailedChoice.WatchAd);
        private void HandleQuit()     => _choiceTcs?.TrySetResult(LevelFailedChoice.Quit);
        private void HandleContinue() => _choiceTcs?.TrySetResult(LevelFailedChoice.Continue);
    }
}
