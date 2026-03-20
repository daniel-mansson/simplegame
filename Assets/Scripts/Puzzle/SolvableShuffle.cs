using System;
using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Builds a deck ordering that is solvable by the greedy slot-based solver.
    ///
    /// <para><b>Model:</b> <c>placed</c> mirrors what the greedy slot-based solver
    /// would have placed from the committed deck prefix so far. This keeps
    /// valid/invalid classification in sync with <c>JigsawLevelFactory.IsSolvable</c>.</para>
    ///
    /// <para><b>Anti-trivialisation:</b> With <c>slotCount &gt; 1</c>, the algorithm
    /// occasionally emits a paired (invalid, valid) entry: an invalid piece immediately
    /// followed by its unlock neighbor. The pair occupies two consecutive deck positions
    /// so the solver always has the unlock piece in an adjacent slot. Only one such pair
    /// per <c>slotCount</c> window is allowed.</para>
    ///
    /// <para><b>Backtracking:</b> Up to <see cref="MaxBacktrackSteps"/> frames on
    /// deadlock. All solver loops are capped at <see cref="MaxSolverPasses"/>.</para>
    /// </summary>
    public static class SolvableShuffle
    {
        /// <summary>Maximum backtrack frames before giving up and appending best-effort.</summary>
        public const int MaxBacktrackSteps = 50;

        /// <summary>Hard cap on solver loop passes — prevents hangs on degenerate inputs.</summary>
        public const int MaxSolverPasses = 500;

        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a solvable deck ordering for the supplied puzzle graph.
        /// </summary>
        /// <param name="seedIds">Piece IDs pre-placed at game start. Not in the result.</param>
        /// <param name="pieces">All pieces including seeds — used to build the neighbour map.</param>
        /// <param name="slotCount">Number of independent player slots (≥ 1).</param>
        /// <param name="rng">Random number generator; caller controls the seed.</param>
        public static List<int> Shuffle(
            IReadOnlyList<int>          seedIds,
            IReadOnlyList<IPuzzlePiece> pieces,
            int                         slotCount,
            Random                      rng)
        {
            if (seedIds   == null) throw new ArgumentNullException(nameof(seedIds));
            if (pieces    == null) throw new ArgumentNullException(nameof(pieces));
            if (rng       == null) throw new ArgumentNullException(nameof(rng));
            if (slotCount < 1)    throw new ArgumentOutOfRangeException(nameof(slotCount), "slotCount must be >= 1");

            // ── Neighbour map ─────────────────────────────────────────────
            var neighbours = new Dictionary<int, List<int>>(pieces.Count);
            foreach (var p in pieces)
            {
                var nbrs = new List<int>(p.NeighborIds.Count);
                foreach (var n in p.NeighborIds) nbrs.Add(n);
                neighbours[p.Id] = nbrs;
            }

            // ── Candidate pool ────────────────────────────────────────────
            var seedSet     = new HashSet<int>(seedIds);
            var allNonSeeds = new List<int>(pieces.Count - seedSet.Count);
            foreach (var p in pieces)
                if (!seedSet.Contains(p.Id)) allNonSeeds.Add(p.Id);

            FisherYates(allNonSeeds, rng); // start from a random permutation

            // ── Working state ─────────────────────────────────────────────
            var remaining = new List<int>(allNonSeeds);
            var result    = new List<int>(allNonSeeds.Count);
            var btStack   = new Stack<BacktrackFrame>();
            var placed    = new HashSet<int>(seedIds); // mirrors IsSolvable's board state

            // Tracks consecutive anti-trivialisation pairs emitted since last
            // non-paired valid pick. One pair per window max.
            int pairsThisWindow = 0;

            while (remaining.Count > 0)
            {
                // ── Classify ──────────────────────────────────────────────
                var valid   = new List<int>();
                var invalid = new List<int>();
                foreach (var id in remaining)
                {
                    if (CanPlace(id, placed, neighbours)) valid.Add(id);
                    else                                   invalid.Add(id);
                }

                // ── Solvability gate ──────────────────────────────────────
                if (valid.Count == 0)
                {
                    if (!TryBacktrack(result, remaining, allNonSeeds, placed,
                                      seedIds, slotCount, neighbours, btStack,
                                      ref pairsThisWindow))
                    {
                        result.AddRange(remaining);
                        remaining.Clear();
                    }
                    continue;
                }

                // ── Anti-trivialisation: paired (invalid, valid) emission ─
                // Emit one invalid piece immediately followed by its unlock
                // neighbor (a valid piece). This guarantees the solver always
                // sees the unlock piece in the adjacent slot — safe by construction.
                // At most one pair per slotCount window.
                bool emittedPair = false;

                if (slotCount > 1
                    && pairsThisWindow == 0
                    && valid.Count >= 1
                    && remaining.Count >= 2        // need room for both entries
                    && rng.NextDouble() < 0.35)
                {
                    // Find a valid piece V and an invalid piece I where V ∈ neighbours(I)
                    var validSet = new HashSet<int>(valid);
                    if (FindPair(invalid, neighbours, validSet, rng,
                                 out int chosenInvalid, out int chosenValid))
                    {
                        // Emit: invalid first, then its unlock
                        result.Add(chosenInvalid);
                        remaining.Remove(chosenInvalid);

                        result.Add(chosenValid);
                        remaining.Remove(chosenValid);

                        // Advance placed: chosenValid is now placed (and may cascade)
                        placed.Add(chosenValid);
                        RunSolver(result, placed, slotCount, neighbours);

                        pairsThisWindow++;

                        // Push frame on the valid pick (chosenValid) for backtracking
                        if (btStack.Count < MaxBacktrackSteps)
                        {
                            btStack.Push(new BacktrackFrame(
                                resultCount:     result.Count,
                                chosenId:        chosenValid,
                                validCandidates: new List<int>(valid)));
                        }

                        emittedPair = true;
                    }
                }

                if (!emittedPair)
                {
                    // Normal pick: choose from valid
                    int chosen = valid[rng.Next(valid.Count)];
                    result.Add(chosen);
                    remaining.Remove(chosen);
                    placed.Add(chosen);
                    RunSolver(result, placed, slotCount, neighbours);
                    pairsThisWindow = 0; // reset window after a normal valid pick

                    if (btStack.Count < MaxBacktrackSteps)
                    {
                        btStack.Push(new BacktrackFrame(
                            resultCount:     result.Count,
                            chosenId:        chosen,
                            validCandidates: new List<int>(valid)));
                    }
                }
            }

            return result;
        }

        // ── Solver ────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the greedy slot-based solver on the committed <paramref name="deck"/>
        /// and advances <paramref name="placed"/> in-place. Capped at
        /// <see cref="MaxSolverPasses"/> to prevent hangs.
        /// </summary>
        private static void RunSolver(
            List<int>                  deck,
            HashSet<int>               placed,
            int                        slotCount,
            Dictionary<int, List<int>> neighbours)
        {
            if (deck.Count == 0) return;

            var queue = new Queue<int>(deck);
            var slots = new int?[slotCount];
            for (int i = 0; i < slotCount && queue.Count > 0; i++)
                slots[i] = queue.Dequeue();

            int remaining = deck.Count;
            int passes    = 0;

            while (remaining > 0 && passes++ < MaxSolverPasses)
            {
                bool progress = false;
                for (int i = 0; i < slotCount; i++)
                {
                    if (!slots[i].HasValue) continue;
                    int pid = slots[i].Value;

                    if (placed.Contains(pid))
                    {
                        // Already placed from a prior call — advance the slot
                        slots[i] = queue.Count > 0 ? queue.Dequeue() : (int?)null;
                        remaining--;
                        progress = true;
                        continue;
                    }

                    if (!CanPlace(pid, placed, neighbours)) continue;

                    placed.Add(pid);
                    remaining--;
                    slots[i] = queue.Count > 0 ? queue.Dequeue() : (int?)null;
                    progress  = true;
                }
                if (!progress) break;
            }
        }

        /// <summary>
        /// Runs the solver from scratch on <paramref name="deck"/> starting from
        /// <paramref name="seeds"/>. Returns the resulting placed set.
        /// </summary>
        private static HashSet<int> RunSolverFresh(
            List<int>                  deck,
            IReadOnlyList<int>         seeds,
            int                        slotCount,
            Dictionary<int, List<int>> neighbours)
        {
            var placed = new HashSet<int>(seeds);
            RunSolver(deck, placed, slotCount, neighbours);
            return placed;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static bool CanPlace(
            int                        id,
            HashSet<int>               placed,
            Dictionary<int, List<int>> neighbours)
        {
            if (!neighbours.TryGetValue(id, out var nbrs)) return false;
            foreach (var nbr in nbrs)
                if (placed.Contains(nbr)) return true;
            return false;
        }

        /// <summary>
        /// Finds a random (invalid, valid) pair where <paramref name="valid"/> piece
        /// V is a neighbour of invalid piece I — so placing V unlocks I.
        /// Returns false if no such pair exists.
        /// </summary>
        private static bool FindPair(
            List<int>                  invalid,
            Dictionary<int, List<int>> neighbours,
            HashSet<int>               validSet,
            Random                     rng,
            out int                    chosenInvalid,
            out int                    chosenValid)
        {
            // Collect all (I, V) pairs
            var pairs = new List<(int inv, int val)>();
            foreach (var id in invalid)
            {
                if (!neighbours.TryGetValue(id, out var nbrs)) continue;
                foreach (var nbr in nbrs)
                {
                    if (validSet.Contains(nbr))
                    {
                        pairs.Add((id, nbr));
                        break; // one unlock neighbor is sufficient
                    }
                }
            }

            if (pairs.Count == 0)
            {
                chosenInvalid = -1;
                chosenValid   = -1;
                return false;
            }

            var chosen = pairs[rng.Next(pairs.Count)];
            chosenInvalid = chosen.inv;
            chosenValid   = chosen.val;
            return true;
        }

        private static bool TryBacktrack(
            List<int>                  result,
            List<int>                  remaining,
            List<int>                  allNonSeeds,
            HashSet<int>               placed,
            IReadOnlyList<int>         seedIds,
            int                        slotCount,
            Dictionary<int, List<int>> neighbours,
            Stack<BacktrackFrame>      btStack,
            ref int                    pairsThisWindow)
        {
            while (btStack.Count > 0)
            {
                var frame = btStack.Pop();
                frame.ValidCandidates.Remove(frame.ChosenId);
                if (frame.ValidCandidates.Count == 0) continue;

                var alt = frame.ValidCandidates[0];

                // Trim result to just before the frame's original choice
                result.RemoveRange(frame.ResultCount - 1, result.Count - (frame.ResultCount - 1));
                result.Add(alt);

                // Recompute remaining
                var inResult = new HashSet<int>(result);
                remaining.Clear();
                foreach (var id in allNonSeeds)
                    if (!inResult.Contains(id)) remaining.Add(id);

                // Recompute placed (full solver — only on backtrack, infrequent)
                var fresh = RunSolverFresh(result, seedIds, slotCount, neighbours);
                placed.Clear();
                foreach (var id in fresh) placed.Add(id);

                pairsThisWindow = 0;

                btStack.Push(new BacktrackFrame(
                    resultCount:     result.Count,
                    chosenId:        alt,
                    validCandidates: new List<int>(frame.ValidCandidates)));

                return true;
            }
            return false;
        }

        private static void FisherYates<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ── BacktrackFrame ────────────────────────────────────────────────

        private readonly struct BacktrackFrame
        {
            public readonly int       ResultCount;
            public readonly int       ChosenId;
            public readonly List<int> ValidCandidates;

            public BacktrackFrame(int resultCount, int chosenId, List<int> validCandidates)
            {
                ResultCount     = resultCount;
                ChosenId        = chosenId;
                ValidCandidates = validCandidates;
            }
        }
    }
}
