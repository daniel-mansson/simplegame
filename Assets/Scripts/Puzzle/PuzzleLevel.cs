using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Immutable level definition. Created by the jigsaw adapter and consumed by PuzzleSession.
    /// </summary>
    public sealed class PuzzleLevel : IPuzzleLevel
    {
        public IReadOnlyList<IPuzzlePiece> Pieces { get; }
        public IReadOnlyList<int> SeedIds { get; }
        public IReadOnlyList<IDeck> Decks { get; }
        public int TotalPieceCount => Pieces.Count;

        public PuzzleLevel(
            IReadOnlyList<IPuzzlePiece> pieces,
            IReadOnlyList<int> seedIds,
            IReadOnlyList<IDeck> decks)
        {
            Pieces = pieces ?? new List<IPuzzlePiece>();
            SeedIds = seedIds ?? new List<int>();
            Decks = decks ?? new List<IDeck>();
        }
    }
}
