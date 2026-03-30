using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// ScriptableObject that controls auto-tracking camera behaviour.
    /// Create an asset via Assets → Create → SimpleGame → CameraConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "SimpleGame/CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        /// <summary>SmoothDamp smooth time for both position and orthographic-size transitions.</summary>
        [SerializeField] public float SmoothTime = 1.2f;

        /// <summary>Minimum allowed orthographic size (most zoomed-in).</summary>
        [SerializeField] public float MinZoom = 2f;

        /// <summary>Maximum allowed orthographic size (most zoomed-out).</summary>
        [SerializeField] public float MaxZoom = 15f;

        /// <summary>World-unit padding added around the bounding box of target positions.</summary>
        [SerializeField] public float Padding = 1.5f;

        /// <summary>
        /// Extra world-unit margin kept between the viewport edge and the board boundary
        /// when clamping manual pan. Prevents the camera from drifting entirely off-board.
        /// </summary>
        [SerializeField] public float BoundaryMargin = 0.5f;

        /// <summary>
        /// Multiplier applied to the raw scroll/pinch delta each frame when the player
        /// manually zooms. Larger values feel snappier.
        /// </summary>
        [SerializeField] public float ZoomSpeed = 5f;
    }
}
