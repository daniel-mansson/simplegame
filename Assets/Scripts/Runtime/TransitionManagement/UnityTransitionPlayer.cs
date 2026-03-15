using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.TransitionManagement;
using UnityEngine;

namespace SimpleGame.Runtime.TransitionManagement
{
    /// <summary>
    /// Unity implementation of <see cref="ITransitionPlayer"/>.
    /// Uses a <see cref="CanvasGroup"/> to fade a full-screen overlay in and out.
    /// Place this component on a high-sort-order Canvas in the persistent scene (wired in S05).
    ///
    /// Input blocking is NOT performed here — that is <c>IInputBlocker</c>'s responsibility.
    /// <c>blocksRaycasts</c> is explicitly kept <c>false</c> so the overlay never steals input.
    /// </summary>
    public class UnityTransitionPlayer : MonoBehaviour, ITransitionPlayer
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;

        /// <summary>
        /// Fades the overlay from transparent to opaque (screen goes dark).
        /// Activates the overlay GameObject before the fade begins and leaves it active
        /// at alpha 1 when done.
        /// </summary>
        public async UniTask FadeOutAsync(CancellationToken ct = default)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                _canvasGroup.blocksRaycasts = false;
                await UniTask.Yield(ct);
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Fades the overlay from opaque to transparent (screen becomes visible).
        /// Deactivates the overlay GameObject after the fade completes so it is
        /// invisible and non-interacting while no transition is in progress.
        /// </summary>
        public async UniTask FadeInAsync(CancellationToken ct = default)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / _fadeDuration));
                _canvasGroup.blocksRaycasts = false;
                await UniTask.Yield(ct);
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.gameObject.SetActive(false);
        }
    }
}
