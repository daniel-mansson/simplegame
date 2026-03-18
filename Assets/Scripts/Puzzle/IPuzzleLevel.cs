using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// The complete definition of a puzzle level.
    /// Immutable — describes what the level contains, not runtime state.
    /// </summary>
    public interface IPuzzleLevel
    {
        /// <summary>All pieces in this level, keyed implicitly by their <see cref="IPuzzlePiece.Id"/>.</summary>
        IReadOnlyList<IPuzzlePiece> Pieces { get; }

        /// <summary>
        /// IDs of pieces that are pre-placed on the board before gameplay begins.
        /// These act as anchors — neighboring pieces can be placed against them immediately.
        /// </summary>
        IReadOnlyList<int> SeedIds { get; }

        /// <summary>
        /// Ordered decks of piece IDs. One deck per slot.
        /// For a single-slot level, this has exactly one entry.
        /// </summary>
        IReadOnlyList<IDeck> Decks { get; }

        /// <summary>
        /// Total number of pieces including seeds.
        /// Win condition: all pieces placed.
        /// </summary>
        int TotalPieceCount { get; }
    }
}
