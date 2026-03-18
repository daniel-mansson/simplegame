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
    /// Show sequence: SetActive(true) → assign sort order → AnimateInAsync (popup bounces in)
    /// Hide sequence: AnimateOutAsync (popup scales/fades out) → SetActive(false)
    ///
    /// Sort Order for Stacking (D053):
    ///   Each popup root has a Canvas added with overrideSorting=true.
    ///   Sort order = 50 + (stackDepth * 100), where stackDepth is the number of
    ///   already-visible popups at the time of show (0-indexed).
    ///   Bottom popup = 50 (below blocker at 100), second popup = 150 (above blocker).
    ///   This ensures the dim overlay visually separates stacked popups.
    ///
    /// If a popup GameObject has no IPopupView component, it shows/hides instantly
    /// with a warning — safe fallback.
    ///
    /// Get&lt;T&gt;() resolves view interfaces via GetComponentInChildren&lt;T&gt;(true),
    /// which searches inactive children — no manual registration required.
    /// </summary>
    public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver
    {
        private const int BasePopupSortOrder = 50;
        private const int SortOrderStep = 100;

        [SerializeField] private GameObject _confirmDialogPopup;
        [SerializeField] private GameObject _levelCompletePopup;
        [SerializeField] private GameObject _levelFailedPopup;
        [SerializeField] private GameObject _rewardedAdPopup;
        [SerializeField] private GameObject _iapPurchasePopup;
        [SerializeField] private GameObject _objectRestoredPopup;
        [SerializeField] private GameObject _shopPopup;

        // Current number of visible (shown) popups — used for sort order assignment.
        private int _visiblePopupCount;

        public async UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            var popup = GetPopupObject(popupId);
            if (popup == null) return;

            popup.SetActive(true);

            // Assign sort order based on current depth before incrementing
            AssignSortOrder(popup, _visiblePopupCount);
            _visiblePopupCount++;

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
            _visiblePopupCount = Mathf.Max(0, _visiblePopupCount - 1);
        }

        public T Get<T>() where T : class
        {
            return GetComponentInChildren<T>(true);
        }

        /// <summary>
        /// Returns the current sort order that would be assigned to the next popup shown.
        /// Useful for tests to verify the sort order scheme.
        /// </summary>
        public int GetNextPopupSortOrder() => BasePopupSortOrder + (_visiblePopupCount * SortOrderStep);

        private static void AssignSortOrder(GameObject popup, int depthIndex)
        {
            var sortOrder = BasePopupSortOrder + (depthIndex * SortOrderStep);

            // Add or reuse Canvas on the popup root for sort order override
            var canvas = popup.GetComponent<Canvas>();
            if (canvas == null)
                canvas = popup.AddComponent<Canvas>();

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
        }

        private GameObject GetPopupObject(PopupId popupId)
        {
            switch (popupId)
            {
                case PopupId.ConfirmDialog:  return _confirmDialogPopup;
                case PopupId.LevelComplete:  return _levelCompletePopup;
                case PopupId.LevelFailed:    return _levelFailedPopup;
                case PopupId.RewardedAd:     return _rewardedAdPopup;
                case PopupId.IAPPurchase:    return _iapPurchasePopup;
                case PopupId.ObjectRestored: return _objectRestoredPopup;
                case PopupId.Shop:           return _shopPopup;
                default:
                    Debug.LogWarning($"[UnityViewContainer] No GameObject registered for PopupId: {popupId}");
                    return null;
            }
        }
    }
}
