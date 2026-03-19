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

            /// <summary>The RNG seed used for this build (board layout + seed selection + shuffle).</summary>
            public int Seed { get; }

            public JigsawBuildResult(
                SimpleJigsaw.PuzzleBoard rawBoard,
                IReadOnlyList<IPuzzlePiece> pieceList,
                IReadOnlyList<int> seedIds,
                IReadOnlyList<int> deckOrder,
                int seed)
            {
                RawBoard  = rawBoard;
                PieceList = pieceList;
                SeedIds   = seedIds;
                DeckOrder = deckOrder;
                Seed      = seed;
            }
        }

        /// <summary>
        /// Builds puzzle data from a grid layout configuration.
        /// </summary>
        /// <param name="config">Grid layout — rows, columns, edge profile.</param>
        /// <param name="seed">
        /// RNG seed controlling board edge generation, seed-piece selection, and deck shuffle.
        /// Pass a random value each run for variety; pass a fixed value for deterministic replay.
        /// </param>
        /// <param name="seedPieceIds">
        /// Explicit seed piece override. When null (the default) one piece is chosen randomly
        /// from the board using <paramref name="seed"/>. Pass a non-null array to force specific
        /// pieces (useful in tests).
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

            // RNG — single instance driven by seed; used for seed-piece selection and deck shuffle
            var rng = new System.Random(seed);

            // Resolve seeds — pick one piece at random when not explicitly provided
            IReadOnlyList<int> seeds;
            if (seedPieceIds != null)
            {
                seeds = seedPieceIds;
            }
            else
            {
                var randomSeedId = rawBoard.Pieces[rng.Next(rawBoard.Pieces.Count)].Id;
                seeds = new[] { randomSeedId };
            }

            // Build deck: all non-seed pieces, shuffled with Fisher-Yates
            var seedSet = new HashSet<int>(seeds);
            var deckOrder = new List<int>(pieces.Count);
            foreach (var piece in pieces)
            {
                if (!seedSet.Contains(piece.Id))
                    deckOrder.Add(piece.Id);
            }
            Shuffle(deckOrder, rng);

            return new JigsawBuildResult(rawBoard, pieces, seeds, deckOrder, seed);
        }

        /// <summary>Fisher-Yates in-place shuffle.</summary>
        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
