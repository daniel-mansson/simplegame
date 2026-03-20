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

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a guaranteed-solvable puzzle. Uses <see cref="SolvableShuffle"/> to
        /// produce a solvable deck ordering by construction, then validates with the
        /// greedy solver as a safety net. Retries with new seeds if validation fails.
        /// Logs an error and returns the last attempt if <paramref name="maxAttempts"/>
        /// is exhausted.
        /// </summary>
        /// <param name="config">Grid layout — rows, columns, edge profile.</param>
        /// <param name="slotCount">Number of player slots (used by shuffle and solvability check).</param>
        /// <param name="initialSeed">Seed for the outer retry RNG. Each attempt draws the next seed from it.</param>
        /// <param name="maxAttempts">Maximum seeds to try before giving up (default 10).</param>
        /// <param name="seedPieceIds">Explicit start-piece override — useful in tests.</param>
        public static JigsawBuildResult BuildSolvable(
            SimpleJigsaw.GridLayoutConfig config,
            int   slotCount,
            int   initialSeed,
            int   maxAttempts = 10,
            int[] seedPieceIds = null)
        {
            var rng = new System.Random(initialSeed);
            JigsawBuildResult result = default;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int seed = rng.Next();
                result = Build(config, slotCount, seed, seedPieceIds);

                if (IsSolvable(result, slotCount))
                {
                    if (attempt > 0)
                        UnityEngine.Debug.Log(
                            $"[JigsawLevelFactory] Solvable layout found after {attempt + 1} attempts (seed={seed}).");
                    return result;
                }
            }

            UnityEngine.Debug.LogError(
                $"[JigsawLevelFactory] Could not find solvable layout after {maxAttempts} attempts " +
                $"(slotCount={slotCount}). Using last result (seed={result.Seed}). Puzzle may be unwinnable.");
            return result;
        }

        /// <summary>
        /// Builds puzzle data from a grid layout configuration using
        /// <see cref="SolvableShuffle"/> to order the deck.
        /// </summary>
        /// <param name="config">Grid layout — rows, columns, edge profile.</param>
        /// <param name="slotCount">
        /// Number of player slots. Passed to <see cref="SolvableShuffle.Shuffle"/> to
        /// guarantee the solvability window invariant for the given slot count.
        /// </param>
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
            int   slotCount,
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

            // Build deck using SolvableShuffle: guarantees at least one placeable piece
            // per slotCount-wide window throughout the deck ordering.
            var deckOrder = SolvableShuffle.Shuffle(seeds, pieces, slotCount, rng);

            return new JigsawBuildResult(rawBoard, pieces, seeds, deckOrder, seed);
        }

        // ── Solver ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Greedy solver: on each pass, places every slot-piece that currently has a
        /// placed neighbour. Repeats until the board is complete or a full pass yields
        /// no placements (deadlock → unsolvable with this deck order).
        ///
        /// Mirrors <see cref="PuzzleModel.TryPlace"/> logic without allocating a full
        /// model or subscribing to events.
        /// </summary>
        private static bool IsSolvable(in JigsawBuildResult result, int slotCount)
        {
            // Build neighbour lookup: pieceId → neighbour IDs
            var neighbours = new Dictionary<int, List<int>>(result.PieceList.Count);
            foreach (var piece in result.PieceList)
            {
                var nbrs = new List<int>();
                foreach (var nbr in piece.NeighborIds)
                    nbrs.Add(nbr);
                neighbours[piece.Id] = nbrs;
            }

            var placed = new HashSet<int>(result.SeedIds);
            var deck   = new Queue<int>(result.DeckOrder);

            var slots = new int?[slotCount];
            for (int i = 0; i < slotCount && deck.Count > 0; i++)
                slots[i] = deck.Dequeue();

            int remaining = result.DeckOrder.Count;

            while (remaining > 0)
            {
                bool progress = false;

                for (int i = 0; i < slotCount; i++)
                {
                    if (!slots[i].HasValue) continue;

                    int pid = slots[i].Value;

                    bool canPlace = false;
                    foreach (var nbr in neighbours[pid])
                    {
                        if (placed.Contains(nbr)) { canPlace = true; break; }
                    }
                    if (!canPlace) continue;

                    placed.Add(pid);
                    remaining--;
                    slots[i] = deck.Count > 0 ? deck.Dequeue() : (int?)null;
                    progress  = true;
                }

                if (!progress) return false; // deadlock — no slot can be placed
            }

            return true;
        }
    }
}
