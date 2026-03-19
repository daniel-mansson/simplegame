using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using SimpleGame.Puzzle;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Presenter for the InGame scene. Subscribes to <see cref="PuzzleModel"/> events
    /// and drives <see cref="IInGameView"/> reactively — no tray-window lookahead,
    /// no deck-cursor walking.
    ///
    /// <para>Event wiring (all in <see cref="Initialize"/>):</para>
    /// <list type="bullet">
    ///   <item><see cref="PuzzleModel.OnSlotChanged"/> → <see cref="IInGameView.RefreshTray"/> (temporary bridge until S03 slot-indexed view API)</item>
    ///   <item><see cref="PuzzleModel.OnPiecePlaced"/> → <see cref="IInGameView.RevealPiece"/> + counter update</item>
    ///   <item><see cref="PuzzleModel.OnRejected"/> → heart deduction + optional Lose signal</item>
    ///   <item><see cref="PuzzleModel.OnCompleted"/> → Win signal</item>
    /// </list>
    ///
    /// <para>The view still fires <c>OnTapPiece(pieceId)</c> (old API, changed in S03).
    /// This presenter bridges it by scanning slots for the tapped piece ID and calling
    /// <see cref="PuzzleModel.TryPlace(int)"/> with the matching slot index.</para>
    /// </summary>
    public class InGamePresenter : Presenter<IInGameView>
    {
        private readonly GameSessionService _session;
        private readonly IHeartService      _hearts;
        private readonly PuzzleModel        _model;
        private readonly int                _initialHearts;

        private UniTaskCompletionSource<InGameAction> _actionTcs;

        public InGamePresenter(IInGameView view, GameSessionService session,
                               IHeartService hearts, PuzzleModel model,
                               int initialHearts = 3)
            : base(view)
        {
            _session       = session;
            _hearts        = hearts;
            _model         = model;
            _initialHearts = initialHearts;
        }

        public override void Initialize()
        {
            _hearts.Reset(_initialHearts);

            // ── Subscribe to model events ─────────────────────────────────
            _model.OnSlotChanged += HandleSlotChanged;
            _model.OnPiecePlaced += HandlePiecePlaced;
            _model.OnRejected    += HandleRejected;
            _model.OnCompleted   += HandleCompleted;

            // ── Subscribe to view events ──────────────────────────────────
            View.OnTapPiece += HandleTapPiece;

            // ── Initial display ───────────────────────────────────────────
            View.UpdateLevelLabel($"Level {_session.CurrentLevelId}");
            View.UpdatePieceCounter($"0/{_model.TotalNonSeedCount}");
            View.UpdateHearts(_hearts.RemainingHearts.ToString());

            // Push initial tray state — one call per slot
            PushAllSlots();
        }

        public override void Dispose()
        {
            _model.OnSlotChanged -= HandleSlotChanged;
            _model.OnPiecePlaced -= HandlePiecePlaced;
            _model.OnRejected    -= HandleRejected;
            _model.OnCompleted   -= HandleCompleted;

            View.OnTapPiece -= HandleTapPiece;

            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
        }

        /// <summary>
        /// Returns a task that completes with the outcome action (Win or Lose).
        /// Called by <see cref="InGameSceneController"/> to await the result.
        /// </summary>
        public UniTask<InGameAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<InGameAction>();
            return _actionTcs.Task;
        }

        /// <summary>
        /// Restores hearts to the initial count and refreshes the hearts display.
        /// Called by <see cref="InGameSceneController"/> after a WatchAd or Continue.
        /// </summary>
        public void RestoreHeartsAndContinue()
        {
            _hearts.Reset(_initialHearts);
            View.UpdateHearts(_hearts.RemainingHearts.ToString());
        }

        // ── View event handler ────────────────────────────────────────────

        /// <summary>
        /// Temporary bridge: view fires OnTapPiece(pieceId) (old API).
        /// Scans slots for the piece and calls TryPlace on the matching slot.
        /// Replaced in S03 when the view moves to a slot-indexed tap API.
        /// </summary>
        private void HandleTapPiece(int pieceId)
        {
            for (int i = 0; i < _model.SlotCount; i++)
            {
                if (_model.GetSlot(i) == pieceId)
                {
                    Debug.Log($"[InGamePresenter] TapPiece id={pieceId} → slot {i}");
                    _model.TryPlace(i);
                    return;
                }
            }
            // Piece not in any slot — ignore (e.g. tap on a board piece)
            Debug.Log($"[InGamePresenter] TapPiece id={pieceId} — not found in any slot, ignored.");
        }

        // ── Model event handlers ──────────────────────────────────────────

        /// <summary>
        /// Temporary bridge: broadcasts slot change as a one-element RefreshTray call.
        /// Replaced in S03 when IInGameView gains RefreshSlot(int, int?).
        /// </summary>
        private void HandleSlotChanged(int slotIndex, int? pieceId)
        {
            // Build full slot window for view (slot count may vary)
            var window = new int?[_model.SlotCount];
            for (int i = 0; i < _model.SlotCount; i++)
                window[i] = _model.GetSlot(i);
            View.RefreshTray(window);
        }

        private void HandlePiecePlaced(int pieceId)
        {
            View.RevealPiece(pieceId);

            _session.CurrentScore = _model.PlacedCount;
            View.UpdatePieceCounter($"{_model.PlacedCount}/{_model.TotalNonSeedCount}");
        }

        private void HandleRejected(int slotIndex, int pieceId)
        {
            Debug.Log($"[InGamePresenter] Rejected slot={slotIndex} piece={pieceId}");
            _hearts.UseHeart();
            View.UpdateHearts(_hearts.RemainingHearts.ToString());

            if (!_hearts.IsAlive)
            {
                _session.CurrentScore = _model.PlacedCount;
                Debug.Log("[Ads] Interstitial ad opportunity — level failed");
                _actionTcs?.TrySetResult(InGameAction.Lose);
            }
        }

        private void HandleCompleted()
        {
            View.RefreshTray(System.Array.Empty<int?>());
            Debug.Log("[Ads] Interstitial ad opportunity — level complete");
            _actionTcs?.TrySetResult(InGameAction.Win);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>Sends the current slot state to the view on Initialize.</summary>
        private void PushAllSlots()
        {
            var window = new int?[_model.SlotCount];
            for (int i = 0; i < _model.SlotCount; i++)
                window[i] = _model.GetSlot(i);
            View.RefreshTray(window);
        }
    }
}
