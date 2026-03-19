using System.Linq;
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
    /// Correct placement: piece counter advances, tray refills with next 3 pieces.
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
            _session       = session;
            _hearts        = hearts;
            _level         = level;
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

            PushTrayWindow();
        }

        public override void Dispose()
        {
            View.OnTapPiece -= HandleTapPiece;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        public UniTask<InGameAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<InGameAction>();
            return _actionTcs.Task;
        }

        public void RestoreHeartsAndContinue()
        {
            _hearts.Reset(_initialHearts);
            View.UpdateHearts(_hearts.RemainingHearts.ToString());
        }

        // ── Tray helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Sends the next 3-piece lookahead window to the view.
        /// Passes an empty array when the deck is exhausted.
        /// </summary>
        /// <summary>
        /// Sends up to 3 unplaced deck pieces to the view.
        /// Skips already-placed pieces so the tray always shows 3 placeable pieces
        /// regardless of which order the player taps them.
        /// Layout: [left-preview, centre, right-preview] — centre is the closest to
        /// the front of the deck, but all 3 are tappable.
        /// </summary>
        private void PushTrayWindow()
        {
            // Collect the next 3 unplaced pieces from deck 0
            var window = new int?[3];
            int filled  = 0;
            int offset  = 0;
            while (filled < 3)
            {
                var id = _puzzleSession.PeekDeckAt(0, offset);
                if (!id.HasValue) break;
                offset++;
                if (_puzzleSession.PlacedIds.Contains(id.Value)) continue; // skip placed
                window[filled++] = id;
            }

            if (filled == 0)
            {
                View.RefreshTray(System.Array.Empty<int?>());
                return;
            }

            // Map to [left-preview=window[1], centre=window[0], right-preview=window[2]]
            View.RefreshTray(new int?[] { window[1], window[0], window[2] });
        }

        // ── Event handler ─────────────────────────────────────────────────

        private void HandleTapPiece(int pieceId)
        {
            var result = _puzzleSession.TryPlace(pieceId);
            Debug.Log($"[InGamePresenter] HandleTapPiece pieceId={pieceId} result={result}");

            if (result == PlacementResult.Placed)
            {
                View.RevealPiece(pieceId);

                int placed = _puzzleSession.PlacedIds.Count - _level.SeedIds.Count;
                int total  = _level.TotalPieceCount - _level.SeedIds.Count;
                _session.CurrentScore = placed;
                View.UpdatePieceCounter($"{placed}/{total}");

                if (_puzzleSession.IsComplete)
                {
                    View.RefreshTray(System.Array.Empty<int?>());
                    Debug.Log("[Ads] Interstitial ad opportunity — level complete");
                    _actionTcs?.TrySetResult(InGameAction.Win);
                }
                else
                {
                    PushTrayWindow();
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
            // AlreadyPlaced: silently ignore
        }
    }
}
