using System.Collections.Generic;
using SimpleGame.Puzzle;

namespace SimpleGame.Game.Puzzle
{
    /// <summary>
    /// The sole bridge between the SimpleJigsaw package and the puzzle domain model.
    /// Converts a <see cref="SimpleJigsaw.PuzzleBoard"/> into puzzle domain data and
    /// returns both the raw board for rendering and the flat piece/seed/deck lists needed
    /// to construct a <see cref="PuzzleModel"/> directly.
    ///
    /// This is the ONLY file in SimpleGame.Game that may import SimpleJigsaw types.
    /// All other game code works exclusively with SimpleGame.Puzzle interfaces.
    /// </summary>
    public static class JigsawLevelFactory
    {
        /// <summary>
        /// Result of a build operation.
        /// </summary>
        public readonly struct JigsawBuildResult
        {
            /// <summary>
            /// Raw SimpleJigsaw board — pass to PieceObjectFactory.CreateAll for rendering.
            /// Keep this reference only in the adapter/rendering layer; never expose to presenters.
            /// </summary>
            public SimpleJigsaw.PuzzleBoard RawBoard { get; }

            /// <summary>All puzzle pieces (including seeds), ready to pass to PuzzleModel.</summary>
            public IReadOnlyList<IPuzzlePiece> PieceList { get; }

            /// <summary>IDs of pieces pre-placed on the board (anchors).</summary>
            public IReadOnlyList<int> SeedIds { get; }

            /// <summary>Ordered deck of non-seed piece IDs, ready to pass to PuzzleModel.</summary>
            public IReadOnlyList<int> DeckOrder { get; }

            public JigsawBuildResult(
                SimpleJigsaw.PuzzleBoard rawBoard,
                IReadOnlyList<IPuzzlePiece> pieceList,
                IReadOnlyList<int> seedIds,
                IReadOnlyList<int> deckOrder)
            {
                RawBoard  = rawBoard;
                PieceList = pieceList;
                SeedIds   = seedIds;
                DeckOrder = deckOrder;
            }
        }

        /// <summary>
        /// Builds puzzle data from a grid layout configuration.
        /// </summary>
        /// <param name="config">Grid layout — rows, columns, edge profile.</param>
        /// <param name="seed">Deterministic RNG seed for edge type assignment.</param>
        /// <param name="seedPieceIds">
        /// Piece IDs pre-placed on the board before gameplay.
        /// Pass null or empty to use no seeds (not recommended — nothing will be placeable).
        /// </param>
        public static JigsawBuildResult Build(
            SimpleJigsaw.GridLayoutConfig config,
            int   seed,
            int[] seedPieceIds = null)
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
            var seeds = (IReadOnlyList<int>)(seedPieceIds ?? System.Array.Empty<int>());

            // Build flat deck: all non-seed pieces in ascending ID order
            var seedSet = new HashSet<int>(seeds);
            var deckOrder = new List<int>(pieces.Count);
            foreach (var piece in pieces)
            {
                if (!seedSet.Contains(piece.Id))
                    deckOrder.Add(piece.Id);
            }
            deckOrder.Sort();

            return new JigsawBuildResult(rawBoard, pieces, seeds, deckOrder);
        }
    }
}
