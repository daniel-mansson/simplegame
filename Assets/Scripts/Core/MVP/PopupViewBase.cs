using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace SimpleGame.Core.MVP
{
    /// <summary>
    /// Abstract base class for popup views. Provides default LitMotion-driven
    /// entrance and exit animations driven by an optional <see cref="PopupAnimationConfig"/>.
    ///
    /// <b>Enter</b>: panel slides up from an Y offset with OutBounce ease.<br/>
    /// <b>Exit</b>: panel scales down and fades alpha to 0 concurrently with InBack ease.
    ///
    /// Wire <see cref="_canvasGroup"/> and <see cref="_panel"/> in the Inspector or via
    /// SceneSetup. If either is null the animation is skipped with a warning.<br/>
    /// Wire <see cref="_animConfig"/> for project-wide tuning; if null, built-in defaults
    /// matching <see cref="PopupAnimationConfig"/> field defaults are used.
    ///
    /// Override either method in a concrete subclass to replace the default entirely.
    /// </summary>
    public abstract class PopupViewBase : MonoBehaviour, IPopupView
    {
        // ── Fallback constants (match PopupAnimationConfig field defaults) ────

        private const float FallbackAnimInDuration  = 0.4f;
        private const float FallbackAnimInOffsetY   = -80f;
        private const Ease  FallbackAnimInEase      = Ease.OutBounce;
        private const float FallbackAnimOutDuration = 0.25f;
        private const float FallbackAnimOutScale    = 0.85f;
        private const Ease  FallbackAnimOutEase     = Ease.InBack;

        // ── Inspector fields ─────────────────────────────────────────────────

        [SerializeField] protected CanvasGroup    _canvasGroup;
        [SerializeField] protected RectTransform  _panel;

        [Tooltip("Animation config asset. Leave null to use built-in defaults.")]
        [SerializeField] private PopupAnimationConfig _animConfig;

        // ── Convenience accessors ─────────────────────────────────────────────

        private float AnimInDuration  => _animConfig != null ? _animConfig.animInDuration  : FallbackAnimInDuration;
        private float AnimInOffsetY   => _animConfig != null ? _animConfig.animInOffsetY   : FallbackAnimInOffsetY;
        private Ease  AnimInEase      => _animConfig != null ? _animConfig.animInEase      : FallbackAnimInEase;
        private float AnimOutDuration => _animConfig != null ? _animConfig.animOutDuration : FallbackAnimOutDuration;
        private float AnimOutScale    => _animConfig != null ? _animConfig.animOutScale    : FallbackAnimOutScale;
        private Ease  AnimOutEase     => _animConfig != null ? _animConfig.animOutEase     : FallbackAnimOutEase;

        // ── IPopupView ────────────────────────────────────────────────────────

        /// <summary>
        /// Default entrance: slide panel up from below with the configured ease.
        /// </summary>
        public virtual async UniTask AnimateInAsync(CancellationToken ct = default)
        {
            if (!ValidateRefs("AnimateInAsync")) return;

            // Reset state before animating in
            _canvasGroup.alpha  = 1f;
            _panel.localScale   = Vector3.one;

            var restingY = _panel.anchoredPosition.y;
            var startPos = new Vector2(_panel.anchoredPosition.x, restingY + AnimInOffsetY);
            var endPos   = new Vector2(_panel.anchoredPosition.x, restingY);

            _panel.anchoredPosition = startPos;

            await LMotion.Create(startPos, endPos, AnimInDuration)
                .WithEase(AnimInEase)
                .BindToAnchoredPosition(_panel)
                .ToUniTask(ct);
        }

        /// <summary>
        /// Default exit: scale panel down to configured scale and fade alpha to 0 concurrently.
        /// </summary>
        public virtual async UniTask AnimateOutAsync(CancellationToken ct = default)
        {
            if (!ValidateRefs("AnimateOutAsync")) return;

            var targetScale = new Vector3(AnimOutScale, AnimOutScale, 1f);

            var scaleHandle = LMotion.Create(Vector3.one, targetScale, AnimOutDuration)
                .WithEase(AnimOutEase)
                .BindToLocalScale(_panel);

            var alphaHandle = LMotion.Create(1f, 0f, AnimOutDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x);

            await UniTask.WhenAll(scaleHandle.ToUniTask(ct), alphaHandle.ToUniTask(ct));

            // Reset for next open
            _panel.localScale   = Vector3.one;
            _canvasGroup.alpha  = 1f;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool ValidateRefs(string caller)
        {
            if (_canvasGroup == null || _panel == null)
            {
                Debug.LogWarning($"[{GetType().Name}] {caller}: _canvasGroup or _panel is null — animation skipped. Wire these fields in the Inspector or SceneSetup.");
                return false;
            }
            return true;
        }
    }
}
