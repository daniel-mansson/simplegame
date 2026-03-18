using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Puzzle;

namespace SimpleGame.Tests.Puzzle
{
    /// <summary>
    /// EditMode tests for the pure puzzle domain model.
    /// No Unity types — all plain C#.
    ///
    /// Test board topology (used in most tests):
    ///
    ///   [0] seed --- [1] --- [3]
    ///                |
    ///               [2]
    ///
    /// Piece 0: seed, neighbors [1]
    /// Piece 1: neighbors [0, 2, 3]
    /// Piece 2: neighbors [1]
    /// Piece 3: neighbors [1]
    /// Deck: [1, 2, 3] (order: place 1 first, then 2, then 3)
    /// </summary>
    [TestFixture]
    internal class PuzzleDomainTests
    {
        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        private static IPuzzleLevel BuildDefaultLevel()
        {
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1 }),        // seed
                new PuzzlePiece(1, new[] { 0, 2, 3 }),
                new PuzzlePiece(2, new[] { 1 }),
                new PuzzlePiece(3, new[] { 1 }),
            };
            var seeds = new[] { 0 };
            var deck = new Deck(new[] { 1, 2, 3 });
            return new PuzzleLevel(pieces, seeds, new IDeck[] { deck });
        }

        private static PuzzleSession BuildDefaultSession() => new PuzzleSession(BuildDefaultLevel());

        // ---------------------------------------------------------------------------
        // Seed placement
        // ---------------------------------------------------------------------------

        [Test]
        public void SeedPiecesArePlacedOnConstruction()
        {
            var session = BuildDefaultSession();
            Assert.That(session.PlacedIds, Contains.Item(0),
                "Seed piece 0 must be placed before the first TryPlace call.");
        }

        [Test]
        public void NonSeedPiecesAreNotPlacedOnConstruction()
        {
            var session = BuildDefaultSession();
            Assert.That(session.PlacedIds, Has.Count.EqualTo(1),
                "Only seed pieces should be pre-placed.");
        }

        // ---------------------------------------------------------------------------
        // Placement rule
        // ---------------------------------------------------------------------------

        [Test]
        public void NonSeedWithNoNeighborOnBoardIsRejected()
        {
            // Isolated piece with no neighbors at all — can never be placed via CanPlace
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1 }),  // seed
                new PuzzlePiece(1, new[] { 0 }),   // neighbor of seed — placeable
                new PuzzlePiece(2, new int[0]),    // isolated — no neighbors
            };
            var deck = new Deck(new[] { 2, 1 });
            var level = new PuzzleLevel(pieces, new[] { 0 }, new IDeck[] { deck });
            var session = new PuzzleSession(level);

            var result = session.TryPlace(2);
            Assert.That(result, Is.EqualTo(PlacementResult.Rejected));
        }

        [Test]
        public void NonSeedWithPlacedNeighborIsAccepted()
        {
            var session = BuildDefaultSession();
            // Piece 1 neighbors seed 0 which is already placed
            var result = session.TryPlace(1);
            Assert.That(result, Is.EqualTo(PlacementResult.Placed));
        }

        [Test]
        public void PlacingPieceAddsToBoardPlacedIds()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);
            Assert.That(session.PlacedIds, Contains.Item(1));
        }

        [Test]
        public void PieceNotNeighboringAnyPlacedPieceIsRejected()
        {
            var session = BuildDefaultSession();
            // Piece 3 neighbors only piece 1, which is NOT yet placed
            var result = session.TryPlace(3);
            Assert.That(result, Is.EqualTo(PlacementResult.Rejected));
        }

        [Test]
        public void AlreadyPlacedPieceReturnsAlreadyPlaced()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);         // first time — Placed
            var result = session.TryPlace(1);  // second time — AlreadyPlaced
            Assert.That(result, Is.EqualTo(PlacementResult.AlreadyPlaced));
        }

        [Test]
        public void AlreadyPlacedPieceDoesNotMutateBoardFurther()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);
            int countBefore = session.PlacedIds.Count;
            session.TryPlace(1);
            Assert.That(session.PlacedIds.Count, Is.EqualTo(countBefore));
        }

        // ---------------------------------------------------------------------------
        // Deck
        // ---------------------------------------------------------------------------

        [Test]
        public void PlacingPieceAdvancesDeck()
        {
            var session = BuildDefaultSession();
            Assert.That(session.CurrentDeckPiece(0), Is.EqualTo(1), "Deck should start at piece 1.");
            session.TryPlace(1);
            Assert.That(session.CurrentDeckPiece(0), Is.EqualTo(2), "After placing 1, deck front should advance to 2.");
        }

        [Test]
        public void RejectingPieceDoesNotAdvanceDeck()
        {
            var session = BuildDefaultSession();
            int? before = session.CurrentDeckPiece(0);
            session.TryPlace(3); // rejected
            Assert.That(session.CurrentDeckPiece(0), Is.EqualTo(before),
                "Deck must not advance on a rejected placement.");
        }

        [Test]
        public void DeckIsEmptyAfterAllPiecesAdvanced()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);
            session.TryPlace(2);
            session.TryPlace(3);
            Assert.That(session.CurrentDeckPiece(0), Is.Null, "Deck should be empty after all pieces placed.");
        }

        // ---------------------------------------------------------------------------
        // Win condition
        // ---------------------------------------------------------------------------

        [Test]
        public void IsCompleteWhenAllPiecesPlaced()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);
            session.TryPlace(2);
            session.TryPlace(3);
            Assert.That(session.IsComplete, Is.True);
        }

        [Test]
        public void IsNotCompleteWithRemainingPieces()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);
            Assert.That(session.IsComplete, Is.False);
        }

        // ---------------------------------------------------------------------------
        // OnPlacementResolved event
        // ---------------------------------------------------------------------------

        [Test]
        public void OnPlacementResolvedFiresOnCorrectPlacement()
        {
            var session = BuildDefaultSession();
            PlacementResult? fired = null;
            int? firedId = null;
            session.OnPlacementResolved += (id, result) => { firedId = id; fired = result; };

            session.TryPlace(1);

            Assert.That(fired, Is.EqualTo(PlacementResult.Placed));
            Assert.That(firedId, Is.EqualTo(1));
        }

        [Test]
        public void OnPlacementResolvedFiresOnRejection()
        {
            var session = BuildDefaultSession();
            PlacementResult? fired = null;
            session.OnPlacementResolved += (id, result) => fired = result;

            session.TryPlace(3); // rejected

            Assert.That(fired, Is.EqualTo(PlacementResult.Rejected));
        }

        [Test]
        public void OnPlacementResolvedFiresOnAlreadyPlaced()
        {
            var session = BuildDefaultSession();
            session.TryPlace(1);

            PlacementResult? fired = null;
            session.OnPlacementResolved += (id, result) => fired = result;
            session.TryPlace(1); // already placed

            Assert.That(fired, Is.EqualTo(PlacementResult.AlreadyPlaced));
        }

        // ---------------------------------------------------------------------------
        // Chain placement
        // ---------------------------------------------------------------------------

        [Test]
        public void MultiPieceChainPlacement()
        {
            // 0(seed) → 1 → 2 → 3 (each placed after its neighbor)
            var session = BuildDefaultSession();

            Assert.That(session.TryPlace(1), Is.EqualTo(PlacementResult.Placed),  "1 neighbors seed 0");
            Assert.That(session.TryPlace(2), Is.EqualTo(PlacementResult.Placed),  "2 neighbors 1 (now placed)");
            Assert.That(session.TryPlace(3), Is.EqualTo(PlacementResult.Placed),  "3 neighbors 1 (now placed)");
            Assert.That(session.IsComplete,  Is.True, "All pieces placed — session complete");
        }
    }
}
