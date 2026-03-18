using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// The puzzle board — tracks which pieces have been placed and enforces placement rules.
    /// </summary>
    public interface IPuzzleBoard
    {
        /// <summary>IDs of all pieces currently placed on the board.</summary>
        IReadOnlyCollection<int> PlacedIds { get; }

        /// <summary>
        /// Returns true if the given piece can be legally placed right now.
        /// A piece is placeable when at least one of its neighbors is already on the board.
        /// Seed pieces bypass this check — they are placed unconditionally at session start.
        /// </summary>
        bool CanPlace(int pieceId);

        /// <summary>
        /// Places the piece on the board unconditionally (caller must verify <see cref="CanPlace"/> first).
        /// Returns false if the piece was already placed (idempotent guard); true if newly placed.
        /// </summary>
        bool Place(int pieceId);
    }
}
