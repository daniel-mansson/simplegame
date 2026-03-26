using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the LevelComplete popup. Shows score, level, and golden
    /// pieces earned. Resolves WaitForContinue when the player clicks Continue.
    /// </summary>
    public class LevelCompletePresenter : Presenter<ILevelCompleteView>
    {
        private UniTaskCompletionSource _continueTcs;

        public LevelCompletePresenter(ILevelCompleteView view) : base(view) { }

        /// <summary>
        /// Initializes the popup with score, level, and golden pieces earned.
        /// </summary>
        public void Initialize(int score, int level, int goldenPiecesEarned)
        {
            View.OnContinueClicked += HandleContinue;
            View.UpdateScore($"Score: {score}");
            View.UpdateLevel($"Level {level} Complete!");
            View.UpdateGoldenPieces($"+{goldenPiecesEarned} Golden Pieces");
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
