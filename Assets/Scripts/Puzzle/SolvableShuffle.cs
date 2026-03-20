using System;
using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Builds a deck ordering that guarantees solvability by construction.
    ///
    /// <para><b>Guarantee:</b> At every index <c>i</c> in the returned list,
    /// at least one piece in the window <c>[i .. i + slotCount - 1]</c> has a
    /// placed neighbour, given the simulated board state at that point.
    /// The simulated board state advances by placing every valid piece it
    /// encounters as the window slides forward.</para>
    ///
    /// <para><b>Anti-trivialisation:</b> When <c>slotCount &gt; 1</c> and both
    /// placeable and non-placeable candidates exist, the algorithm occasionally
    /// places a non-placeable piece at the front of the window, provided a
    /// placeable piece remains within the window. This produces windows that are
    /// not trivially all-valid.</para>
    ///
    /// <para><b>Backtracking:</b> If no valid candidate exists for the current
    /// window, the algorithm backtracks up to <see cref="MaxBacktrackSteps"/>
    /// positions before falling back to best-effort appending.</para>
    /// </summary>
    public static class SolvableShuffle
    {
        /// <summary>Maximum positions to unwind during a single backtrack event.</summary>
        public const int MaxBacktrackSteps = 50;

        /// <summary>
        /// Builds a solvable deck ordering from the supplied puzzle graph.
        /// </summary>
        /// <param name="seedIds">
        /// Piece IDs pre-placed at game start. These are NOT included in the result.
        /// </param>
        /// <param name="pieces">
        /// All pieces in the puzzle, including seeds. Used to build the neighbour map.
        /// </param>
        /// <param name="slotCount">
        /// Number of independent player slots. Determines the lookahead window size.
        /// </param>
        /// <param name="rng">Random number generator. Caller controls the seed.</param>
        /// <returns>
        /// An ordered list of non-seed piece IDs. At every window of
        /// <paramref name="slotCount"/> consecutive positions at least one piece
        /// is placeable given the simulated board state at that point.
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

            // Fisher-Yates shuffle as the starting candidate order —
            // anti-trivialisation base: start from a random permutation.
            FisherYates(remaining, rng);

            // ── State ─────────────────────────────────────────────────────
            // `placed`    — pieces on the simulated board (seeds + placed deck pieces)
            // `result`    — the deck we are building
            // `remaining` — pieces not yet assigned a deck position
            var placed = new HashSet<int>(seedIds);
            var result = new List<int>(remaining.Count);
            var btStack = new Stack<BacktrackFrame>();

            // Tracks how many consecutive invalid (non-placeable) pieces have been
            // emitted since the last valid pick. Must not reach slotCount, because
            // that would mean an entire window of slotCount positions contains no
            // valid pick — violating the solvability guarantee.
            int consecutiveInvalidPicks = 0;

            while (remaining.Count > 0)
            {
                // ── Classify ──────────────────────────────────────────────
                var valid   = new List<int>(); // placeable given current board
                var invalid = new List<int>(); // not yet placeable

                foreach (var id in remaining)
                {
                    if (IsPlaceable(id, placed, neighbours))
                        valid.Add(id);
                    else
                        invalid.Add(id);
                }

                // ── Solvability check ─────────────────────────────────────
                // Need at least one valid candidate within the next slotCount
                // positions. Because we place valid picks immediately (simulating
                // optimal player action), having ANY valid candidate satisfies
                // the window — the player will place it.
                if (valid.Count == 0)
                {
                    if (!TryBacktrack(result, remaining, placed, btStack, ref consecutiveInvalidPicks))
                    {
                        // Exhausted — best-effort append
                        result.AddRange(remaining);
                        remaining.Clear();
                    }
                    continue;
                }

                // ── Anti-trivialisation ───────────────────────────────────
                // With slotCount > 1 and both valid + invalid candidates available,
                // occasionally emit an invalid piece at this position.
                //
                // Safety constraints:
                // 1. consecutiveInvalidPicks must stay below slotCount - 1.
                // 2. The invalid piece must be "soon-unlockable": at least one of
                //    its neighbors must currently be in `valid` (i.e. will be placed
                //    within this same window round, unlocking the invalid piece).
                //    This prevents placing a piece that cannot be unlocked within
                //    the current slot window.
                //
                // Probability ~40% per eligible position.
                int chosen;
                bool isInvalidPick = false;

                // Build the set of soon-unlockable invalid candidates
                var validSet              = new HashSet<int>(valid);
                var unlockableInvalid     = new List<int>();
                foreach (var id in invalid)
                {
                    if (!neighbours.TryGetValue(id, out var inbrs)) continue;
                    foreach (var nbr in inbrs)
                    {
                        if (validSet.Contains(nbr)) { unlockableInvalid.Add(id); break; }
                    }
                }

                bool canPickInvalid =
                    slotCount > 1
                    && unlockableInvalid.Count > 0
                    && consecutiveInvalidPicks < slotCount - 1
                    && remaining.Count > slotCount - 1 - consecutiveInvalidPicks
                    && rng.NextDouble() < 0.40;

                if (canPickInvalid)
                {
                    chosen = unlockableInvalid[rng.Next(unlockableInvalid.Count)];
                    isInvalidPick = true;
                    consecutiveInvalidPicks++;
                }
                else
                {
                    chosen = valid[rng.Next(valid.Count)];
                    consecutiveInvalidPicks = 0;
                }

                // Commit this pick
                result.Add(chosen);
                remaining.Remove(chosen);

                if (!isInvalidPick)
                {
                    // Valid pick: add to placed — this may unlock further pieces.
                    placed.Add(chosen);

                    // Cascade: any invalid piece already committed to the result
                    // that is NOW placeable (because `chosen` was their missing
                    // neighbour) should also be added to `placed`. These pieces
                    // are already in slots — once their neighbour is placed the
                    // player will place them, so they are effectively on the board.
                    // We keep cascading until no new pieces can be resolved.
                    bool cascading = true;
                    while (cascading)
                    {
                        cascading = false;
                        foreach (var id in result)
                        {
                            if (!placed.Contains(id) && IsPlaceable(id, placed, neighbours))
                            {
                                placed.Add(id);
                                cascading = true;
                            }
                        }
                    }

                    // Push a backtrack frame so we can retry with a different
                    // valid candidate if a deadlock is discovered later.
                    if (btStack.Count < MaxBacktrackSteps)
                    {
                        btStack.Push(new BacktrackFrame(
                            resultCount:       result.Count,
                            placedSnapshot:    new HashSet<int>(placed),
                            remainingSnapshot: new List<int>(remaining),
                            chosenId:          chosen,
                            validCandidates:   new List<int>(valid)));
                    }
                }
                // Invalid pick: piece is not added to `placed` —
                // it will sit in its slot until a neighbour is placed,
                // at which point the player can place it. The simulation
                // does not need to track this because the window guarantee
                // is satisfied by the valid piece that will come next.
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static bool IsPlaceable(
            int                           id,
            HashSet<int>                  placed,
            Dictionary<int, List<int>>    neighbours)
        {
            if (!neighbours.TryGetValue(id, out var nbrs)) return false;
            foreach (var nbr in nbrs)
            {
                if (placed.Contains(nbr)) return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to unwind to the most recent backtrack frame and try a
        /// different valid candidate there. Returns true if recovery succeeded.
        /// </summary>
        private static bool TryBacktrack(
            List<int>              result,
            List<int>              remaining,
            HashSet<int>           placed,
            Stack<BacktrackFrame>  btStack,
            ref int                consecutiveInvalidPicks)
        {
            while (btStack.Count > 0)
            {
                var frame = btStack.Pop();

                // Drop the candidate we already tried
                frame.ValidCandidates.Remove(frame.ChosenId);

                if (frame.ValidCandidates.Count == 0)
                    continue; // no alternatives at this position — pop further

                // Pick the first remaining alternative
                // (list was already shuffled at construction time)
                var alternative = frame.ValidCandidates[0];

                // Restore result and remaining to the frame's snapshot
                int trimFrom = frame.ResultCount - 1; // -1 because frame was pushed AFTER adding chosen
                for (int i = trimFrom; i < result.Count; i++)
                    remaining.Add(result[i]);
                result.RemoveRange(trimFrom, result.Count - trimFrom);

                // Restore placed
                placed.Clear();
                foreach (var id in frame.PlacedSnapshot) placed.Add(id);
                // Remove the original choice from placed (it was added before the frame was pushed)
                placed.Remove(frame.ChosenId);
                placed.Add(alternative);

                // Restore remaining from snapshot, excluding the new choice
                remaining.Clear();
                foreach (var id in frame.RemainingSnapshot)
                {
                    if (id != alternative)
                        remaining.Add(id);
                }

                result.Add(alternative);

                // Backtrack succeeded — the restored state is at a valid pick,
                // so reset the consecutive invalid counter.
                consecutiveInvalidPicks = 0;

                // Push a revised frame for the new choice
                btStack.Push(new BacktrackFrame(
                    resultCount:       result.Count,
                    placedSnapshot:    new HashSet<int>(placed),
                    remainingSnapshot: new List<int>(remaining),
                    chosenId:          alternative,
                    validCandidates:   new List<int>(frame.ValidCandidates)));

                return true;
            }

            return false; // stack exhausted
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
            public readonly int          ResultCount;
            public readonly HashSet<int> PlacedSnapshot;
            public readonly List<int>    RemainingSnapshot;
            public readonly int          ChosenId;
            public readonly List<int>    ValidCandidates;

            public BacktrackFrame(
                int          resultCount,
                HashSet<int> placedSnapshot,
                List<int>    remainingSnapshot,
                int          chosenId,
                List<int>    validCandidates)
            {
                ResultCount       = resultCount;
                PlacedSnapshot    = placedSnapshot;
                RemainingSnapshot = remainingSnapshot;
                ChosenId          = chosenId;
                ValidCandidates   = validCandidates;
            }
        }
    }
}
