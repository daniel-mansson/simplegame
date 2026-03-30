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
    }
}
