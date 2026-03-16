using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SimpleGame.Core.TransitionManagement;
using UnityEngine;

namespace SimpleGame.Core.Unity.TransitionManagement
{
    /// <summary>
    /// Unity implementation of <see cref="ITransitionPlayer"/>.
    /// Uses a <see cref="CanvasGroup"/> to fade a full-screen overlay in and out,
    /// driven by LitMotion tweening.
    ///
    /// This component lives on a self-contained transition prefab. The prefab owns
    /// all visual elements (Canvas, CanvasGroup, Image, etc.). Swapping the prefab
    /// changes the transition look without touching callers or the API.
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

            await LMotion.Create(0f, 1f, _fadeDuration)
                .BindToAlpha(_canvasGroup)
                .ToUniTask(cancellationToken: ct);

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

            await LMotion.Create(1f, 0f, _fadeDuration)
                .BindToAlpha(_canvasGroup)
                .ToUniTask(cancellationToken: ct);

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.gameObject.SetActive(false);
        }
    }
}
