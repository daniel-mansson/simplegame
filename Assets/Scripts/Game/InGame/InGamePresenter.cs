using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Presenter for the InGame scene. Manages piece placement, hearts,
    /// and automatic win/lose resolution.
    ///
    /// Place-correct: increments piece counter. Win when all pieces placed.
    /// Place-incorrect: costs a heart. Lose when hearts reach 0.
    ///
    /// Debug logs fire at win/lose for interstitial ad stub.
    /// </summary>
    public class InGamePresenter : Presenter<IInGameView>
    {
        private readonly GameSessionService _session;
        private readonly IHeartService _hearts;
        private readonly int _totalPieces;
        private readonly int _initialHearts;

        private int _piecesPlaced;
        private UniTaskCompletionSource<InGameAction> _actionTcs;

        public InGamePresenter(IInGameView view, GameSessionService session,
                               IHeartService hearts, int totalPieces, int initialHearts = 3)
            : base(view)
        {
            _session = session;
            _hearts = hearts;
            _totalPieces = totalPieces;
            _initialHearts = initialHearts;
        }

        public override void Initialize()
        {
            _piecesPlaced = 0;
            _hearts.Reset(_initialHearts);

            View.OnPlaceCorrect += HandlePlaceCorrect;
            View.OnPlaceIncorrect += HandlePlaceIncorrect;

            View.UpdateLevelLabel($"Level {_session.CurrentLevelId}");
            View.UpdatePieceCounter($"0/{_totalPieces}");
            View.UpdateHearts(_hearts.RemainingHearts.ToString());
        }

        public override void Dispose()
        {
            View.OnPlaceCorrect -= HandlePlaceCorrect;
            View.OnPlaceIncorrect -= HandlePlaceIncorrect;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the game ends (Win or Lose).
        /// The presenter auto-resolves based on pieces placed and hearts remaining.
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

        private void HandlePlaceCorrect()
        {
            _piecesPlaced++;
            _session.CurrentScore = _piecesPlaced;
            View.UpdatePieceCounter($"{_piecesPlaced}/{_totalPieces}");

            if (_piecesPlaced >= _totalPieces)
            {
                Debug.Log("[Ads] Interstitial ad opportunity — level complete");
                _actionTcs?.TrySetResult(InGameAction.Win);
            }
        }

        private void HandlePlaceIncorrect()
        {
            _hearts.UseHeart();
            View.UpdateHearts(_hearts.RemainingHearts.ToString());

            if (!_hearts.IsAlive)
            {
                _session.CurrentScore = _piecesPlaced;
                Debug.Log("[Ads] Interstitial ad opportunity — level failed");
                _actionTcs?.TrySetResult(InGameAction.Lose);
            }
        }
    }
}
