using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Game.InGame;
using SimpleGame.Puzzle;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// EditMode tests for CameraMath perspective framing and PuzzleModel.GetPlaceablePieceIds.
    /// No Unity scene required — CameraMath is pure static, PuzzleModel has no MonoBehaviour deps.
    /// </summary>
    [TestFixture]
    internal class CameraMathTests
    {
        // ── Helpers ───────────────────────────────────────────────────────

        private const float Aspect = 1.0f;
        private const float FOV    = 60f;
        private const float Padding = 1.5f;

        // Z limits — MinZ is closest (zoomed in), MaxZ is furthest (zoomed out)
        private static readonly float MinZ = CameraMath.ZForHalfHeight(2f, FOV); // same visible area as old MinZoom=2
        private static readonly float MaxZ = CameraMath.ZForHalfHeight(15f, FOV);

        // ── Tests ─────────────────────────────────────────────────────────

        [Test]
        public void CameraMath_ComputeFraming_SinglePosition_ReturnsPositionAsCenter()
        {
            var pos = new Vector3(3f, 7f, 0f);
            var positions = new List<Vector3> { pos };

            var (center, z) = CameraMath.ComputeFraming(positions, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(pos.x, center.x, 0.001f, "Center X should match the single position");
            Assert.AreEqual(pos.y, center.y, 0.001f, "Center Y should match the single position");
            // span is zero → requiredHalfH = Padding = 1.5
            // z = 1.5 / tan(30°) = 1.5 / 0.5774 ≈ 2.598, which is < MinZ equivalent
            // so result should be clamped to MinZ
            Assert.AreEqual(MinZ, z, 0.001f, "Single position Z should be clamped to MinZ");
        }

        [Test]
        public void CameraMath_ComputeFraming_MultiplePositions_CorrectBounds()
        {
            var positions = new List<Vector3>
            {
                new Vector3(-5f, -5f, 0f),
                new Vector3( 5f, -5f, 0f),
                new Vector3(-5f,  5f, 0f),
                new Vector3( 5f,  5f, 0f),
            };

            var (center, z) = CameraMath.ComputeFraming(positions, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(0f, center.x, 0.001f, "Center X should be 0 for symmetric positions");
            Assert.AreEqual(0f, center.y, 0.001f, "Center Y should be 0 for symmetric positions");

            // spanY=10, spanX=10, aspect=1
            // requiredHalfH = (10 + 2*1.5) / 2 = 6.5
            // requiredHalfW/aspect = 6.5/1 = 6.5
            // halfH = 6.5 → z = 6.5 / tan(30°) ≈ 11.258
            float expectedZ = CameraMath.ZForHalfHeight(6.5f, FOV);
            expectedZ = Mathf.Clamp(expectedZ, MinZ, MaxZ);
            Assert.AreEqual(expectedZ, z, 0.01f, "Z should cover spread + padding");
        }

        [Test]
        public void CameraMath_ComputeFraming_EmptyPositions_ReturnsFallback()
        {
            var (center, z) = CameraMath.ComputeFraming(
                new List<Vector3>(), Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(Vector3.zero, center, "Empty list should return Vector3.zero as center");
            Assert.AreEqual(MinZ, z, 0.001f, "Empty list should return MinZ");
        }

        [Test]
        public void CameraMath_ComputeFraming_NullPositions_ReturnsFallback()
        {
            var (center, z) = CameraMath.ComputeFraming(
                null, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(Vector3.zero, center, "Null list should return Vector3.zero as center");
            Assert.AreEqual(MinZ, z, 0.001f, "Null list should return MinZ");
        }

        [Test]
        public void CameraMath_ComputeFraming_ClampsToMaxZ()
        {
            var positions = new List<Vector3>
            {
                new Vector3(-100f,  0f, 0f),
                new Vector3( 100f,  0f, 0f),
            };

            var (_, z) = CameraMath.ComputeFraming(positions, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(MaxZ, z, 0.001f, "Wide spread should be clamped to MaxZ");
        }

        [Test]
        public void CameraMath_ComputeFraming_ClampsToMinZ()
        {
            var positions = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0.01f, 0.01f, 0f),
            };

            var (_, z) = CameraMath.ComputeFraming(positions, 0f, Aspect, FOV, MinZ, MaxZ);

            Assert.GreaterOrEqual(z, MinZ, "Z should never go below MinZ");
        }

        // ── Frustum helpers ──────────────────────────────────────────────

        [Test]
        public void FrustumHalfHeight_RoundTrips_WithZForHalfHeight()
        {
            float halfH = 5f;
            float z = CameraMath.ZForHalfHeight(halfH, FOV);
            float result = CameraMath.FrustumHalfHeight(z, FOV);
            Assert.AreEqual(halfH, result, 0.001f, "FrustumHalfHeight should round-trip with ZForHalfHeight");
        }
    }

    // ---------------------------------------------------------------------------
    // PuzzleModel.GetPlaceablePieceIds tests
    // ---------------------------------------------------------------------------

    [TestFixture]
    internal class GetPlaceablePieceIdsTests
    {
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
            var model = LinearChainModel(5, slotCount: 1);
            model.TryPlace(0);

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
            model.TryPlace(0);
            model.TryPlace(0);
            model.TryPlace(0);
            model.TryPlace(0);

            Assert.IsTrue(model.IsComplete, "Model should be complete after placing all pieces");
            var ids = model.GetPlaceablePieceIds();
            Assert.AreEqual(0, ids.Count, "No placeable pieces when puzzle is complete");
        }

        [Test]
        public void GetPlaceablePieceIds_AfterEachPlacement_ListShrinks()
        {
            var model = LinearChainModel(5, slotCount: 1);

            var ids0 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids0.Count, "One placeable piece before any placement");

            model.TryPlace(0);
            var ids1 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids1.Count, "One placeable piece after placing piece 1");
            CollectionAssert.Contains(ids1, 2);

            model.TryPlace(0);
            var ids2 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids2.Count, "One placeable piece after placing piece 2");
            CollectionAssert.Contains(ids2, 3);

            model.TryPlace(0);
            var ids3 = model.GetPlaceablePieceIds();
            Assert.AreEqual(1, ids3.Count, "One placeable piece after placing piece 3");
            CollectionAssert.Contains(ids3, 4);

            model.TryPlace(0);
            var ids4 = model.GetPlaceablePieceIds();
            Assert.AreEqual(0, ids4.Count, "No placeable pieces when puzzle is complete");
        }

        [Test]
        public void GetPlaceablePieceIds_BranchingModel_MultipleRootsReachable()
        {
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
        private const float FOV    = 60f;

        // ── ClampToBounds ─────────────────────────────────────────────────

        [Test]
        public void ClampToBounds_PositionInsideBounds_Unchanged()
        {
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            // Z=2 → halfH ≈ 1.155, halfW ≈ 1.155 — well within 10×10 board
            var cam = new Vector3(0f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, z: 2f, fovDegrees: FOV, aspect: Aspect, bounds: bounds, margin: 0f);

            Assert.AreEqual(0f, result.x, 0.001f, "X should be unchanged when inside bounds");
            Assert.AreEqual(0f, result.y, 0.001f, "Y should be unchanged when inside bounds");
            Assert.AreEqual(cam.z, result.z, 0.001f, "Z should be preserved");
        }

        [Test]
        public void ClampToBounds_PositionBeyondRight_ClampsX()
        {
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            float z = 2f;
            var cam = new Vector3(20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, z: z, fovDegrees: FOV, aspect: Aspect, bounds: bounds, margin: 0f);

            float halfW = CameraMath.FrustumHalfHeight(z, FOV) * Aspect;
            float expectedMaxX = 5f - halfW;
            Assert.AreEqual(expectedMaxX, result.x, 0.01f, "X should be clamped to keep right viewport edge on board");
        }

        [Test]
        public void ClampToBounds_PositionBeyondLeft_ClampsX()
        {
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            float z = 2f;
            var cam = new Vector3(-20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, z: z, fovDegrees: FOV, aspect: Aspect, bounds: bounds, margin: 0f);

            float halfW = CameraMath.FrustumHalfHeight(z, FOV) * Aspect;
            float expectedMinX = -5f + halfW;
            Assert.AreEqual(expectedMinX, result.x, 0.01f, "X should be clamped to keep left viewport edge on board");
        }

        [Test]
        public void ClampToBounds_ViewportLargerThanBoard_CentresOnBoard()
        {
            var bounds = new Rect(0f, 0f, 2f, 2f);
            // Large Z → viewport much bigger than 2×2 board
            float z = 20f;
            var cam = new Vector3(99f, -99f, -10f);

            var result = CameraMath.ClampToBounds(cam, z: z, fovDegrees: FOV, aspect: Aspect, bounds: bounds, margin: 0f);

            Assert.AreEqual(bounds.center.x, result.x, 0.001f, "X should be board centre when viewport exceeds board");
            Assert.AreEqual(bounds.center.y, result.y, 0.001f, "Y should be board centre when viewport exceeds board");
        }

        [Test]
        public void ClampToBounds_MarginReducesAllowedRange()
        {
            var bounds = new Rect(-5f, -5f, 10f, 10f);
            float z = 2f;
            var cam = new Vector3(20f, 0f, -10f);

            var result = CameraMath.ClampToBounds(cam, z: z, fovDegrees: FOV, aspect: Aspect, bounds: bounds, margin: 1f);

            float halfW = CameraMath.FrustumHalfHeight(z, FOV) * Aspect;
            float expectedMaxX = 5f - 1f - halfW;
            Assert.AreEqual(expectedMaxX, result.x, 0.01f, "Margin should further restrict the clamp range");
        }

        // ── ComputeBoardRect ──────────────────────────────────────────────

        [Test]
        public void ComputeBoardRect_SquareGrid_ReturnsUnitSquare()
        {
            var rect = CameraMath.ComputeBoardRect(4, 4);

            Assert.AreEqual(1f, rect.width,  0.001f, "Square grid width should be 1 world unit");
            Assert.AreEqual(1f, rect.height, 0.001f, "Square grid height should be 1 world unit");
            Assert.AreEqual(0f, rect.center.x, 0.001f, "Board rect should be centred on X=0");
            Assert.AreEqual(0f, rect.center.y, 0.001f, "Board rect should be centred on Y=0");
        }

        [Test]
        public void ComputeBoardRect_RectangularGrid_CorrectAspect()
        {
            var rect = CameraMath.ComputeBoardRect(2, 4);

            Assert.AreEqual(1f,   rect.width,  0.001f, "Width (longest side) should be 1");
            Assert.AreEqual(0.5f, rect.height, 0.001f, "Height should be 0.5 for a 2×4 grid");
            Assert.AreEqual(0f, rect.center.x, 0.001f, "Board rect should be centred on X=0");
            Assert.AreEqual(0f, rect.center.y, 0.001f, "Board rect should be centred on Y=0");
        }
    }

    // ---------------------------------------------------------------------------
    // CameraMath.ComputeFullBoardFraming tests
    // ---------------------------------------------------------------------------

    [TestFixture]
    internal class ComputeFullBoardFramingTests
    {
        private const float Aspect  = 1.0f;
        private const float FOV     = 60f;
        private const float Padding = 1.0f;

        private static readonly float MinZ = CameraMath.ZForHalfHeight(2f, FOV);
        private static readonly float MaxZ = CameraMath.ZForHalfHeight(15f, FOV);

        [Test]
        public void ComputeFullBoardFraming_SquareBoard_ReturnsCorrectFraming()
        {
            var boardRect = new Rect(-0.5f, -0.5f, 1f, 1f);

            var (center, z) = CameraMath.ComputeFullBoardFraming(
                boardRect, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(0f, center.x, 0.001f, "Center X should be 0 for origin-centred board");
            Assert.AreEqual(0f, center.y, 0.001f, "Center Y should be 0 for origin-centred board");
            Assert.AreEqual(0f, center.z, 0.001f, "Center Z should be 0");

            // requiredHalfH = (1 + 2*1) * 0.5 = 1.5
            // requiredHalfW = (1 + 2*1) * 0.5 = 1.5; halfHFromWidth = 1.5/1 = 1.5
            // halfH = max(1.5, 1.5) = 1.5 → z = 1.5/tan(30°) ≈ 2.598 → clamped to MinZ
            float expectedZ = Mathf.Clamp(CameraMath.ZForHalfHeight(1.5f, FOV), MinZ, MaxZ);
            Assert.AreEqual(expectedZ, z, 0.01f, "Z should frame the small board or be clamped to MinZ");
        }

        [Test]
        public void ComputeFullBoardFraming_RectangularBoard_AdjustsForAspect()
        {
            var boardRect = new Rect(-1f, -0.5f, 2f, 1f);

            var (center, z) = CameraMath.ComputeFullBoardFraming(
                boardRect, Padding, Aspect, FOV, MinZ, MaxZ);

            Assert.AreEqual(0f, center.x, 0.001f, "Center X should be 0");
            Assert.AreEqual(0f, center.y, 0.001f, "Center Y should be 0");

            // requiredHalfH = (1 + 2*1) * 0.5 = 1.5
            // requiredHalfW = (2 + 2*1) * 0.5 = 2.0; halfHFromWidth = 2.0/1 = 2.0
            // halfH = max(1.5, 2.0) = 2.0 → z = 2.0/tan(30°) ≈ 3.464
            float expectedZ = Mathf.Clamp(CameraMath.ZForHalfHeight(2f, FOV), MinZ, MaxZ);
            Assert.AreEqual(expectedZ, z, 0.01f, "Z should account for the wider dimension");
        }

        [Test]
        public void ComputeFullBoardFraming_TinyBoard_ClampsToMinZ()
        {
            var boardRect = new Rect(-0.05f, -0.05f, 0.1f, 0.1f);

            var (_, z) = CameraMath.ComputeFullBoardFraming(
                boardRect, padding: 0f, aspect: Aspect, fovDegrees: FOV, minZ: MinZ, maxZ: MaxZ);

            Assert.AreEqual(MinZ, z, 0.001f, "Tiny board Z must be clamped to MinZ");
        }
    }
}
