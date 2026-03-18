using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace SimpleGame.Core.MVP
{
    /// <summary>
    /// Abstract base class for popup views. Provides default LitMotion-driven
    /// entrance and exit animations:
    ///   - AnimateInAsync:  panel slides up from -80px below with OutBounce ease (0.4s)
    ///   - AnimateOutAsync: panel scales to 0.85 + fades alpha to 0 with InBack ease (0.25s),
    ///                      both tweens run concurrently
    ///
    /// Wire <see cref="_canvasGroup"/> and <see cref="_panel"/> in the Inspector (or via
    /// SceneSetup). If either reference is null the animation is skipped with a warning.
    ///
    /// Override either method in a concrete subclass to replace the default.
    /// </summary>
    public abstract class PopupViewBase : MonoBehaviour, IPopupView
    {
        private const float AnimInDuration   = 0.4f;
        private const float AnimOutDuration  = 0.25f;
        private const float AnimInOffsetY    = -80f;
        private const float AnimOutScale     = 0.85f;

        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _panel;

        // ── IPopupView ────────────────────────────────────────────────────────

        /// <summary>
        /// Default entrance: slide panel up from below with OutBounce, restore full alpha.
        /// </summary>
        public virtual async UniTask AnimateInAsync(CancellationToken ct = default)
        {
            if (!ValidateRefs("AnimateInAsync")) return;

            // Reset state before animating in
            _canvasGroup.alpha = 1f;
            _panel.localScale   = Vector3.one;

            var startPos = new Vector2(_panel.anchoredPosition.x, _panel.anchoredPosition.y + AnimInOffsetY);
            var endPos   = new Vector2(_panel.anchoredPosition.x, _panel.anchoredPosition.y);

            _panel.anchoredPosition = startPos;

            await LMotion.Create(startPos, endPos, AnimInDuration)
                .WithEase(Ease.OutBounce)
                .BindToAnchoredPosition(_panel)
                .ToUniTask(ct);
        }

        /// <summary>
        /// Default exit: scale panel down to 0.85 and fade alpha to 0 concurrently (InBack).
        /// </summary>
        public virtual async UniTask AnimateOutAsync(CancellationToken ct = default)
        {
            if (!ValidateRefs("AnimateOutAsync")) return;

            var scaleHandle = LMotion.Create(Vector3.one, new Vector3(AnimOutScale, AnimOutScale, 1f), AnimOutDuration)
                .WithEase(Ease.InBack)
                .BindToLocalScale(_panel);

            var alphaHandle = LMotion.Create(1f, 0f, AnimOutDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x);

            await UniTask.WhenAll(scaleHandle.ToUniTask(ct), alphaHandle.ToUniTask(ct));

            // Reset for next open
            _panel.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;
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
