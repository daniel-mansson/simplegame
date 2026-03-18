using LitMotion;
using UnityEngine;

namespace SimpleGame.Core.MVP
{
    /// <summary>
    /// ScriptableObject holding default animation parameters for the popup system.
    ///
    /// <b>Popup enter animation</b> (AnimateInAsync in PopupViewBase):
    ///   panel slides up from <see cref="animInOffsetY"/> below its final position
    ///   using <see cref="animInEase"/> over <see cref="animInDuration"/> seconds.
    ///
    /// <b>Popup exit animation</b> (AnimateOutAsync in PopupViewBase):
    ///   panel scales to <see cref="animOutScale"/> and fades alpha to 0
    ///   using <see cref="animOutEase"/> over <see cref="animOutDuration"/> seconds.
    ///
    /// <b>Blocker overlay fade</b> (UnityInputBlocker):
    ///   alpha animates 0 → <see cref="blockerFadedAlpha"/> on open,
    ///   and back to 0 on close, over <see cref="blockerFadeDuration"/> seconds.
    ///
    /// Create via: <c>Assets/Create/SimpleGame/Popup Animation Config</c>
    /// </summary>
    [CreateAssetMenu(
        fileName = "PopupAnimationConfig",
        menuName  = "SimpleGame/Popup Animation Config",
        order     = 100)]
    public class PopupAnimationConfig : ScriptableObject
    {
        [Header("Popup — Enter")]
        [Tooltip("Duration of the enter animation in seconds.")]
        [Min(0f)]
        public float animInDuration = 0.4f;

        [Tooltip("Y offset (pixels) the panel starts below its resting position.")]
        public float animInOffsetY = -80f;

        [Tooltip("Ease curve used for the enter slide.")]
        public Ease animInEase = Ease.OutBounce;

        [Header("Popup — Exit")]
        [Tooltip("Duration of the exit animation in seconds.")]
        [Min(0f)]
        public float animOutDuration = 0.25f;

        [Tooltip("Uniform scale the panel shrinks to during exit.")]
        [Range(0f, 1f)]
        public float animOutScale = 0.85f;

        [Tooltip("Ease curve used for the exit scale+fade.")]
        public Ease animOutEase = Ease.InBack;

        [Header("Blocker Overlay")]
        [Tooltip("Peak alpha of the dim overlay when a popup is open.")]
        [Range(0f, 1f)]
        public float blockerFadedAlpha = 0.5f;

        [Tooltip("Duration of the blocker fade-in and fade-out in seconds.")]
        [Min(0f)]
        public float blockerFadeDuration = 0.2f;
    }
}
