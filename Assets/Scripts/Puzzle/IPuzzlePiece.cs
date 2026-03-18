using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// A single puzzle piece. Immutable data — identity and adjacency only.
    /// No rendering, position, or Unity types.
    /// </summary>
    public interface IPuzzlePiece
    {
        /// <summary>Unique identifier for this piece within a level.</summary>
        int Id { get; }

        /// <summary>
        /// IDs of pieces that share an edge with this piece.
        /// Used by the placement rule to determine whether a piece can be placed.
        /// </summary>
        IReadOnlyList<int> NeighborIds { get; }
    }
}
