using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Presenter for the InGame scene. Manages score state and resolves
    /// WaitForAction only when the player explicitly wins or loses.
    /// Score clicks are handled inline — they update the view but don't
    /// resolve the pending task.
    /// </summary>
    public class InGamePresenter : Presenter<IInGameView>
    {
        private readonly GameSessionService _session;
        private int _score;
        private UniTaskCompletionSource<InGameAction> _actionTcs;

        public InGamePresenter(IInGameView view, GameSessionService session)
            : base(view)
        {
            _session = session;
        }

        public override void Initialize()
        {
            _score = 0;
            View.OnScoreClicked += HandleScoreClicked;
            View.OnWinClicked += HandleWinClicked;
            View.OnLoseClicked += HandleLoseClicked;
            View.UpdateScore("0");
            View.UpdateLevelLabel($"Level {_session.CurrentLevelId}");
        }

        public override void Dispose()
        {
            View.OnScoreClicked -= HandleScoreClicked;
            View.OnWinClicked -= HandleWinClicked;
            View.OnLoseClicked -= HandleLoseClicked;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves only when the player wins or loses.
        /// Score clicks are handled inline and do not resolve this task.
        /// </summary>
        public UniTask<InGameAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<InGameAction>();
            return _actionTcs.Task;
        }

        private void HandleScoreClicked()
        {
            _score++;
            _session.CurrentScore = _score;
            View.UpdateScore(_score.ToString());
        }

        private void HandleWinClicked()
        {
            _session.CurrentScore = _score;
            _actionTcs?.TrySetResult(InGameAction.Win);
        }

        private void HandleLoseClicked()
        {
            _session.CurrentScore = _score;
            _actionTcs?.TrySetResult(InGameAction.Lose);
        }
    }
}
