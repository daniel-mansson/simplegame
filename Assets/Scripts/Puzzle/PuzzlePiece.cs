using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Immutable puzzle piece data — identity and adjacency.
    /// </summary>
    public sealed class PuzzlePiece : IPuzzlePiece
    {
        public int Id { get; }
        public IReadOnlyList<int> NeighborIds { get; }

        public PuzzlePiece(int id, IReadOnlyList<int> neighborIds)
        {
            Id = id;
            NeighborIds = neighborIds ?? new List<int>();
        }
    }
}
