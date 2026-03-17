using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IPopupContainer&lt;PopupId&gt; and IViewResolver.
    /// Shows and hides pre-instantiated popup GameObjects via SetActive.
    /// All popups live in the Boot scene and start inactive.
    /// Uses a switch on PopupId — add new cases as new popups are introduced.
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

        public UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            var popup = GetPopupObject(popupId);
            if (popup != null)
                popup.SetActive(true);
            return UniTask.CompletedTask;
        }

        public UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            var popup = GetPopupObject(popupId);
            if (popup != null)
                popup.SetActive(false);
            return UniTask.CompletedTask;
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
