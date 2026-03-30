using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Game.InGame;
using SimpleGame.Puzzle;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// EditMode tests for CameraMath.ComputeFraming and PuzzleModel.GetPlaceablePieceIds.
    /// No Unity scene required — CameraMath is pure static, PuzzleModel has no MonoBehaviour deps.
    /// </summary>
    [TestFixture]
    internal class CameraMathTests
    {
        // ── Helpers ───────────────────────────────────────────────────────

        private const float Aspect = 1.0f; // square aspect for predictable framing math
        private const float MinZoom = 2f;
        private const float MaxZoom = 15f;
        private const float Padding = 1.5f;

        // ── Tests ─────────────────────────────────────────────────────────

        [Test]
        public void CameraMath_ComputeFraming_SinglePosition_ReturnsPositionAsCenter()
        {
            var pos = new Vector3(3f, 7f, 0f);
            var positions = new List<Vector3> { pos };

            var (center, ortho) = CameraMath.ComputeFraming(positions, Padding, Aspect, MinZoom, MaxZoom);

            Assert.AreEqual(pos.x, center.x, 0.001f, "Center X should match the single position");
            Assert.AreEqual(pos.y, center.y, 0.001f, "Center Y should match the single position");
            // span is zero → requiredByHeight = Padding, requiredByWidth = Padding / (2*aspect) = Padding
            // ortho = Padding = 1.5 which is < MinZoom=2 → clamped to MinZoom
            Assert.AreEqual(MinZoom, ortho, 0.001f, "Single position ortho should be clamped to MinZoom");
        }

        [Test]
        public void CameraMath_ComputeFraming_MultiplePositions_CorrectBounds()
        {
            // 4 corner positions at ±5 → bounding box is 10×10, center (0,0)
            var positions = new List<Vector3>
            {
                new Vector3(-5f, -5f, 0f),
                new Vector3( 5f, -5f, 0f),
                new Vector3(-5f,  5f, 0f),
                new Vector3( 5f,  5f, 0f),
            };

            var (center, ortho) = CameraMath.ComputeFraming(positions, Padding, Aspect, MinZoom, MaxZoom);

            Assert.AreEqual(0f, center.x, 0.001f, "Center X should be 0 for symmetric positions");
            Assert.AreEqual(0f, center.y, 0.001f, "Center Y should be 0 for symmetric positions");

            // spanY=10, spanX=10, aspect=1
            // requiredByHeight = (10 + 2*1.5) / 2 = 6.5
            // requiredByWidth  = (10 + 2*1.5) / (2*1) = 6.5
            // ortho = max(6.5, 6.5) = 6.5, clamped to [2,15] → 6.5
            Assert.AreEqual(6.5f, ortho, 0.001f, "OrthoSize should cover spread + padding");
        }

        [Test]
        public void CameraMath_ComputeFraming_EmptyPositions_ReturnsFallback()
        {
            var (center, ortho) = CameraMath.ComputeFraming(
                new List<Vector3>(), Padding, Aspect, MinZoom, MaxZoom);

            Assert.AreEqual(Vector3.zero, center, "Empty list should return Vector3.zero as center");
            Assert.AreEqual(MinZoom, ortho, 0.001f, "Empty list should return minZoom as orthoSize");
        }

        [Test]
        public void CameraMath_ComputeFraming_NullPositions_ReturnsFallback()
        {
            var (center, ortho) = CameraMath.ComputeFraming(
                null, Padding, Aspect, MinZoom, MaxZoom);

            Assert.AreEqual(Vector3.zero, center, "Null list should return Vector3.zero as center");
            Assert.AreEqual(MinZoom, ortho, 0.001f, "Null list should return minZoom as orthoSize");
        }

        [Test]
        public void CameraMath_ComputeFraming_ClampsToMaxZoom()
        {
            // Positions spread 200 units apart → required ortho far exceeds MaxZoom=15
            var positions = new List<Vector3>
            {
                new Vector3(-100f,  0f, 0f),
                new Vector3( 100f,  0f, 0f),
            };

            var (_, ortho) = CameraMath.ComputeFraming(positions, Padding, Aspect, MinZoom, MaxZoom);

            Assert.AreEqual(MaxZoom, ortho, 0.001f, "Wide spread should be clamped to MaxZoom");
        }

        [Test]
        public void CameraMath_ComputeFraming_ClampsToMinZoom()
        {
            // Tiny spread — should be clamped up to MinZoom
            var positions = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0.01f, 0.01f, 0f),
            };

            var (_, ortho) = CameraMath.ComputeFraming(positions, 0f, Aspect, MinZoom, MaxZoom);

            Assert.GreaterOrEqual(ortho, MinZoom, "OrthoSize should never go below MinZoom");
        }
    }

    // ---------------------------------------------------------------------------
    // PuzzleModel.GetPlaceablePieceIds tests
    // ---------------------------------------------------------------------------

    [TestFixture]
    internal class GetPlaceablePieceIdsTests
    {
        // Builds a linear chain: 0(seed) → 1 → 2 → 3 → 4
        // with slotCount=1 so TryPlace from slot 0 places the piece at slot 0.
        private static PuzzleModel LinearChainModel(int totalPieces, int slotCount = 1)
        {
            var pieces = new List<IPuzzlePiece>(totalPieces);
            for (int i = 0; i < totalPieces; i++)
            {
                var neighbors = new List<int>();
                if (i > 0) neighbors.Add(i - 1);
                if (i < totalPieces - 1) neighbors.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, neighbors));
            }
            var deckOrder = new int[totalPieces - 1];
            for (int i = 0; i < deckOrder.Length; i++) deckOrder[i] = i + 1;
            return new PuzzleModel(pieces, new[] { 0 }, deckOrder, slotCount);
        }

        [Test]
        public void GetPlaceablePieceIds_InitialState_ReturnsOnlyDirectNeighboursOfSeed()
        {
            // 5-piece chain: seed=0. Only piece 1 is placeable initially (neighbour of 0).
            // Pieces 2,3,4 are not yet reachable.
            var model = LinearChainModel(5);

            var ids = model.GetPlaceablePieceIds();

            CollectionAssert.Contains(ids, 1, "Piece 1 (neighbour of seed 0) should be placeable");
            CollectionAssert.DoesNotContain(ids, 2, "Piece 2 should not be placeable until 1 is placed");
            CollectionAssert.DoesNotContain(ids, 3, "Piece 3 should not be placeable yet");
            CollectionAssert.DoesNotContain(ids, 4, "Piece 4 should not be placeable yet");
        }

        [Test]
        public void GetPlaceablePieceIds_ReturnsOnlyValidUnplacedPieces()
        {
            // Place seed (already done) and piece 1 → only piece 2 should be placeable
            var model = LinearChainModel(5, slotCount: 1);

            // slot 0 starts with piece 1 — place it
            model.TryPlace(0); // places piece 1

            var ids = model.GetPlaceablePieceIds();

            CollectionAssert.DoesNotContain(ids, 0, "Seed is already placed");
            CollectionAssert.DoesNotContain(ids, 1, "Piece 1 is already placed");
            CollectionAssert.Contains(ids, 2, "Piece 2 should now be placeable (neighbour of 1)");
            CollectionAssert.DoesNotContain(ids, 3, "Piece 3 should not be placeable (2 not placed)");
            CollectionAssert.DoesNotContain(ids, 4, "Piece 4 should not be placeable (3 not placed)");
        }

        [Test]
        public void GetPlaceablePieceIds_AllPlaced_ReturnsEmpty()
        {
            var model = LinearChainModel(5, slotCount: 1);

            // Place all 4 non-seed pieces in sequence
            model.TryPlace(0); // piece 1
            model.TryPlace(0); // piece 2
            model.TryPlace(0); // piece 3
            model.TryPlace(0); // piece 4

            Assert.IsTrue(model.IsComplete, "Model should be complete after placing all pieces");
            var ids = model.GetPlaceablePieceIds();
            Assert.AreEqual(0, ids.Count, "No placeable pieces when puzzle is complete");
        }

        [Test]
        public void GetPlaceablePieceIds_AfterEachPlacement_ListShrinks()
        {
            // A linear chain: each placement removes one piece from the "placed" category
            // and potentially adds the next. The list should change predictably.
            var model = LinearChainModel(5, slotCount: 1);

            // Initially: 1 placeable (piece 1)
            var ids0 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids0.Count, "One placeable piece before any placement");

            model.TryPlace(0); // place piece 1 → piece 2 becomes placeable
            var ids1 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids1.Count, "One placeable piece after placing piece 1");
            CollectionAssert.Contains(ids1, 2);

            model.TryPlace(0); // place piece 2 → piece 3 becomes placeable
            var ids2 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids2.Count, "One placeable piece after placing piece 2");
            CollectionAssert.Contains(ids2, 3);

            model.TryPlace(0); // place piece 3 → piece 4 becomes placeable
            var ids3 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids3.Count, "One placeable piece after placing piece 3");
            CollectionAssert.Contains(ids3, 4);

            model.TryPlace(0); // place piece 4 → complete, nothing placeable
            var ids4 = model.GetPlaceablePieceIds();
            Assert.AreEqual(0, ids4.Count, "No placeable pieces when puzzle is complete");
        }

        [Test]
        public void GetPlaceablePieceIds_BranchingModel_MultipleRootsReachable()
        {
            // Star topology: piece 0 (seed) connects to pieces 1,2,3 (all neighbours of 0).
            // After seed, all three should be immediately placeable.
            var pieces = new List<IPuzzlePiece>
            {
                new PuzzlePiece(0, new[] { 1, 2, 3 }),
                new PuzzlePiece(1, new[] { 0 }),
                new PuzzlePiece(2, new[] { 0 }),
                new PuzzlePiece(3, new[] { 0 }),
            };
            var deckOrder = new[] { 1, 2, 3 };
            var model = new PuzzleModel(pieces, new[] { 0 }, deckOrder, slotCount: 1);

            var ids = model.GetPlaceablePieceIds();

            Assert.AreEqual(3, ids.Count, "All three star-branches should be placeable initially");
            CollectionAssert.Contains(ids, 1);
            CollectionAssert.Contains(ids, 2);
            CollectionAssert.Contains(ids, 3);
        }
    }

    // ---------------------------------------------------------------------------
    // CameraMath.ClampToBounds and CameraMath.ComputeBoardRect tests
    // ---------------------------------------------------------------------------

    [TestFixture]
    internal class CameraClampAndBoardRectTests
    {
        private const float Aspect = 1.0f;

        // ── ClampToBounds ─────────────────────────────────────────────────

        [Test]
        public void ClampToBounds_PositionInsideBounds_Unchanged()
        {
            // 10×10 board centred at origin; camera at (0,0) with orthoSize=2 (viewport 4×4)
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            var cam    = new Vector3(0f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, orthoSize: 2f, aspect: Aspect, bounds: bounds, margin: 0f);

            Assert.AreEqual(0f, result.x, 0.001f, "X should be unchanged when inside bounds");
            Assert.AreEqual(0f, result.y, 0.001f, "Y should be unchanged when inside bounds");
            Assert.AreEqual(cam.z, result.z, 0.001f, "Z should be preserved");
        }

        [Test]
        public void ClampToBounds_PositionBeyondRight_ClampsX()
        {
            // 10×10 board; camera pushed far right (x=20) — should be pulled back so viewport edge aligns with board edge
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            var cam    = new Vector3(20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, orthoSize: 2f, aspect: Aspect, bounds: bounds, margin: 0f);

            // halfW = 2*1 = 2; maxX = 5 - 0 - 2 = 3
            Assert.AreEqual(3f, result.x, 0.001f, "X should be clamped to keep right viewport edge on board");
        }

        [Test]
        public void ClampToBounds_PositionBeyondLeft_ClampsX()
        {
            // 10×10 board; camera pushed far left (x=-20)
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            var cam    = new Vector3(-20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, orthoSize: 2f, aspect: Aspect, bounds: bounds, margin: 0f);

            // halfW = 2; minX = -5 + 0 + 2 = -3
            Assert.AreEqual(-3f, result.x, 0.001f, "X should be clamped to keep left viewport edge on board");
        }

        [Test]
        public void ClampToBounds_ViewportLargerThanBoard_CentresOnBoard()
        {
            // 2×2 board centred at (1,1); camera with orthoSize=10 (viewport 20×20 — far bigger)
            var bounds = new Rect(0f, 0f, 2f, 2f);
            var cam    = new Vector3(99f, -99f, -10f);

            var result = CameraMath.ClampToBounds(cam, orthoSize: 10f, aspect: Aspect, bounds: bounds, margin: 0f);

            Assert.AreEqual(bounds.center.x, result.x, 0.001f, "X should be board centre when viewport exceeds board");
            Assert.AreEqual(bounds.center.y, result.y, 0.001f, "Y should be board centre when viewport exceeds board");
        }

        [Test]
        public void ClampToBounds_MarginReducesAllowedRange()
        {
            // 10×10 board; margin=1 shrinks the allowed movement range by 1 unit each side
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            // Push camera to the right past the margin-adjusted edge
            var cam    = new Vector3(20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, orthoSize: 2f, aspect: Aspect, bounds: bounds, margin: 1f);

            // halfW=2, margin=1: maxX = 5 - 1 - 2 = 2
            Assert.AreEqual(2f, result.x, 0.001f, "Margin should further restrict the clamp range");
        }

        // ── ComputeBoardRect ──────────────────────────────────────────────

        [Test]
        public void ComputeBoardRect_SquareGrid_ReturnsUnitSquare()
        {
            // 4×4 grid — longest side = 4, so each axis spans 4/4 = 1 world unit
            var rect = CameraMath.ComputeBoardRect(4, 4);

            Assert.AreEqual(1f, rect.width,  0.001f, "Square grid width should be 1 world unit");
            Assert.AreEqual(1f, rect.height, 0.001f, "Square grid height should be 1 world unit");
            Assert.AreEqual(0f, rect.center.x, 0.001f, "Board rect should be centred on X=0");
            Assert.AreEqual(0f, rect.center.y, 0.001f, "Board rect should be centred on Y=0");
        }

        [Test]
        public void ComputeBoardRect_RectangularGrid_CorrectAspect()
        {
            // 2 rows × 4 cols — longest side = 4. Width = 4/4 = 1, height = 2/4 = 0.5
            var rect = CameraMath.ComputeBoardRect(2, 4);

            Assert.AreEqual(1f,   rect.width,  0.001f, "Width (longest side) should be 1");
            Assert.AreEqual(0.5f, rect.height, 0.001f, "Height should be 0.5 for a 2×4 grid");
            Assert.AreEqual(0f, rect.center.x, 0.001f, "Board rect should be centred on X=0");
            Assert.AreEqual(0f, rect.center.y, 0.001f, "Board rect should be centred on Y=0");
        }
    }
}
