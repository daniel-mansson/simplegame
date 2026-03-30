using System.Collections.Generic;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Pure-math helpers for auto-tracking camera framing.
    /// No Unity object dependencies — safe to use in EditMode tests.
    /// </summary>
    public static class CameraMath
    {
        /// <summary>
        /// Compute the camera center and orthographic size needed to frame an entire
        /// puzzle board defined by <paramref name="boardRect"/>.
        /// </summary>
        /// <param name="boardRect">World-space rect of the board.</param>
        /// <param name="padding">World-unit margin added around the rect on every side.</param>
        /// <param name="aspect">Camera aspect ratio (width / height).</param>
        /// <param name="minZoom">Minimum orthographic size (most zoomed-in).</param>
        /// <param name="maxZoom">Maximum orthographic size (most zoomed-out).</param>
        /// <returns>
        /// A tuple of <c>(center, orthoSize)</c> where <c>center</c> is the centre of
        /// <paramref name="boardRect"/> and <c>orthoSize</c> is clamped to
        /// [<paramref name="minZoom"/>, <paramref name="maxZoom"/>].
        /// </returns>
        public static (Vector3 center, float orthoSize) ComputeFullBoardFraming(
            Rect boardRect, float padding, float aspect, float minZoom, float maxZoom)
        {
            var center = new Vector3(boardRect.center.x, boardRect.center.y, 0f);

            float requiredByHeight = (boardRect.height + 2f * padding) * 0.5f;
            float requiredByWidth  = (boardRect.width  + 2f * padding) / (2f * aspect);
            float orthoSize        = Mathf.Max(requiredByHeight, requiredByWidth);

            orthoSize = Mathf.Clamp(orthoSize, minZoom, maxZoom);

            return (center, orthoSize);
        }

        /// <summary>
        /// Compute the camera center and orthographic size needed to frame all
        /// <paramref name="positions"/> with the given <paramref name="padding"/>.
        /// </summary>
        /// <param name="positions">World-space positions to frame. Empty list returns board-center fallback.</param>
        /// <param name="padding">World-unit margin added around the bounding box on every side.</param>
        /// <param name="aspect">Camera aspect ratio (width / height).</param>
        /// <param name="minZoom">Minimum orthographic size (most zoomed-in).</param>
        /// <param name="maxZoom">Maximum orthographic size (most zoomed-out).</param>
        /// <returns>
        /// A tuple of <c>(center, orthoSize)</c> where <c>center</c> is the midpoint of the
        /// bounding box and <c>orthoSize</c> is clamped to [<paramref name="minZoom"/>,
        /// <paramref name="maxZoom"/>].
        /// </returns>
        public static (Vector3 center, float orthoSize) ComputeFraming(
            IReadOnlyList<Vector3> positions,
            float padding,
            float aspect,
            float minZoom,
            float maxZoom)
        {
            if (positions == null || positions.Count == 0)
                return (Vector3.zero, minZoom);

            float minX = positions[0].x, maxX = positions[0].x;
            float minY = positions[0].y, maxY = positions[0].y;

            for (int i = 1; i < positions.Count; i++)
            {
                var p = positions[i];
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            var center = new Vector3(
                (minX + maxX) * 0.5f,
                (minY + maxY) * 0.5f,
                0f);

            float spanY = maxY - minY;
            float spanX = maxX - minX;

            // orthoSize covers half the camera height; aspect scales horizontal coverage.
            float requiredByHeight = (spanY + 2f * padding) * 0.5f;
            float requiredByWidth  = (spanX + 2f * padding) / (2f * aspect);
            float orthoSize        = Mathf.Max(requiredByHeight, requiredByWidth);

            orthoSize = Mathf.Clamp(orthoSize, minZoom, maxZoom);

            return (center, orthoSize);
        }

        /// <summary>
        /// Clamp a proposed camera XY position so that the visible viewport (sized by
        /// <paramref name="orthoSize"/> and <paramref name="aspect"/>) stays within
        /// <paramref name="bounds"/> plus a <paramref name="margin"/> on every side.
        /// <para>
        /// When the viewport is larger than the board in a given axis the camera is
        /// centred on the board in that axis rather than pushed to either edge.
        /// </para>
        /// </summary>
        /// <param name="cameraPos">Proposed camera world position (XY used; Z preserved).</param>
        /// <param name="orthoSize">Current orthographic half-height of the camera.</param>
        /// <param name="aspect">Camera aspect ratio (width / height).</param>
        /// <param name="bounds">The rect that defines board space (e.g. from ComputeBoardRect).</param>
        /// <param name="margin">Extra world-unit margin to keep between viewport and board edge.</param>
        /// <returns>Clamped camera position.</returns>
        public static Vector3 ClampToBounds(
            Vector3 cameraPos,
            float   orthoSize,
            float   aspect,
            Rect    bounds,
            float   margin)
        {
            float halfH = orthoSize;
            float halfW = orthoSize * aspect;

            float boardHalfW = bounds.width  * 0.5f;
            float boardHalfH = bounds.height * 0.5f;
            float cx         = bounds.center.x;
            float cy         = bounds.center.y;

            float x;
            if (halfW >= boardHalfW + margin)
            {
                // Viewport wider than (board + margin) — centre on board
                x = cx;
            }
            else
            {
                float minX = bounds.xMin + margin + halfW;
                float maxX = bounds.xMax - margin - halfW;
                x = Mathf.Clamp(cameraPos.x, minX, maxX);
            }

            float y;
            if (halfH >= boardHalfH + margin)
            {
                // Viewport taller than (board + margin) — centre on board
                y = cy;
            }
            else
            {
                float minY = bounds.yMin + margin + halfH;
                float maxY = bounds.yMax - margin - halfH;
                y = Mathf.Clamp(cameraPos.y, minY, maxY);
            }

            return new Vector3(x, y, cameraPos.z);
        }

        /// <summary>
        /// Compute the world-space Rect that encloses a puzzle board of
        /// <paramref name="rows"/> × <paramref name="cols"/> cells, using the same
        /// unit-based convention as GridPlanner: the board is centred on the origin
        /// and each cell is 1 / max(rows, cols) world units wide and tall so that the
        /// longest side always spans exactly 1 world unit.
        /// </summary>
        /// <param name="rows">Number of grid rows.</param>
        /// <param name="cols">Number of grid columns.</param>
        /// <returns>A <see cref="Rect"/> centred on (0, 0).</returns>
        public static Rect ComputeBoardRect(int rows, int cols)
        {
            if (rows <= 0) rows = 1;
            if (cols <= 0) cols = 1;

            float unitScale = Mathf.Max(rows, cols);
            float width     = cols / unitScale;
            float height    = rows / unitScale;

            return new Rect(-width * 0.5f, -height * 0.5f, width, height);
        }
    }
}
