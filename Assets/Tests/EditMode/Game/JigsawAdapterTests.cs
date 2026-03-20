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
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.PieceList.Count, Is.EqualTo(4));
        }

        [Test]
        public void Build_2x2Grid_RawBoardIsReturned()
        {
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.RawBoard, Is.Not.Null);
            Assert.That(result.RawBoard.Pieces.Count, Is.EqualTo(4));
        }

        [Test]
        public void Build_2x2Grid_EachPieceHasTwoNeighbors()
        {
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            foreach (var piece in result.PieceList)
            {
                Assert.That(piece.NeighborIds.Count, Is.EqualTo(2),
                    $"Piece {piece.Id} in a 2×2 grid should have exactly 2 neighbors.");
            }
        }

        [Test]
        public void Build_2x2Grid_NeighborRelationshipsAreSymmetric()
        {
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
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
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            Assert.That(result.SeedIds, Contains.Item(0));
        }

        [Test]
        public void Build_DefaultDeck_ContainsAllNonSeedPieces()
        {
            // Deck must contain all non-seed pieces; seed not included; order is from SolvableShuffle
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });

            Assert.That(result.DeckOrder, Has.No.Member(0), "Seed piece should not be in the deck.");
            Assert.That(result.DeckOrder.Count, Is.EqualTo(3), "Deck should contain the 3 non-seed pieces.");
            // All non-seed IDs (1,2,3) must appear exactly once
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result.DeckOrder);
        }

        [Test]
        public void Build_DeckOrder_ContainsAllNonSeedPieces_Unordered()
        {
            // Deck order is topology-aware — assert containment, not specific order
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, result.DeckOrder);
        }

        [Test]
        public void Build_PuzzleModelCanCompleteLevelFromDefaultDeck()
        {
            // SolvableShuffle guarantees a solvable ordering — verify with greedy placement.
            var result = JigsawLevelFactory.Build(_config2x2, slotCount: 1, seed: 42, seedPieceIds: new[] { 0 });
            var model  = new PuzzleModel(result.PieceList, result.SeedIds, result.DeckOrder, slotCount: 1);

            // Greedy: try placing slot 0 until complete or stalled
            int passes = 0;
            while (!model.IsComplete && passes++ < 10)
                model.TryPlace(0);

            Assert.That(model.IsComplete, Is.True,
                "SolvableShuffle deck must be completable from a single slot.");
        }

        [Test]
        public void BuildSolvable_2x2_ReturnsSolvableResult()
        {
            // Any seed piece — let factory choose randomly
            var result = JigsawLevelFactory.BuildSolvable(_config2x2, slotCount: 1, initialSeed: 99);
            Assert.That(result.PieceList.Count, Is.EqualTo(4));
            Assert.That(result.DeckOrder.Count, Is.EqualTo(3));
        }

        [Test]
        public void BuildSolvable_2x2_PuzzleIsCompletableWithReturnedDeck()
        {
            // Verify the returned layout is actually solvable end-to-end with PuzzleModel
            var result = JigsawLevelFactory.BuildSolvable(_config2x2, slotCount: 3, initialSeed: 7);
            var model = new PuzzleModel(result.PieceList, result.SeedIds, result.DeckOrder, slotCount: 3);

            // Greedy: on each pass, place any slot that accepts
            bool MakeProgress()
            {
                bool any = false;
                for (int s = 0; s < model.SlotCount; s++)
                    if (model.TryPlace(s) == SlotTapResult.Placed) any = true;
                return any;
            }

            int passes = 0;
            while (!model.IsComplete && passes++ < 50)
                MakeProgress();

            Assert.That(model.IsComplete, Is.True, "BuildSolvable result must be completable.");
        }
    }
}
