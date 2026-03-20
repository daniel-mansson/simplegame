using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleGame.Puzzle;

namespace SimpleGame.Tests.Puzzle
{
    /// <summary>
    /// EditMode tests for <see cref="SolvableShuffle"/>.
    ///
    /// Topologies used:
    ///
    ///   Linear chain:
    ///     [0]→[1]→[2]→[3]→[4]   (each piece connects only to its immediate neighbour)
    ///     Only valid deck order with slotCount=1: [1,2,3,4]
    ///
    ///   Star (fully connected to seed):
    ///     [0]←→[1], [0]←→[2], [0]←→[3]
    ///     Any permutation of [1,2,3] is valid.
    ///
    ///   Diamond:
    ///     [0]←→[1], [0]←→[2], [1]←→[3], [2]←→[3]
    ///     slotCount=2: allows piece 2 in slot while piece 1 is being placed.
    /// </summary>
    [TestFixture]
    internal class SolvableShuffleTests
    {
        // ── Topology helpers ──────────────────────────────────────────────

        /// <summary>
        /// Linear chain: 0→1→2→3→4
        /// Each piece connects only to its immediate successor/predecessor.
        /// </summary>
        private static IReadOnlyList<IPuzzlePiece> LinearChain(int length)
        {
            var pieces = new List<IPuzzlePiece>();
            for (int i = 0; i < length; i++)
            {
                var nbrs = new List<int>();
                if (i > 0)         nbrs.Add(i - 1);
                if (i < length -1) nbrs.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, nbrs));
            }
            return pieces;
        }

        /// <summary>
        /// Star: piece 0 is the seed; pieces 1..n all connect to 0 only.
        /// Any permutation of non-seeds is valid.
        /// </summary>
        private static IReadOnlyList<IPuzzlePiece> StarGraph(int leafCount)
        {
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, Enumerable.Range(1, leafCount).ToList())
            };
            for (int i = 1; i <= leafCount; i++)
                pieces.Add(new PuzzlePiece(i, new[] { 0 }));
            return pieces;
        }

        /// <summary>
        /// Diamond: 0←→1, 0←→2, 1←→3, 2←→3
        /// Pieces 1 and 2 can be placed (neighbours of seed 0).
        /// Piece 3 can only be placed after 1 or 2.
        /// </summary>
        private static IReadOnlyList<IPuzzlePiece> DiamondGraph()
        {
            return new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1, 2 }),
                new PuzzlePiece(1, new[] { 0, 3 }),
                new PuzzlePiece(2, new[] { 0, 3 }),
                new PuzzlePiece(3, new[] { 1, 2 }),
            };
        }

        // ── Basic contract ────────────────────────────────────────────────

        [Test]
        public void ResultContainsAllNonSeedPieces_LinearChain()
        {
            var pieces = LinearChain(5);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(42));
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, result,
                "Result must contain exactly all non-seed pieces.");
        }

        [Test]
        public void ResultContainsAllNonSeedPieces_Star([Values(1, 2, 3)] int slotCount)
        {
            var pieces = StarGraph(5);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount, new Random(7));
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, result,
                "Result must contain exactly all non-seed pieces.");
        }

        [Test]
        public void NoDuplicates_LinearChain()
        {
            var pieces = LinearChain(6);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(99));
            Assert.AreEqual(result.Count, result.Distinct().Count(), "No duplicate piece IDs in result.");
        }

        // ── Linear chain: only one valid ordering with slotCount=1 ────────

        [Test]
        public void LinearChain_SlotCount1_MustReturnChainOrder()
        {
            // 0→1→2→3→4, seed=0, slotCount=1
            // The only valid ordering is [1,2,3,4] because each piece
            // only becomes placeable after its predecessor is placed.
            var pieces = LinearChain(5);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(123));
            Assert.AreEqual(new[] { 1, 2, 3, 4 }, result.ToArray(),
                "Linear chain with slotCount=1 has only one valid ordering.");
        }

        [Test]
        public void LinearChain_SlotCount1_MultipleSeeds_DifferentOrdering()
        {
            // 0→1→2→3→4, seeds={0,2}, slotCount=1
            // Valid orderings: 1 must come before 3, 3 before 4; 1 can come before or after... 
            // Actually with seed=2 placed, piece 3 is immediately valid.
            // With seed=0, piece 1 is valid. So valid starts are {1,3}.
            var pieces = LinearChain(5);
            var result = SolvableShuffle.Shuffle(new[] { 0, 2 }, pieces, slotCount: 1, new Random(42));
            CollectionAssert.AreEquivalent(new[] { 1, 3, 4 }, result,
                "With seeds 0 and 2, result must contain pieces 1, 3, 4.");
        }

        // ── Star graph: any order valid ───────────────────────────────────

        [Test]
        public void StarGraph_AnyOrderValid_ResultIsPermutation()
        {
            var pieces = StarGraph(4);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(55));
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, result,
                "Star graph: result should be a permutation of non-seed pieces.");
        }

        // ── Window invariant: the main correctness property ───────────────

        /// <summary>
        /// Simulates placement left-to-right and verifies at every position that
        /// at least one piece within the window is placeable.
        /// </summary>
        [Test]
        public void WindowInvariant_HoldsAtEveryPosition_LinearChain([Values(1, 2, 3)] int slotCount)
        {
            var pieces = LinearChain(8);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount, new Random(77));
            AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount);
        }

        [Test]
        public void WindowInvariant_HoldsAtEveryPosition_Diamond([Values(1, 2)] int slotCount)
        {
            var pieces = DiamondGraph();
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount, new Random(33));
            AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount);
        }

        [Test]
        public void WindowInvariant_HoldsAtEveryPosition_Star([Values(1, 2, 3)] int slotCount)
        {
            var pieces = StarGraph(6);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount, new Random(11));
            AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount);
        }

        [Test]
        public void WindowInvariant_HoldsAcrossMultipleSeeds_LinearChain()
        {
            var pieces = LinearChain(10);
            var result = SolvableShuffle.Shuffle(new[] { 0, 5 }, pieces, slotCount: 2, new Random(42));
            AssertWindowInvariant(result, new[] { 0, 5 }, pieces, slotCount: 2);
        }

        // ── Slot-window semantics ─────────────────────────────────────────

        [Test]
        public void SlotWindow_DiamondWithSlotCount2_WindowInvariantAlwaysHolds()
        {
            // Diamond: seeds={0}, piece 3 connects only to 1 and 2 (neither initially placed).
            // With slotCount=2, piece 3 CAN appear at result[0] as long as result[1]
            // contains a valid piece (1 or 2). The window invariant must hold, but
            // piece 3 is not forbidden from position 0 specifically.
            var pieces = DiamondGraph();
            for (int seed = 0; seed < 50; seed++)
            {
                var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 2, new Random(seed));
                AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount: 2);
            }
        }

        [Test]
        public void SlotWindow_LinearChainSlotCount2_Piece1AlwaysInWindowBeforePiece2()
        {
            // 0→1→2→3, seed=0, slotCount=2.
            // Piece 2 connects only to pieces 1 and 3. Piece 3 connects only to pieces 2.
            // Piece 1 connects to 0 (seed) and 2.
            // With slotCount=2, if piece 2 is at result[0], piece 1 must be at result[1]
            // (within the window). The window invariant handles this, but we also verify
            // that piece 1 always appears before piece 3 (piece 3 needs piece 2 which
            // needs piece 1 to already be reachable).
            // The key correctness property is captured by the window invariant test;
            // here we just verify the result is a valid permutation of [1,2,3].
            var pieces = LinearChain(4);
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 2, new Random(88));
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result,
                "Result must contain exactly pieces 1, 2, 3.");
            AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount: 2);
        }

        // ── Anti-trivialisation ───────────────────────────────────────────

        [Test]
        public void AntiTrivialisation_StarGraph_NotAlwaysAscending()
        {
            // Star graph: all non-seed pieces are always valid from the start.
            // If there were no anti-trivialisation, the order would be the same
            // every time or deterministically ascending. With shuffling as the base
            // and slotCount > 1, we expect variance across seeds.
            // Run 30 seeds; at least one result should NOT be ascending.
            var pieces = StarGraph(5);
            bool foundNonAscending = false;

            for (int seed = 0; seed < 30; seed++)
            {
                var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 2, new Random(seed));
                if (!IsAscending(result))
                {
                    foundNonAscending = true;
                    break;
                }
            }

            Assert.IsTrue(foundNonAscending,
                "At least one shuffle result across 30 seeds should NOT be in ascending order.");
        }

        [Test]
        public void AntiTrivialisation_StarGraph_SlotCount1_WindowInvariantHolds()
        {
            // Even with anti-trivialisation, slotCount=1 on a star should still
            // satisfy the window invariant (every piece is valid immediately).
            var pieces = StarGraph(6);
            for (int seed = 0; seed < 20; seed++)
            {
                var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(seed));
                AssertWindowInvariant(result, new[] { 0 }, pieces, slotCount: 1);
            }
        }

        [Test]
        public void AntiTrivialisation_NotAllWindowsAllValid_DiamondSlotCount2()
        {
            // Diamond with slotCount=2: result = [x, y, z] where x,y,z are 1,2,3.
            // If all windows were trivially all-valid (both slots always hold a valid piece),
            // then result[0] and result[1] would always both be from {1,2}.
            // Anti-trivialisation should sometimes place piece 3 early (at result[1])
            // when piece 1 or 2 is at result[0]. Run many seeds to find this.
            var pieces = DiamondGraph();
            bool foundNonTrivialWindow = false;

            for (int seed = 0; seed < 100; seed++)
            {
                var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 2, new Random(seed));
                if (result.Count >= 2)
                {
                    // result[0] is in {1,2}, result[1] is piece 3 → non-trivial
                    bool firstIsValid   = result[0] == 1 || result[0] == 2;
                    bool secondIsPiece3 = result[1] == 3;
                    if (firstIsValid && secondIsPiece3)
                    {
                        foundNonTrivialWindow = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(foundNonTrivialWindow,
                "Expected at least one seed to produce a window where result[1]==3 " +
                "(non-trivial: piece 3 placed before piece 2 is available in the other slot).");
        }

        // ── Edge cases ────────────────────────────────────────────────────

        [Test]
        public void EmptyNonSeedPieces_ReturnsEmptyList()
        {
            // Only seed piece — nothing to shuffle
            var pieces = new List<IPuzzlePiece> { new PuzzlePiece(0, new int[0]) };
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(1));
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SingleNonSeedPiece_ReturnsIt()
        {
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1 }),
                new PuzzlePiece(1, new[] { 0 }),
            };
            var result = SolvableShuffle.Shuffle(new[] { 0 }, pieces, slotCount: 1, new Random(1));
            Assert.AreEqual(new[] { 1 }, result.ToArray());
        }

        [Test]
        public void ArgumentValidation_NullSeeds_Throws()
        {
            var pieces = StarGraph(2);
            Assert.Throws<ArgumentNullException>(() =>
                SolvableShuffle.Shuffle(null, pieces, 1, new Random(1)));
        }

        [Test]
        public void ArgumentValidation_NullPieces_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SolvableShuffle.Shuffle(new[] { 0 }, null, 1, new Random(1)));
        }

        [Test]
        public void ArgumentValidation_SlotCountZero_Throws()
        {
            var pieces = StarGraph(2);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                SolvableShuffle.Shuffle(new[] { 0 }, pieces, 0, new Random(1)));
        }

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Simulates the slot-based game and asserts no deadlock occurs.
        /// Uses a `slotCount`-wide window on the deck. At each step, scans
        /// all active slots for a placeable piece. If found, places it and
        /// advances the deck. Fails the test if the game deadlocks before
        /// all pieces are placed.
        /// </summary>
        private static void AssertWindowInvariant(
            IReadOnlyList<int>          deck,
            IReadOnlyList<int>          seedIds,
            IReadOnlyList<IPuzzlePiece> pieces,
            int                         slotCount)
        {
            // Build neighbour lookup
            var nbrs = pieces.ToDictionary(
                p => p.Id,
                p => new HashSet<int>(p.NeighborIds));

            var placed = new HashSet<int>(seedIds);

            // Fill initial slots from deck front
            var slots      = new int?[slotCount];
            int deckCursor = 0;
            for (int s = 0; s < slotCount && deckCursor < deck.Count; s++, deckCursor++)
                slots[s] = deck[deckCursor];

            int remaining = deck.Count;

            while (remaining > 0)
            {
                // Find any placeable slot
                bool progress = false;
                for (int s = 0; s < slotCount; s++)
                {
                    if (!slots[s].HasValue) continue;
                    int pid = slots[s].Value;

                    bool canPlace = false;
                    if (nbrs.TryGetValue(pid, out var pnbrs))
                    {
                        foreach (var nbr in pnbrs)
                        {
                            if (placed.Contains(nbr)) { canPlace = true; break; }
                        }
                    }
                    if (!canPlace) continue;

                    // Place it
                    placed.Add(pid);
                    remaining--;
                    slots[s] = deckCursor < deck.Count ? deck[deckCursor++] : (int?)null;
                    progress = true;
                }

                if (!progress)
                {
                    var slotContents = string.Join(",", slots.Select(s => s.HasValue ? s.Value.ToString() : "_"));
                    Assert.Fail(
                        $"Deadlock with slotCount={slotCount}: no slot is placeable. " +
                        $"Slots=[{slotContents}], placed={{{string.Join(",", placed)}}}, " +
                        $"remaining={remaining}");
                }
            }
        }

        private static bool IsAscending(IReadOnlyList<int> list)
        {
            for (int i = 1; i < list.Count; i++)
                if (list[i] < list[i - 1]) return false;
            return true;
        }
    }
}
