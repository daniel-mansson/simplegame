using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using SimpleGame.Puzzle;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Presenter for the InGame scene. Delegates all placement decisions to
    /// <see cref="PuzzleSession"/> — the view fires <see cref="IInGameView.OnTapPiece"/>
    /// with a piece ID and the session determines correct/incorrect.
    ///
    /// Correct placement: piece counter advances. Win when session IsComplete.
    /// Incorrect placement: costs a heart. Lose when hearts reach 0.
    /// </summary>
    public class InGamePresenter : Presenter<IInGameView>
    {
        private readonly GameSessionService _session;
        private readonly IHeartService _hearts;
        private readonly IPuzzleLevel _level;
        private readonly int _initialHearts;

        private PuzzleSession _puzzleSession;
        private UniTaskCompletionSource<InGameAction> _actionTcs;

        public InGamePresenter(IInGameView view, GameSessionService session,
                               IHeartService hearts, IPuzzleLevel level, int initialHearts = 3)
            : base(view)
        {
            _session = session;
            _hearts = hearts;
            _level = level;
            _initialHearts = initialHearts;
        }

        public override void Initialize()
        {
            _puzzleSession = new PuzzleSession(_level);
            _hearts.Reset(_initialHearts);

            View.OnTapPiece += HandleTapPiece;

            View.UpdateLevelLabel($"Level {_session.CurrentLevelId}");
            View.UpdatePieceCounter($"0/{_level.TotalPieceCount - _level.SeedIds.Count}");
            View.UpdateHearts(_hearts.RemainingHearts.ToString());
        }

        public override void Dispose()
        {
            View.OnTapPiece -= HandleTapPiece;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the game ends (Win or Lose).
        /// </summary>
        public UniTask<InGameAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<InGameAction>();
            return _actionTcs.Task;
        }

        /// <summary>
        /// Restores hearts to full and re-arms the action source so gameplay continues
        /// from current piece progress. Called after a rewarded ad grants a retry.
        /// </summary>
        public void RestoreHeartsAndContinue()
        {
            _hearts.Reset(_initialHearts);
            View.UpdateHearts(_hearts.RemainingHearts.ToString());
        }

        private void HandleTapPiece(int pieceId)
        {
            var result = _puzzleSession.TryPlace(pieceId);

            if (result == PlacementResult.Placed)
            {
                int placed = _puzzleSession.PlacedIds.Count - _level.SeedIds.Count;
                int total = _level.TotalPieceCount - _level.SeedIds.Count;
                _session.CurrentScore = placed;
                View.UpdatePieceCounter($"{placed}/{total}");

                if (_puzzleSession.IsComplete)
                {
                    Debug.Log("[Ads] Interstitial ad opportunity — level complete");
                    _actionTcs?.TrySetResult(InGameAction.Win);
                }
            }
            else if (result == PlacementResult.Rejected)
            {
                _hearts.UseHeart();
                View.UpdateHearts(_hearts.RemainingHearts.ToString());

                if (!_hearts.IsAlive)
                {
                    _session.CurrentScore = _puzzleSession.PlacedIds.Count - _level.SeedIds.Count;
                    Debug.Log("[Ads] Interstitial ad opportunity — level failed");
                    _actionTcs?.TrySetResult(InGameAction.Lose);
                }
            }
            // AlreadyPlaced: silently ignore — no state change, no heart cost
        }
    }
}
