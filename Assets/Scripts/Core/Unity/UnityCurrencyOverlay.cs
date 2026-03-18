using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using SimpleGame.Core;
using TMPro;
using UnityEngine;

namespace SimpleGame.Core.Unity
{
    /// <summary>
    /// Unity MonoBehaviour implementation of <see cref="ICurrencyOverlay"/>.
    /// Sits on its own Canvas at sort order 120 — above the popup blocker (100)
    /// but below actively-stacked second popups (150+).
    ///
    /// Fades the overlay CanvasGroup alpha with LitMotion (0 ↔ 1).
    /// Starts hidden (alpha = 0, blocksRaycasts = false).
    /// </summary>
    public class UnityCurrencyOverlay : MonoBehaviour, ICurrencyOverlay
    {
        private const float FadeDuration = 0.2f;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _balanceText;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <inheritdoc/>
        public UniTask ShowAsync(CancellationToken ct = default)
        {
            if (_canvasGroup == null) return UniTask.CompletedTask;

            _canvasGroup.blocksRaycasts = true;
            return LMotion.Create(_canvasGroup.alpha, 1f, FadeDuration)
                .WithEase(Ease.Linear)
                .Bind(x => _canvasGroup.alpha = x)
                .ToUniTask(ct);
        }

        /// <inheritdoc/>
        public UniTask HideAsync(CancellationToken ct = default)
        {
            if (_canvasGroup == null) return UniTask.CompletedTask;

            return LMotion.Create(_canvasGroup.alpha, 0f, FadeDuration)
                .WithEase(Ease.Linear)
                .Bind(x =>
                {
                    _canvasGroup.alpha = x;
                    if (x <= 0f) _canvasGroup.blocksRaycasts = false;
                })
                .ToUniTask(ct);
        }

        /// <inheritdoc/>
        public void UpdateBalance(string text)
        {
            if (_balanceText != null)
                _balanceText.text = text;
        }
    }
}
