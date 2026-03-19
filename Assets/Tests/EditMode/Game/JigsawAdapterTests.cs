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
    /// Verifies the adapter correctly maps SimpleJigsaw.PuzzleBoard → puzzle domain data.
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
            Assert.That(result.PieceList.Count, Is.EqualTo(4));
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
            foreach (var piece in result.PieceList)
            {
                Assert.That(piece.NeighborIds.Count, Is.EqualTo(2),
                    $"Piece {piece.Id} in a 2×2 grid should have exactly 2 neighbors.");
            }
        }

        [Test]
        public void Build_2x2Grid_NeighborRelationshipsAreSymmetric()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            var pieceById = result.PieceList.ToDictionary(p => p.Id);

            foreach (var piece in result.PieceList)
            {
                foreach (var neighborId in piece.NeighborIds)
                {
                    Assert.That(pieceById[neighborId].NeighborIds, Contains.Item(piece.Id),
                        $"Piece {neighborId} should list {piece.Id} as a neighbor (symmetric).");
                }
            }
        }

        [Test]
        public void Build_SeedIdsArePreservedInResult()
        {
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.SeedIds, Contains.Item(0));
        }

        [Test]
        public void Build_DefaultDeck_ContainsAllNonSeedPieces()
        {
            // Default deck: all non-seed pieces in ascending ID order
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });

            Assert.That(result.DeckOrder, Has.No.Member(0), "Seed piece should not be in the deck.");
            Assert.That(result.DeckOrder.Count, Is.EqualTo(3), "Deck should contain the 3 non-seed pieces.");
            Assert.That(result.DeckOrder, Is.Ordered, "Default deck should be in ascending ID order.");
        }

        [Test]
        public void Build_DeckOrder_IsAscending()
        {
            // Default deck should be in ascending ID order
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.DeckOrder, Is.Ordered, "Default deck should be in ascending ID order.");
        }

        [Test]
        public void Build_PuzzleModelCanCompleteLevelFromDefaultDeck()
        {
            // Seed piece 0. Neighbors of 0 are 1 and 2. Neighbor of both 1 and 2 is 3.
            // Valid placement via PuzzleModel (single slot): 1, 2, 3.
            var result = JigsawLevelFactory.Build(_config2x2, seed: 42, seedPieceIds: new[] { 0 });
            var model = new PuzzleModel(result.PieceList, result.SeedIds, result.DeckOrder, slotCount: 1);

            // Slot 0 = piece 1 (neighbours seed 0) — correct
            var r1 = model.TryPlace(0);
            Assert.That(r1, Is.EqualTo(SlotTapResult.Placed), "Piece 1 should be placeable (neighbors seed 0).");

            // Slot 0 now = piece 2 (neighbours seed 0) — correct
            var r2 = model.TryPlace(0);
            Assert.That(r2, Is.EqualTo(SlotTapResult.Placed), "Piece 2 should be placeable (neighbors seed 0).");

            // Slot 0 now = piece 3 (neighbours 1 and 2) — correct
            var r3 = model.TryPlace(0);
            Assert.That(r3, Is.EqualTo(SlotTapResult.Placed), "Piece 3 should be placeable (neighbors 1 and 2).");

            Assert.That(model.IsComplete, Is.True, "Model should be complete after all pieces placed.");
        }
    }
}
