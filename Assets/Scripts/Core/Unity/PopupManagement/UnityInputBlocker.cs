using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
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
    /// The CanvasGroup alpha starts at 0 (transparent). FadeIn animates to 0.5 (dim);
    /// FadeOut animates back to 0. blocksRaycasts is managed by Block()/Unblock().
    /// </summary>
    public class UnityInputBlocker : MonoBehaviour, IInputBlocker
    {
        private const float FadedAlpha    = 0.5f;
        private const float FadeInDuration  = 0.2f;
        private const float FadeOutDuration = 0.2f;

        [SerializeField] private CanvasGroup _canvasGroup;

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

            return LMotion.Create(_canvasGroup.alpha, FadedAlpha, FadeInDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x)
                .ToUniTask(ct);
        }

        /// <inheritdoc />
        public UniTask FadeOutAsync(CancellationToken ct = default)
        {
            if (_canvasGroup == null) return UniTask.CompletedTask;

            return LMotion.Create(_canvasGroup.alpha, 0f, FadeOutDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x)
                .ToUniTask(ct);
        }
    }
}
