using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IPopupContainer&lt;PopupId&gt; and IViewResolver.
    /// Shows and hides pre-instantiated popup GameObjects, running their entrance and
    /// exit animations via IPopupView.AnimateInAsync / AnimateOutAsync.
    ///
    /// Show sequence: SetActive(true) → AnimateInAsync (popup bounces in)
    /// Hide sequence: AnimateOutAsync (popup scales/fades out) → SetActive(false)
    ///
    /// If a popup GameObject has no IPopupView component, it shows/hides instantly
    /// with a warning — safe fallback for popups not yet migrated to PopupViewBase.
    ///
    /// Get&lt;T&gt;() resolves view interfaces via GetComponentInChildren&lt;T&gt;(true),
    /// which searches inactive children — no manual registration required.
    /// </summary>
    public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver
    {
        [SerializeField] private GameObject _confirmDialogPopup;
        [SerializeField] private GameObject _levelCompletePopup;
        [SerializeField] private GameObject _levelFailedPopup;
        [SerializeField] private GameObject _rewardedAdPopup;
        [SerializeField] private GameObject _iapPurchasePopup;
        [SerializeField] private GameObject _objectRestoredPopup;

        public async UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            var popup = GetPopupObject(popupId);
            if (popup == null) return;

            popup.SetActive(true);

            var view = popup.GetComponentInChildren<IPopupView>(true);
            if (view != null)
                await view.AnimateInAsync(ct);
            else
                Debug.LogWarning($"[UnityViewContainer] No IPopupView found on {popup.name} — showing without animation.");
        }

        public async UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            var popup = GetPopupObject(popupId);
            if (popup == null) return;

            var view = popup.GetComponentInChildren<IPopupView>(true);
            if (view != null)
                await view.AnimateOutAsync(ct);
            else
                Debug.LogWarning($"[UnityViewContainer] No IPopupView found on {popup.name} — hiding without animation.");

            popup.SetActive(false);
        }

        public T Get<T>() where T : class
        {
            return GetComponentInChildren<T>(true);
        }

        private GameObject GetPopupObject(PopupId popupId)
        {
            switch (popupId)
            {
                case PopupId.ConfirmDialog:
                    return _confirmDialogPopup;
                case PopupId.LevelComplete:
                    return _levelCompletePopup;
                case PopupId.LevelFailed:
                    return _levelFailedPopup;
                case PopupId.RewardedAd:
                    return _rewardedAdPopup;
                case PopupId.IAPPurchase:
                    return _iapPurchasePopup;
                case PopupId.ObjectRestored:
                    return _objectRestoredPopup;
                default:
                    Debug.LogWarning($"[UnityViewContainer] No GameObject registered for PopupId: {popupId}");
                    return null;
            }
        }
    }
}
