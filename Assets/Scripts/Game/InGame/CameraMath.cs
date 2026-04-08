using System.Collections.Generic;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Pure-math helpers for perspective camera framing (fixed FOV, Z-based zoom).
    /// No Unity object dependencies — safe to use in EditMode tests.
    /// </summary>
    public static class CameraMath
    {
        /// <summary>
        /// Compute the visible half-height at a given Z distance for a perspective camera.
        /// </summary>
        public static float FrustumHalfHeight(float z, float fovDegrees)
            => z * Mathf.Tan(fovDegrees * 0.5f * Mathf.Deg2Rad);

        /// <summary>
        /// Compute the Z distance needed to see a given half-height with a perspective camera.
        /// </summary>
        public static float ZForHalfHeight(float halfHeight, float fovDegrees)
            => halfHeight / Mathf.Tan(fovDegrees * 0.5f * Mathf.Deg2Rad);

        /// <summary>
        /// Compute the camera center and Z distance needed to frame an entire
        /// puzzle board defined by <paramref name="boardRect"/>.
        /// </summary>
        public static (Vector3 center, float z) ComputeFullBoardFraming(
            Rect boardRect, float padding, float aspect, float fovDegrees, float minZ, float maxZ)
        {
            var center = new Vector3(boardRect.center.x, boardRect.center.y, 0f);

            float requiredHalfH = (boardRect.height + 2f * padding) * 0.5f;
            float requiredHalfW = (boardRect.width  + 2f * padding) * 0.5f;
            float halfHFromWidth = requiredHalfW / aspect;

            float halfH = Mathf.Max(requiredHalfH, halfHFromWidth);
            float z = ZForHalfHeight(halfH, fovDegrees);

            z = Mathf.Clamp(z, minZ, maxZ);
            return (center, z);
        }

        /// <summary>
        /// Compute the camera center and Z distance needed to frame all
        /// <paramref name="positions"/> with the given <paramref name="padding"/>.
        /// </summary>
        public static (Vector3 center, float z) ComputeFraming(
            IReadOnlyList<Vector3> positions,
            float padding,
            float aspect,
            float fovDegrees,
            float minZ,
            float maxZ)
        {
            if (positions == null || positions.Count == 0)
                return (Vector3.zero, minZ);

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

            float requiredHalfH = (spanY + 2f * padding) * 0.5f;
            float requiredHalfW = (spanX + 2f * padding) * 0.5f;
            float halfHFromWidth = requiredHalfW / aspect;

            float halfH = Mathf.Max(requiredHalfH, halfHFromWidth);
            float z = ZForHalfHeight(halfH, fovDegrees);

            z = Mathf.Clamp(z, minZ, maxZ);
            return (center, z);
        }

        /// <summary>
        /// Clamp a proposed camera XY position so that the visible viewport (sized by
        /// perspective frustum at the given Z distance) stays within
        /// <paramref name="bounds"/> plus a <paramref name="margin"/> on every side.
        /// When the viewport is larger than the board in a given axis the camera is
        /// centred on the board in that axis.
        /// </summary>
        public static Vector3 ClampToBounds(
            Vector3 cameraPos,
            float   z,
            float   fovDegrees,
            float   aspect,
            Rect    bounds,
            float   margin)
        {
            float halfH = FrustumHalfHeight(z, fovDegrees);
            float halfW = halfH * aspect;

            float boardHalfW = bounds.width  * 0.5f;
            float boardHalfH = bounds.height * 0.5f;
            float cx = bounds.center.x;
            float cy = bounds.center.y;

            float x;
            if (halfW >= boardHalfW + margin)
                x = cx;
            else
            {
                float lo = bounds.xMin + margin + halfW;
                float hi = bounds.xMax - margin - halfW;
                x = Mathf.Clamp(cameraPos.x, lo, hi);
            }

            float y;
            if (halfH >= boardHalfH + margin)
                y = cy;
            else
            {
                float lo = bounds.yMin + margin + halfH;
                float hi = bounds.yMax - margin - halfH;
                y = Mathf.Clamp(cameraPos.y, lo, hi);
            }

            return new Vector3(x, y, cameraPos.z);
        }

        /// <summary>
        /// Compute the world-space Rect that encloses a puzzle board of
        /// <paramref name="rows"/> × <paramref name="cols"/> cells, using the same
        /// unit-based convention: the board is centred on the origin
        /// and each cell is 1 / max(rows, cols) world units so that the
        /// longest side always spans exactly 1 world unit.
        /// </summary>
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
