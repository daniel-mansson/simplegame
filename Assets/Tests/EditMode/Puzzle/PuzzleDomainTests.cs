using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Puzzle;

namespace SimpleGame.Tests.Puzzle
{
    /// <summary>
    /// EditMode tests for <see cref="PuzzleBoard"/> — the core placement engine
    /// used internally by <see cref="PuzzleModel"/>.
    ///
    /// Test topology:
    ///   [0] seed --- [1] --- [3]
    ///                |
    ///               [2]
    /// </summary>
    [TestFixture]
    internal class PuzzleBoardTests
    {
        private static PuzzleBoard BuildBoard()
        {
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1 }),
                new PuzzlePiece(1, new[] { 0, 2, 3 }),
                new PuzzlePiece(2, new[] { 1 }),
                new PuzzlePiece(3, new[] { 1 }),
            };
            return new PuzzleBoard(pieces);
        }

        [Test]
        public void CanPlace_WhenNeighborPresent_ReturnsTrue()
        {
            var board = BuildBoard();
            board.Place(0); // seed
            Assert.IsTrue(board.CanPlace(1), "Piece 1 neighbors placed piece 0");
        }

        [Test]
        public void CanPlace_WhenNoNeighborPresent_ReturnsFalse()
        {
            var board = BuildBoard();
            board.Place(0); // seed only
            Assert.IsFalse(board.CanPlace(3), "Piece 3 needs piece 1 which is not yet placed");
        }

        [Test]
        public void Place_AddsToPlacedIds()
        {
            var board = BuildBoard();
            board.Place(0);
            Assert.That(board.PlacedIds, Contains.Item(0));
        }

        [Test]
        public void Place_ReturnsFalseOnDuplicate()
        {
            var board = BuildBoard();
            board.Place(0);
            Assert.IsFalse(board.Place(0), "Duplicate place should return false");
        }

        [Test]
        public void Place_ReturnsTrueOnFirstPlace()
        {
            var board = BuildBoard();
            Assert.IsTrue(board.Place(0));
        }

        [Test]
        public void CanPlace_AfterChain_AllowsDownstreamPiece()
        {
            var board = BuildBoard();
            board.Place(0); // seed
            board.Place(1); // 1 neighbors 0
            Assert.IsTrue(board.CanPlace(2), "Piece 2 neighbors piece 1 (now placed)");
            Assert.IsTrue(board.CanPlace(3), "Piece 3 neighbors piece 1 (now placed)");
        }

        [Test]
        public void IsPlaced_ReturnsTrueAfterPlace()
        {
            var board = BuildBoard();
            board.Place(0);
            Assert.IsTrue(board.IsPlaced(0));
        }

        [Test]
        public void IsPlaced_ReturnsFalseBeforePlace()
        {
            var board = BuildBoard();
            Assert.IsFalse(board.IsPlaced(0));
        }
    }
}
