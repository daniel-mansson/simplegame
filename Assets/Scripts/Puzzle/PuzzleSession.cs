using System;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Runtime puzzle session. Owns the board and decks, enforces placement rules,
    /// pre-places seeds on construction, and fires <see cref="OnPlacementResolved"/> after
    /// every placement attempt.
    /// </summary>
    public sealed class PuzzleSession
    {
        private readonly IPuzzleLevel _level;
        private readonly PuzzleBoard _board;

        /// <summary>
        /// Fires after every <see cref="TryPlace"/> call with the piece ID and the result.
        /// Fires even for Rejected and AlreadyPlaced outcomes.
        /// </summary>
        public event Action<int, PlacementResult> OnPlacementResolved;

        /// <summary>
        /// True when every piece in the level (including seeds) is on the board.
        /// </summary>
        public bool IsComplete => _board.PlacedIds.Count == _level.TotalPieceCount;

        /// <summary>
        /// Constructs a session and pre-places all seed pieces on the board.
        /// Seeds bypass the neighbor rule — they are placed unconditionally.
        /// </summary>
        public PuzzleSession(IPuzzleLevel level)
        {
            _level = level;
            _board = new PuzzleBoard(level.Pieces);

            // Pre-place seeds — they define the starting anchors for placement
            foreach (var seedId in level.SeedIds)
                _board.Place(seedId);
        }

        /// <summary>
        /// Attempt to place the given piece on the board.
        /// Returns <see cref="PlacementResult.Placed"/> on success,
        /// <see cref="PlacementResult.AlreadyPlaced"/> if already on board,
        /// <see cref="PlacementResult.Rejected"/> if no neighbor is present.
        /// Always fires <see cref="OnPlacementResolved"/>.
        /// On <see cref="PlacementResult.Placed"/>, advances all non-empty decks that had this piece at the front.
        /// </summary>
        public PlacementResult TryPlace(int pieceId)
        {
            PlacementResult result;

            if (_board.IsPlaced(pieceId))
            {
                result = PlacementResult.AlreadyPlaced;
            }
            else if (!_board.CanPlace(pieceId))
            {
                result = PlacementResult.Rejected;
            }
            else
            {
                _board.Place(pieceId);
                result = PlacementResult.Placed;

                // Advance each deck's cursor past any pieces that are now placed,
                // so the front always points to an unplaced piece (or becomes empty).
                foreach (var deck in _level.Decks)
                {
                    var front = deck.Peek();
                    while (front.HasValue && _board.IsPlaced(front.Value))
                    {
                        deck.Advance();
                        front = deck.Peek();
                    }
                }
            }

            OnPlacementResolved?.Invoke(pieceId, result);
            return result;
        }

        /// <summary>
        /// Returns the piece ID at the front of the deck for the given slot index,
        /// or null if that deck is empty or the slot index is out of range.
        /// </summary>
        public int? CurrentDeckPiece(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _level.Decks.Count)
                return null;
            return _level.Decks[slotIndex].Peek();
        }

        /// <summary>
        /// Peeks ahead in a deck: returns the piece ID at <paramref name="offset"/>
        /// positions from the current front (0 = front, 1 = next, 2 = one after that),
        /// or null if that position is beyond the deck end.
        /// </summary>
        public int? PeekDeckAt(int deckIndex, int offset)
        {
            if (deckIndex < 0 || deckIndex >= _level.Decks.Count)
                return null;
            return _level.Decks[deckIndex].PeekAt(offset);
        }

        /// <summary>Current state of the board (read-only).</summary>
        public System.Collections.Generic.IReadOnlyCollection<int> PlacedIds => _board.PlacedIds;

        /// <summary>True if the piece can be legally placed right now (has a placed neighbour).</summary>
        public bool CanPlace(int pieceId) => _board.CanPlace(pieceId);
    }
}
