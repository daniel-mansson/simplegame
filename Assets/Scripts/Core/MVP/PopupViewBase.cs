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
    /// Wire <see cref="_animConfig"/> for project-wide tuning. A default asset is assigned
    /// on the script itself so every new instance is pre-wired; it should only be null if
    /// deliberately cleared in the Inspector.
    ///
    /// Override either method in a concrete subclass to replace the default entirely.
    /// </summary>
    public abstract class PopupViewBase : MonoBehaviour, IPopupView
    {
        // ── Inspector fields ─────────────────────────────────────────────────

        [SerializeField] protected CanvasGroup    _canvasGroup;
        [SerializeField] protected RectTransform  _panel;

        [Tooltip("Animation config asset. A default is pre-assigned via the script's default references.")]
        [SerializeField] private PopupAnimationConfig _animConfig;

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
            var startPos = new Vector2(_panel.anchoredPosition.x, restingY + _animConfig.animInOffsetY);
            var endPos   = new Vector2(_panel.anchoredPosition.x, restingY);

            _panel.anchoredPosition = startPos;

            await LMotion.Create(startPos, endPos, _animConfig.animInDuration)
                .WithEase(_animConfig.animInEase)
                .BindToAnchoredPosition(_panel)
                .ToUniTask(ct);
        }

        /// <summary>
        /// Default exit: scale panel down to configured scale and fade alpha to 0 concurrently.
        /// </summary>
        public virtual async UniTask AnimateOutAsync(CancellationToken ct = default)
        {
            if (!ValidateRefs("AnimateOutAsync")) return;

            var targetScale = new Vector3(_animConfig.animOutScale, _animConfig.animOutScale, 1f);

            var scaleHandle = LMotion.Create(Vector3.one, targetScale, _animConfig.animOutDuration)
                .WithEase(_animConfig.animOutEase)
                .BindToLocalScale(_panel);

            var alphaHandle = LMotion.Create(1f, 0f, _animConfig.animOutDuration)
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
            if (_animConfig == null)
            {
                Debug.LogError($"[{GetType().Name}] {caller}: _animConfig is null — animation skipped. Assign a PopupAnimationConfig asset in the Inspector.");
                return false;
            }
            return true;
        }
    }
}
