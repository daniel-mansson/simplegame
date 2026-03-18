using System.Collections.Generic;
using SimpleGame.Puzzle;

namespace SimpleGame.Game.Puzzle
{
    /// <summary>
    /// The sole bridge between the SimpleJigsaw package and the puzzle domain model.
    /// Converts a SimpleJigsaw.PuzzleBoard into an IPuzzleLevel and returns both
    /// the domain level and the raw board for rendering via PieceObjectFactory.
    ///
    /// This is the ONLY file in SimpleGame.Game that may import SimpleJigsaw types.
    /// All other game code works exclusively with SimpleGame.Puzzle interfaces.
    /// </summary>
    public static class JigsawLevelFactory
    {
        /// <summary>
        /// Result of a build operation — domain level for game logic, raw board for rendering.
        /// </summary>
        public readonly struct JigsawBuildResult
        {
            /// <summary>Domain level — pass to PuzzleSession constructor.</summary>
            public IPuzzleLevel Level { get; }

            /// <summary>
            /// Raw SimpleJigsaw board — pass to PieceObjectFactory.CreateAll for rendering.
            /// Keep this reference only in the adapter/rendering layer; never expose to presenters.
            /// </summary>
            public SimpleJigsaw.PuzzleBoard RawBoard { get; }

            public JigsawBuildResult(IPuzzleLevel level, SimpleJigsaw.PuzzleBoard rawBoard)
            {
                Level = level;
                RawBoard = rawBoard;
            }
        }

        /// <summary>
        /// Builds a puzzle level from a grid layout configuration.
        /// </summary>
        /// <param name="config">Grid layout — rows, columns, edge profile.</param>
        /// <param name="seed">Deterministic RNG seed for edge type assignment.</param>
        /// <param name="seedPieceIds">
        /// Piece IDs pre-placed on the board before gameplay.
        /// Pass null or empty to use no seeds (not recommended — nothing will be placeable).
        /// </param>
        /// <param name="deckOrders">
        /// Ordered piece ID sequences, one per slot.
        /// Pass null to auto-generate a single shared deck of all non-seed pieces in ID order.
        /// </param>
        public static JigsawBuildResult Build(
            SimpleJigsaw.GridLayoutConfig config,
            int seed,
            int[] seedPieceIds = null,
            int[][] deckOrders = null)
        {
            // Generate the jigsaw board — this is the only SimpleJigsaw call
            var rawBoard = SimpleJigsaw.BoardFactory.Generate(config, seed);

            // Map PieceDescriptors → domain PuzzlePieces
            var pieces = new List<IPuzzlePiece>(rawBoard.Pieces.Count);
            foreach (var descriptor in rawBoard.Pieces)
            {
                var neighborIds = new List<int>(descriptor.Neighbors.Count);
                foreach (var (neighborId, _) in descriptor.Neighbors)
                    neighborIds.Add(neighborId);

                pieces.Add(new PuzzlePiece(descriptor.Id, neighborIds));
            }

            // Resolve seeds
            var seeds = seedPieceIds ?? System.Array.Empty<int>();

            // Resolve decks
            List<IDeck> decks;
            if (deckOrders != null && deckOrders.Length > 0)
            {
                decks = new List<IDeck>(deckOrders.Length);
                foreach (var order in deckOrders)
                    decks.Add(new Deck(order));
            }
            else
            {
                // Default: single shared deck of all non-seed pieces in ascending ID order
                var seedSet = new HashSet<int>(seeds);
                var defaultOrder = new List<int>(pieces.Count);
                foreach (var piece in pieces)
                {
                    if (!seedSet.Contains(piece.Id))
                        defaultOrder.Add(piece.Id);
                }
                defaultOrder.Sort();
                decks = new List<IDeck> { new Deck(defaultOrder) };
            }

            var level = new PuzzleLevel(pieces, seeds, decks);
            return new JigsawBuildResult(level, rawBoard);
        }
    }
}
