using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Puzzle;

namespace SimpleGame.Tests.Puzzle
{
    /// <summary>
    /// EditMode tests for <see cref="PuzzleModel"/>.
    ///
    /// Test topology used by most tests (2 slots):
    ///
    ///   [0] seed --- [1] --- [3]
    ///                |
    ///               [2]
    ///
    /// Piece 0: seed, neighbours [1]
    /// Piece 1: neighbours [0, 2, 3]
    /// Piece 2: neighbours [1]
    /// Piece 3: neighbours [1]
    ///
    /// Deck: [1, 2, 3]
    /// Slots at start: slot 0 = 1, slot 1 = 2  (deck cursor now at 3)
    /// </summary>
    [TestFixture]
    internal class PuzzleModelTests
    {
        // ── Helpers ───────────────────────────────────────────────────────

        private static IReadOnlyList<IPuzzlePiece> DefaultPieces() => new List<IPuzzlePiece>
        {
            new PuzzlePiece(0, new[] { 1 }),
            new PuzzlePiece(1, new[] { 0, 2, 3 }),
            new PuzzlePiece(2, new[] { 1 }),
            new PuzzlePiece(3, new[] { 1 }),
        };

        private static PuzzleModel BuildDefault(int slotCount = 2) =>
            new PuzzleModel(DefaultPieces(), new[] { 0 }, new[] { 1, 2, 3 }, slotCount);

        // ── Construction ──────────────────────────────────────────────────

        [Test]
        public void Construction_SeedPrePlaced_SlotsFilled()
        {
            var model = BuildDefault(slotCount: 2);
            // Seeds not exposed as progress — confirm initial slot state
            Assert.AreEqual(1, model.GetSlot(0), "slot 0 should hold piece 1");
            Assert.AreEqual(2, model.GetSlot(1), "slot 1 should hold piece 2");
        }

        [Test]
        public void Construction_PlacedCount_IsZero()
        {
            var model = BuildDefault();
            Assert.AreEqual(0, model.PlacedCount, "No non-seed pieces placed yet");
        }

        [Test]
        public void Construction_TotalNonSeedCount_MatchesDeck()
        {
            var model = BuildDefault();
            Assert.AreEqual(3, model.TotalNonSeedCount);
        }

        [Test]
        public void Construction_IsComplete_False()
        {
            var model = BuildDefault();
            Assert.IsFalse(model.IsComplete);
        }

        // ── TryPlace — Empty ──────────────────────────────────────────────

        [Test]
        public void TryPlace_OutOfRange_ReturnsEmpty()
        {
            var model = BuildDefault();
            Assert.AreEqual(SlotTapResult.Empty, model.TryPlace(99));
        }

        [Test]
        public void TryPlace_NegativeIndex_ReturnsEmpty()
        {
            var model = BuildDefault();
            Assert.AreEqual(SlotTapResult.Empty, model.TryPlace(-1));
        }

        [Test]
        public void TryPlace_EmptySlot_ReturnsEmpty()
        {
            // Build a single-slot model with a 1-piece deck so the slot becomes null after placement
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1 }),
                new PuzzlePiece(1, new[] { 0 }),
            };
            var model = new PuzzleModel(pieces, new[] { 0 }, new[] { 1 }, slotCount: 1);
            model.TryPlace(0); // places piece 1, deck exhausted → slot 0 becomes null

            Assert.AreEqual(SlotTapResult.Empty, model.TryPlace(0), "Exhausted slot should return Empty");
        }

        // ── TryPlace — Rejected ───────────────────────────────────────────

        [Test]
        public void TryPlace_UnplaceablePiece_ReturnsRejected()
        {
            // slot 1 holds piece 2; piece 2 neighbours piece 1 which is NOT on board yet
            var model = BuildDefault(slotCount: 2);
            Assert.AreEqual(SlotTapResult.Rejected, model.TryPlace(1));
        }

        [Test]
        public void TryPlace_Rejected_SlotContentUnchanged()
        {
            var model = BuildDefault(slotCount: 2);
            model.TryPlace(1); // rejected — slot 1 holds piece 2
            Assert.AreEqual(2, model.GetSlot(1), "Slot content must not change on rejection");
        }

        [Test]
        public void TryPlace_Rejected_PlacedCountUnchanged()
        {
            var model = BuildDefault(slotCount: 2);
            model.TryPlace(1); // rejected
            Assert.AreEqual(0, model.PlacedCount);
        }

        [Test]
        public void TryPlace_Rejected_OnRejectedFires()
        {
            var model = BuildDefault(slotCount: 2);
            int firedSlot = -1;
            int firedPiece = -1;
            model.OnRejected += (s, p) => { firedSlot = s; firedPiece = p; };

            model.TryPlace(1); // slot 1 = piece 2, rejected

            Assert.AreEqual(1, firedSlot);
            Assert.AreEqual(2, firedPiece);
        }

        // ── TryPlace — Placed ─────────────────────────────────────────────

        [Test]
        public void TryPlace_PlaceablePiece_ReturnsPlaced()
        {
            var model = BuildDefault(slotCount: 2);
            // slot 0 holds piece 1 which neighbours seed 0 → placeable
            Assert.AreEqual(SlotTapResult.Placed, model.TryPlace(0));
        }

        [Test]
        public void TryPlace_Placed_SlotRefillsFromDeck()
        {
            var model = BuildDefault(slotCount: 2);
            // deck after construction: cursor at piece 3 (1 and 2 already in slots)
            model.TryPlace(0); // places piece 1, slot 0 draws piece 3
            Assert.AreEqual(3, model.GetSlot(0), "Slot 0 should now hold piece 3");
        }

        [Test]
        public void TryPlace_Placed_PlacedCountIncrements()
        {
            var model = BuildDefault(slotCount: 2);
            model.TryPlace(0); // piece 1 placed
            Assert.AreEqual(1, model.PlacedCount);
        }

        [Test]
        public void TryPlace_Placed_OnPiecePlacedFires()
        {
            var model = BuildDefault(slotCount: 2);
            int firedPiece = -1;
            model.OnPiecePlaced += p => firedPiece = p;
            model.TryPlace(0);
            Assert.AreEqual(1, firedPiece);
        }

        [Test]
        public void TryPlace_Placed_OnSlotChangedFiresWithNewContent()
        {
            var model = BuildDefault(slotCount: 2);
            int firedSlot = -1;
            int? firedPiece = -1;
            model.OnSlotChanged += (s, p) => { firedSlot = s; firedPiece = p; };
            model.TryPlace(0); // slot 0 refills with piece 3
            Assert.AreEqual(0, firedSlot);
            Assert.AreEqual(3, firedPiece);
        }

        // ── Multi-slot independence ───────────────────────────────────────

        [Test]
        public void TryPlace_Slot0_DoesNotChangeSlot1()
        {
            var model = BuildDefault(slotCount: 2);
            // slot 1 = piece 2 before tap
            model.TryPlace(0); // tap slot 0
            Assert.AreEqual(2, model.GetSlot(1), "Slot 1 must be unaffected by tapping slot 0");
        }

        // ── Deck exhaustion ───────────────────────────────────────────────

        [Test]
        public void TryPlace_DeckExhausted_SlotBecomesNull()
        {
            // 1 slot, deck = [1, 2, 3] — 3 non-seed pieces
            var model = BuildDefault(slotCount: 1);
            // slot 0 = 1; after placing 1, slot 0 = 2; after placing 2, slot 0 = 3; after placing 3, deck empty
            model.TryPlace(0); // places 1, slot → 2
            model.TryPlace(0); // places 2, slot → 3
            model.TryPlace(0); // places 3, deck exhausted → slot → null

            Assert.IsNull(model.GetSlot(0), "Slot should be null when deck is exhausted");
        }

        [Test]
        public void TryPlace_DeckExhausted_OnSlotChangedFiresWithNull()
        {
            var model = BuildDefault(slotCount: 1);
            model.TryPlace(0); // places 1, slot → 2
            model.TryPlace(0); // places 2, slot → 3

            int? lastPiece = -1;
            model.OnSlotChanged += (_, p) => lastPiece = p;
            model.TryPlace(0); // places 3, deck exhausted → null

            Assert.IsNull(lastPiece);
        }

        // ── Win condition ─────────────────────────────────────────────────

        [Test]
        public void AllPiecesPlaced_IsCompleteTrue()
        {
            var model = BuildDefault(slotCount: 1);
            model.TryPlace(0); // 1
            model.TryPlace(0); // 2
            model.TryPlace(0); // 3
            Assert.IsTrue(model.IsComplete);
        }

        [Test]
        public void AllPiecesPlaced_OnCompletedFires()
        {
            var model = BuildDefault(slotCount: 1);
            bool fired = false;
            model.OnCompleted += () => fired = true;
            model.TryPlace(0);
            model.TryPlace(0);
            model.TryPlace(0);
            Assert.IsTrue(fired);
        }

        [Test]
        public void OnCompleted_FiresExactlyOnce()
        {
            var model = BuildDefault(slotCount: 1);
            int fireCount = 0;
            model.OnCompleted += () => fireCount++;
            model.TryPlace(0);
            model.TryPlace(0);
            model.TryPlace(0);
            // Try to trigger again — should not fire
            model.TryPlace(0); // empty slot
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void AllPiecesPlaced_PlacedCountEqualsTotal()
        {
            var model = BuildDefault(slotCount: 1);
            model.TryPlace(0);
            model.TryPlace(0);
            model.TryPlace(0);
            Assert.AreEqual(model.TotalNonSeedCount, model.PlacedCount);
        }
    }
}
