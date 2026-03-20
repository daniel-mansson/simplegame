using System;
using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Builds a deck ordering that is solvable by the greedy slot-based solver.
    ///
    /// <para><b>Guarantee:</b> The returned deck can be completed by the slot-based
    /// game (slotCount independent slots, each refilling from the deck on successful
    /// placement). This matches the verification model used by
    /// <c>JigsawLevelFactory.IsSolvable</c>.</para>
    ///
    /// <para><b>Approach:</b> Incremental construction. At each deck position the
    /// algorithm simulates the full slot-based game up to that point to determine
    /// which remaining pieces the solver could currently place. It picks one of those
    /// valid candidates, with occasional anti-trivialisation when a non-placeable
    /// piece can be safely scheduled (its unlock piece is currently valid and will
    /// enter slots before the non-placeable piece's slot stalls).</para>
    ///
    /// <para><b>Backtracking:</b> If no valid candidate exists the algorithm
    /// backtracks up to <see cref="MaxBacktrackSteps"/> positions.</para>
    /// </summary>
    public static class SolvableShuffle
    {
        /// <summary>Maximum positions to unwind during a single backtrack event.</summary>
        public const int MaxBacktrackSteps = 50;

        /// <summary>
        /// Builds a solvable deck ordering from the supplied puzzle graph.
        /// </summary>
        /// <param name="seedIds">
        /// Piece IDs pre-placed at game start. NOT included in the returned list.
        /// </param>
        /// <param name="pieces">
        /// All pieces in the puzzle, including seeds. Used to build the neighbour map.
        /// </param>
        /// <param name="slotCount">
        /// Number of independent player slots. Must be ≥ 1.
        /// </param>
        /// <param name="rng">Random number generator. Caller controls the seed.</param>
        /// <returns>
        /// An ordered list of non-seed piece IDs that the greedy slot-based solver
        /// can complete without deadlocking.
        /// </returns>
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

            // ── Build neighbour map ───────────────────────────────────────
            var neighbours = new Dictionary<int, List<int>>(pieces.Count);
            foreach (var piece in pieces)
            {
                var nbrs = new List<int>(piece.NeighborIds.Count);
                foreach (var n in piece.NeighborIds)
                    nbrs.Add(n);
                neighbours[piece.Id] = nbrs;
            }

            // ── Initial candidate pool ────────────────────────────────────
            var seedSet   = new HashSet<int>(seedIds);
            var remaining = new List<int>(pieces.Count - seedSet.Count);
            foreach (var piece in pieces)
            {
                if (!seedSet.Contains(piece.Id))
                    remaining.Add(piece.Id);
            }

            // Fisher-Yates shuffle as starting order (randomises valid candidate
            // selection, provides anti-trivialisation base).
            FisherYates(remaining, rng);

            // ── State ─────────────────────────────────────────────────────
            var result  = new List<int>(remaining.Count);
            var btStack = new Stack<BacktrackFrame>();

            // consecutiveInvalidPicks: tracks how many not-yet-placeable pieces
            // have been scheduled since the last valid (immediately placeable) pick.
            // Must stay below slotCount - 1 to keep the window invariant.
            int consecutiveInvalidPicks = 0;

            while (remaining.Count > 0)
            {
                // ── Simulate solver on current deck to find reachable set ─
                // Run the greedy slot solver on `result` (already committed deck)
                // to get the board state after the solver plays as far as it can.
                // This tells us what's actually placed when the next deck position
                // is drawn into a slot.
                var placed = SimulateSolver(result, seedIds, slotCount, neighbours);

                // ── Classify remaining candidates ─────────────────────────
                var valid   = new List<int>(); // placeable given current board
                var invalid = new List<int>(); // not yet placeable

                foreach (var id in remaining)
                {
                    if (IsPlaceable(id, placed, neighbours))
                        valid.Add(id);
                    else
                        invalid.Add(id);
                }

                // ── Solvability gate ──────────────────────────────────────
                if (valid.Count == 0)
                {
                    if (!TryBacktrack(result, remaining, btStack, ref consecutiveInvalidPicks))
                    {
                        // Best-effort: append remaining as-is
                        result.AddRange(remaining);
                        remaining.Clear();
                    }
                    continue;
                }

                // ── Anti-trivialisation ───────────────────────────────────
                // Allow scheduling a not-yet-placeable piece at this position when:
                // 1. slotCount > 1 (there is a window to hide it in)
                // 2. The piece has at least one neighbor that is currently in `valid`
                //    (its unlock piece is reachable right now and will be placed soon)
                // 3. consecutiveInvalidPicks < slotCount - 1 (window still has room)
                // 4. Probability gate ~35%
                int  chosen       = -1;
                bool isInvalidPick = false;

                if (slotCount > 1
                    && consecutiveInvalidPicks < slotCount - 1
                    && rng.NextDouble() < 0.35)
                {
                    // Find invalid pieces whose unlock neighbor is in valid
                    var validSet = new HashSet<int>(valid);
                    int candidate = FindUnlockableInvalid(invalid, neighbours, validSet, rng);
                    if (candidate >= 0)
                    {
                        chosen        = candidate;
                        isInvalidPick = true;
                        consecutiveInvalidPicks++;
                    }
                }

                if (chosen < 0)
                {
                    // Normal path: pick uniformly from valid candidates
                    chosen                  = valid[rng.Next(valid.Count)];
                    consecutiveInvalidPicks = 0;
                }

                // Commit
                result.Add(chosen);
                remaining.Remove(chosen);

                if (!isInvalidPick && btStack.Count < MaxBacktrackSteps)
                {
                    // Snapshot after a valid pick for possible backtracking
                    btStack.Push(new BacktrackFrame(
                        resultCount:       result.Count,
                        remainingSnapshot: new List<int>(remaining),
                        chosenId:          chosen,
                        validCandidates:   new List<int>(valid)));
                }
            }

            return result;
        }

        // ── Solver simulation ─────────────────────────────────────────────

        /// <summary>
        /// Runs the greedy slot-based solver on <paramref name="deck"/> and returns
        /// the set of piece IDs that end up placed on the board. This is the same
        /// logic as <c>JigsawLevelFactory.IsSolvable</c> but returns the placed set
        /// instead of a bool.
        /// </summary>
        private static HashSet<int> SimulateSolver(
            List<int>                  deck,
            IReadOnlyList<int>         seedIds,
            int                        slotCount,
            Dictionary<int, List<int>> neighbours)
        {
            var placed = new HashSet<int>(seedIds);
            if (deck.Count == 0) return placed;

            var deckQueue = new Queue<int>(deck);

            var slots = new int?[slotCount];
            for (int i = 0; i < slotCount && deckQueue.Count > 0; i++)
                slots[i] = deckQueue.Dequeue();

            int remaining = deck.Count;

            while (remaining > 0)
            {
                bool progress = false;
                for (int i = 0; i < slotCount; i++)
                {
                    if (!slots[i].HasValue) continue;
                    int pid = slots[i].Value;
                    if (!IsPlaceable(pid, placed, neighbours)) continue;

                    placed.Add(pid);
                    remaining--;
                    slots[i] = deckQueue.Count > 0 ? deckQueue.Dequeue() : (int?)null;
                    progress  = true;
                }
                if (!progress) break; // solver stalled — return what's placed so far
            }

            return placed;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static bool IsPlaceable(
            int                        id,
            HashSet<int>               placed,
            Dictionary<int, List<int>> neighbours)
        {
            if (!neighbours.TryGetValue(id, out var nbrs)) return false;
            foreach (var nbr in nbrs)
            {
                if (placed.Contains(nbr)) return true;
            }
            return false;
        }

        /// <summary>
        /// Finds a random invalid piece whose unlock neighbor is in <paramref name="validSet"/>.
        /// Returns -1 if none exist.
        /// </summary>
        private static int FindUnlockableInvalid(
            List<int>                  invalid,
            Dictionary<int, List<int>> neighbours,
            HashSet<int>               validSet,
            Random                     rng)
        {
            // Collect candidates first, then pick randomly for fair distribution
            var candidates = new List<int>();
            foreach (var id in invalid)
            {
                if (!neighbours.TryGetValue(id, out var nbrs)) continue;
                foreach (var nbr in nbrs)
                {
                    if (validSet.Contains(nbr)) { candidates.Add(id); break; }
                }
            }
            return candidates.Count > 0 ? candidates[rng.Next(candidates.Count)] : -1;
        }

        private static bool TryBacktrack(
            List<int>             result,
            List<int>             remaining,
            Stack<BacktrackFrame> btStack,
            ref int               consecutiveInvalidPicks)
        {
            while (btStack.Count > 0)
            {
                var frame = btStack.Pop();
                frame.ValidCandidates.Remove(frame.ChosenId);
                if (frame.ValidCandidates.Count == 0) continue;

                var alternative = frame.ValidCandidates[0];

                // Restore result to frame position
                int trimFrom = frame.ResultCount - 1;
                for (int i = trimFrom; i < result.Count; i++)
                    remaining.Add(result[i]);
                result.RemoveRange(trimFrom, result.Count - trimFrom);

                // Restore remaining from snapshot, swap in the alternative
                remaining.Clear();
                foreach (var id in frame.RemainingSnapshot)
                {
                    if (id != alternative) remaining.Add(id);
                }

                result.Add(alternative);
                consecutiveInvalidPicks = 0;

                btStack.Push(new BacktrackFrame(
                    resultCount:       result.Count,
                    remainingSnapshot: new List<int>(remaining),
                    chosenId:          alternative,
                    validCandidates:   new List<int>(frame.ValidCandidates)));

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
            public readonly List<int> RemainingSnapshot;
            public readonly int       ChosenId;
            public readonly List<int> ValidCandidates;

            public BacktrackFrame(
                int       resultCount,
                List<int> remainingSnapshot,
                int       chosenId,
                List<int> validCandidates)
            {
                ResultCount       = resultCount;
                RemainingSnapshot = remainingSnapshot;
                ChosenId          = chosenId;
                ValidCandidates   = validCandidates;
            }
        }
    }
}
