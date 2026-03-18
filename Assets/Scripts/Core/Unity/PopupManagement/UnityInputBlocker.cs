using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using UnityEngine;

namespace SimpleGame.Core.Unity.PopupManagement
{
    /// <summary>
    /// Unity implementation of IInputBlocker. Uses a CanvasGroup to block raycasts
    /// and LitMotion to animate the overlay alpha.
    ///
    /// Reference counting (Block/Unblock) controls input blocking.
    /// FadeInAsync / FadeOutAsync animate the visual overlay independently —
    /// the caller is responsible for timing (Block before FadeIn; Unblock before FadeOut).
    ///
    /// The CanvasGroup alpha starts at 0 (transparent). FadeIn animates to
    /// <see cref="PopupAnimationConfig.blockerFadedAlpha"/> (default 0.5); FadeOut returns
    /// to 0. blocksRaycasts is managed by Block()/Unblock().
    ///
    /// Wire <see cref="_animConfig"/> for project-wide tuning; if null, built-in defaults
    /// matching <see cref="PopupAnimationConfig"/> field defaults are used.
    /// </summary>
    public class UnityInputBlocker : MonoBehaviour, IInputBlocker
    {
        // ── Fallback constants (match PopupAnimationConfig field defaults) ────

        private const float FallbackFadedAlpha    = 0.5f;
        private const float FallbackFadeDuration  = 0.2f;

        // ── Inspector fields ─────────────────────────────────────────────────

        [SerializeField] private CanvasGroup         _canvasGroup;

        [Tooltip("Animation config asset. Leave null to use built-in defaults.")]
        [SerializeField] private PopupAnimationConfig _animConfig;

        // ── Convenience accessors ─────────────────────────────────────────────

        private float FadedAlpha    => _animConfig != null ? _animConfig.blockerFadedAlpha   : FallbackFadedAlpha;
        private float FadeDuration  => _animConfig != null ? _animConfig.blockerFadeDuration : FallbackFadeDuration;

        private int _blockCount;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        // ── IInputBlocker — blocking ──────────────────────────────────────────

        /// <inheritdoc />
        public void Block()
        {
            _blockCount++;
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = true;
        }

        /// <inheritdoc />
        public void Unblock()
        {
            _blockCount = Math.Max(0, _blockCount - 1);
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = _blockCount > 0;
        }

        /// <inheritdoc />
        public bool IsBlocked => _blockCount > 0;

        // ── IInputBlocker — fading ────────────────────────────────────────────

        /// <inheritdoc />
        public UniTask FadeInAsync(CancellationToken ct = default)
        {
            if (_canvasGroup == null) return UniTask.CompletedTask;

            return LMotion.Create(_canvasGroup.alpha, FadedAlpha, FadeDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x)
                .ToUniTask(ct);
        }

        /// <inheritdoc />
        public UniTask FadeOutAsync(CancellationToken ct = default)
        {
            if (_canvasGroup == null) return UniTask.CompletedTask;

            return LMotion.Create(_canvasGroup.alpha, 0f, FadeDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x)
                .ToUniTask(ct);
        }

        /// <inheritdoc />
        public void SetSortOrder(int sortOrder)
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.sortingOrder = sortOrder;
        }
    }
}
