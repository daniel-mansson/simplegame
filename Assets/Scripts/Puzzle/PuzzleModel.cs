using System;
using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Core puzzle state machine. Owns the board, a single shared deck, and N
    /// independently-tracked slots. Operates entirely on integer piece IDs — no
    /// Unity types, no rendering concerns.
    ///
    /// <para><b>Slot mechanics:</b> Each slot holds one piece ID drawn from the
    /// shared deck. When a slot's piece is successfully placed on the board the slot
    /// immediately draws the next piece from the deck top. Slots are independent —
    /// placing from slot 0 does not affect slot 1's contents.</para>
    ///
    /// <para><b>Placement rule:</b> A piece can be placed only when at least one of
    /// its declared neighbours is already on the board (the standard adjacency rule).
    /// Seed pieces are pre-placed at construction and act as the initial anchors.</para>
    ///
    /// <para><b>Events:</b> Subscribe before calling <see cref="TryPlace"/> to
    /// receive all state-change notifications. All events fire synchronously inside
    /// <see cref="TryPlace"/>.</para>
    /// </summary>
    public sealed class PuzzleModel
    {
        // ── Internal state ────────────────────────────────────────────────

        private readonly PuzzleBoard _board;
        private readonly Deck        _deck;
        private readonly int?[]      _slots;
        private readonly int         _totalNonSeedCount;
        private readonly int         _seedCount;

        private int  _placedNonSeedCount;
        private bool _completed;

        // ── Events ────────────────────────────────────────────────────────

        /// <summary>
        /// Fires when a slot's content changes after a successful placement.
        /// Parameters: (slotIndex, newPieceId) — newPieceId is null when the
        /// deck is exhausted and the slot becomes empty.
        /// </summary>
        public event Action<int, int?> OnSlotChanged;

        /// <summary>
        /// Fires when a piece is successfully placed on the board.
        /// Parameter: the piece ID that was placed.
        /// </summary>
        public event Action<int> OnPiecePlaced;

        /// <summary>
        /// Fires when a placement attempt is rejected (no placed neighbour).
        /// Parameters: (slotIndex, pieceId) — the slot that was tapped and the
        /// piece it holds. Slot content is unchanged.
        /// </summary>
        public event Action<int, int> OnRejected;

        /// <summary>
        /// Fires exactly once when all non-seed pieces have been placed on the board.
        /// </summary>
        public event Action OnCompleted;

        // ── Constructor ───────────────────────────────────────────────────

        /// <summary>
        /// Constructs a PuzzleModel and immediately pre-places all seed pieces.
        /// </summary>
        /// <param name="pieces">All pieces in the puzzle, including seeds.</param>
        /// <param name="seedIds">
        /// IDs of pieces that are pre-placed at construction. They bypass the
        /// adjacency rule and serve as starting anchors.
        /// </param>
        /// <param name="deckOrder">
        /// Ordered sequence of non-seed piece IDs. The first piece in this list
        /// occupies slot 0 at the start of the game.
        /// </param>
        /// <param name="slotCount">Number of independent slots. Must be ≥ 1.</param>
        public PuzzleModel(
            IReadOnlyList<IPuzzlePiece> pieces,
            IReadOnlyList<int>          seedIds,
            IReadOnlyList<int>          deckOrder,
            int                         slotCount)
        {
            if (pieces    == null) throw new ArgumentNullException(nameof(pieces));
            if (seedIds   == null) throw new ArgumentNullException(nameof(seedIds));
            if (deckOrder == null) throw new ArgumentNullException(nameof(deckOrder));
            if (slotCount < 1)    throw new ArgumentOutOfRangeException(nameof(slotCount), "slotCount must be ≥ 1");

            _board             = new PuzzleBoard(pieces);
            _deck              = new Deck(deckOrder);
            _slots             = new int?[slotCount];
            _seedCount         = seedIds.Count;
            _totalNonSeedCount = deckOrder.Count;

            // Pre-place seeds — they bypass the adjacency rule
            foreach (var id in seedIds)
                _board.Place(id);

            // Fill slots left-to-right from the deck front
            for (int i = 0; i < slotCount; i++)
            {
                _slots[i] = _deck.Peek();
                if (_slots[i].HasValue)
                    _deck.Advance();
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Number of slots.</summary>
        public int SlotCount => _slots.Length;

        /// <summary>
        /// True when all non-seed pieces are on the board.
        /// Becomes true at the same moment <see cref="OnCompleted"/> fires.
        /// </summary>
        public bool IsComplete => _placedNonSeedCount == _totalNonSeedCount;

        /// <summary>Number of non-seed pieces placed so far (player progress).</summary>
        public int PlacedCount => _placedNonSeedCount;

        /// <summary>Total number of non-seed pieces (the win target).</summary>
        public int TotalNonSeedCount => _totalNonSeedCount;

        /// <summary>
        /// Returns the piece ID currently in the given slot, or null if the slot
        /// is empty (deck exhausted) or the index is out of range.
        /// </summary>
        public int? GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return null;
            return _slots[slotIndex];
        }

        /// <summary>
        /// Attempts to place the piece currently held in <paramref name="slotIndex"/>.
        ///
        /// <list type="bullet">
        /// <item><see cref="SlotTapResult.Empty"/> — slot index out of range or slot has no piece.</item>
        /// <item><see cref="SlotTapResult.Rejected"/> — piece exists but cannot be placed (no placed
        ///   neighbour). <see cref="OnRejected"/> fires. Slot content is unchanged.</item>
        /// <item><see cref="SlotTapResult.Placed"/> — piece placed on board, slot refilled from deck
        ///   top (or set to null if deck exhausted). <see cref="OnSlotChanged"/> and
        ///   <see cref="OnPiecePlaced"/> fire. If this completes the puzzle,
        ///   <see cref="OnCompleted"/> fires immediately after.</item>
        /// </list>
        /// </summary>
        public SlotTapResult TryPlace(int slotIndex)
        {
            // ── Guard: out of range or empty slot ─────────────────────────
            if (slotIndex < 0 || slotIndex >= _slots.Length)
                return SlotTapResult.Empty;

            var pieceId = _slots[slotIndex];
            if (!pieceId.HasValue)
                return SlotTapResult.Empty;

            // ── Guard: adjacency rule ─────────────────────────────────────
            if (!_board.CanPlace(pieceId.Value))
            {
                OnRejected?.Invoke(slotIndex, pieceId.Value);
                return SlotTapResult.Rejected;
            }

            // ── Place piece on board ──────────────────────────────────────
            _board.Place(pieceId.Value);
            _placedNonSeedCount++;

            // ── Refill slot from deck top ─────────────────────────────────
            var nextPiece = _deck.Peek();
            _slots[slotIndex] = nextPiece;
            if (nextPiece.HasValue)
                _deck.Advance();

            // ── Fire events ───────────────────────────────────────────────
            OnSlotChanged?.Invoke(slotIndex, _slots[slotIndex]);
            OnPiecePlaced?.Invoke(pieceId.Value);

            if (IsComplete && !_completed)
            {
                _completed = true;
                OnCompleted?.Invoke();
            }

            return SlotTapResult.Placed;
        }
    }
}
