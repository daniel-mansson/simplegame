using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleGame.Game.Puzzle;
using SimpleGame.Puzzle;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// EditMode tests for JigsawLevelFactory.
    /// Verifies the adapter correctly maps SimpleJigsaw.PuzzleBoard → IPuzzleLevel.
    ///
    /// 2×2 grid topology (IDs = row * cols + col):
    ///   [0][1]
    ///   [2][3]
    ///
    /// Adjacency: 0↔1, 0↔2, 1↔3, 2↔3
    /// Each piece has exactly 2 neighbors.
    /// </summary>
    [TestFixture]
    internal class JigsawAdapterTests
    {
        private SimpleJigsaw.GridLayoutConfig _config2x2;

        [SetUp]
        public void SetUp()
        {
            _config2x2 = ScriptableObject.CreateInstance<SimpleJigsaw.GridLayoutConfig>();
            _config2x2.Rows = 2;
            _config2x2.Columns = 2;
            _config2x2.EdgeProfile = null; // Flat edges — no profile needed for topology tests
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config2x2);
        }

        [Test]
        public void Build_2x2Grid_ReturnsFourPieces()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.Level.Pieces.Count, Is.EqualTo(4));
        }

        [Test]
        public void Build_2x2Grid_RawBoardIsReturned()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.RawBoard, Is.Not.Null);
            Assert.That(result.RawBoard.Pieces.Count, Is.EqualTo(4));
        }

        [Test]
        public void Build_2x2Grid_EachPieceHasTwoNeighbors()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            foreach (var piece in result.Level.Pieces)
            {
                Assert.That(piece.NeighborIds.Count, Is.EqualTo(2),
                    $"Piece {piece.Id} in a 2×2 grid should have exactly 2 neighbors.");
            }
        }

        [Test]
        public void Build_2x2Grid_NeighborRelationshipsAreSymmetric()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            var pieceById = result.Level.Pieces.ToDictionary(p => p.Id);

            foreach (var piece in result.Level.Pieces)
            {
                foreach (var neighborId in piece.NeighborIds)
                {
                    Assert.That(pieceById[neighborId].NeighborIds, Contains.Item(piece.Id),
                        $"Piece {neighborId} should list {piece.Id} as a neighbor (symmetric).");
                }
            }
        }

        [Test]
        public void Build_SeedIdsArePreservedInLevel()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.Level.SeedIds, Contains.Item(0));
        }

        [Test]
        public void Build_DefaultDeck_ContainsAllNonSeedPieces()
        {
            // No explicit deckOrders → auto-generated deck of non-seed pieces
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.Level.Decks.Count, Is.EqualTo(1));

            var deckPieces = new List<int>();
            var deck = result.Level.Decks[0];
            while (!deck.IsEmpty)
            {
                deckPieces.Add(deck.Peek().Value);
                deck.Advance();
            }

            Assert.That(deckPieces, Has.No.Member(0), "Seed piece should not be in the deck.");
            Assert.That(deckPieces.Count, Is.EqualTo(3), "Deck should contain the 3 non-seed pieces.");
            Assert.That(deckPieces, Is.Ordered, "Default deck should be in ascending ID order.");
        }

        [Test]
        public void Build_ExplicitDeckOrders_ArePreserved()
        {
            // Explicit deck order: 3, 2, 1
            var deckOrder = new[] { new[] { 3, 2, 1 } };
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 }, deckOrders: deckOrder);

            var deck = result.Level.Decks[0];
            Assert.That(deck.Peek(), Is.EqualTo(3));
            deck.Advance();
            Assert.That(deck.Peek(), Is.EqualTo(2));
            deck.Advance();
            Assert.That(deck.Peek(), Is.EqualTo(1));
        }

        [Test]
        public void Build_PuzzleSessionCanCompleteLevelFromDefaultDeck()
        {
            // Seed piece 0. Neighbors of 0 are 1 and 2. Neighbor of both 1 and 2 is 3.
            // Valid placement order from default deck (1, 2, 3): 1, 2, 3.
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            var session = new PuzzleSession(result.Level);

            // Place piece 1 (neighbors seed 0) — should be Placed
            var r1 = session.TryPlace(1);
            Assert.That(r1, Is.EqualTo(PlacementResult.Placed), "Piece 1 should be placeable (neighbors seed 0).");

            // Place piece 2 (neighbors seed 0) — should be Placed
            var r2 = session.TryPlace(2);
            Assert.That(r2, Is.EqualTo(PlacementResult.Placed), "Piece 2 should be placeable (neighbors seed 0).");

            // Place piece 3 (neighbors 1 and 2) — should be Placed
            var r3 = session.TryPlace(3);
            Assert.That(r3, Is.EqualTo(PlacementResult.Placed), "Piece 3 should be placeable (neighbors 1 and 2).");

            Assert.That(session.IsComplete, Is.True, "Session should be complete after all pieces placed.");
        }
    }
}
