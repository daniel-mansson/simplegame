using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// ScriptableObject that controls perspective camera behaviour.
    /// Create an asset via Assets → Create → SimpleGame → CameraConfig.
    /// Camera uses a fixed FOV and varies Z position for zoom.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "SimpleGame/CameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        /// <summary>SmoothDamp smooth time for position transitions (including Z zoom).</summary>
        [SerializeField] public float SmoothTime = 0.4f;

        /// <summary>Camera field of view in degrees (fixed — zoom is Z-based).</summary>
        [SerializeField] public float FieldOfView = 60f;

        /// <summary>Closest Z distance to the board plane (most zoomed-in). Positive value, camera sits at -MinZ.</summary>
        [SerializeField] public float MinZ = 0.5f;

        /// <summary>Furthest Z distance from the board plane (most zoomed-out). Positive value, camera sits at -MaxZ.</summary>
        [SerializeField] public float MaxZ = 5f;

        /// <summary>World-unit padding added around the bounding box of target positions when framing.</summary>
        [SerializeField] public float Padding = 0.15f;

        /// <summary>
        /// Extra world-unit margin kept between the viewport edge and the board boundary
        /// when clamping manual pan.
        /// </summary>
        [SerializeField] public float BoundaryMargin = 0.1f;

        /// <summary>
        /// Multiplier applied to the raw scroll/pinch delta each frame when the player
        /// manually zooms. Larger values feel snappier.
        /// </summary>
        [SerializeField] public float ZoomSpeed = 5f;

        /// <summary>
        /// World-unit Y offset applied to all camera targets. Positive values shift the
        /// camera up so that the framed content sits above the bottom UI strip.
        /// </summary>
        [SerializeField] public float TargetYOffset = 0.15f;

        /// <summary>
        /// How long (in seconds) the camera holds the full-board overview shot at level start
        /// before animating to the first valid placement area.
        /// </summary>
        [SerializeField] public float OverviewHoldDuration = 1.0f;
    }
}
