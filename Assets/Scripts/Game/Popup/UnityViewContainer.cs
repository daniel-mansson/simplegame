using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.Unity.PopupManagement;
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
    /// Sort Order for Stacking (D053 — revised):
    ///   Each popup root has a Canvas added with overrideSorting=true.
    ///   Sort order = BasePopupSortOrder + (stackDepth * SortOrderStep).
    ///     BasePopupSortOrder = 200, SortOrderStep = 100
    ///     → depth 0 = 200, depth 1 = 300, etc.
    ///   Blocker Canvas base sort order = 100 (set externally via SceneSetup).
    ///   When a second popup is stacked (depth ≥ 1 already visible), the blocker
    ///   sort order is raised to BlockerStackedSortOrder = 250 so it sits between
    ///   the bottom popup (200) and the top popup (300), visually dimming the bottom.
    ///   When the stack returns to ≤ 1 popup, the blocker is reset to 100.
    ///
    /// If a popup GameObject has no IPopupView component, it shows/hides instantly
    /// with a warning — safe fallback.
    ///
    /// Get&lt;T&gt;() resolves view interfaces via GetComponentInChildren&lt;T&gt;(true),
    /// which searches inactive children — no manual registration required.
    /// </summary>
    public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver
    {
        private const int BasePopupSortOrder    = 200;
        private const int SortOrderStep         = 100;
        private const int BlockerBaseSortOrder  = 100;
        private const int BlockerStackedSortOrder = 250;  // Between depth-0 (200) and depth-1 (300)

        [SerializeField] private GameObject _confirmDialogPopup;
        [SerializeField] private GameObject _levelCompletePopup;
        [SerializeField] private GameObject _levelFailedPopup;
        [SerializeField] private GameObject _rewardedAdPopup;
        [SerializeField] private GameObject _iapPurchasePopup;
        [SerializeField] private GameObject _objectRestoredPopup;
        [SerializeField] private GameObject _shopPopup;
        [SerializeField] private GameObject _consentGatePopup;

        [Tooltip("Reference to the input blocker so its sort order can be adjusted when popups stack.")]
        [SerializeField] private UnityInputBlocker _inputBlocker;

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

            // Raise blocker between the two popups when stacking
            UpdateBlockerSortOrder();

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

            // Reset blocker sort order when no longer stacking
            UpdateBlockerSortOrder();
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

        private void UpdateBlockerSortOrder()
        {
            if (_inputBlocker == null) return;
            // When 2+ popups visible, raise blocker to sit between bottom (200) and top (300)
            int blockerOrder = _visiblePopupCount >= 2 ? BlockerStackedSortOrder : BlockerBaseSortOrder;
            _inputBlocker.SetSortOrder(blockerOrder);
        }

        private static void AssignSortOrder(GameObject popup, int depthIndex)
        {
            var sortOrder = BasePopupSortOrder + (depthIndex * SortOrderStep);

            // Add or reuse Canvas on the popup root for sort order override.
            // A nested Canvas with overrideSorting=true detaches from the parent canvas
            // for rendering AND input — it needs its own GraphicRaycaster so buttons
            // inside it receive pointer events (parent's raycaster won't reach them).
            var canvas = popup.GetComponent<Canvas>();
            if (canvas == null)
                canvas = popup.AddComponent<Canvas>();

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;

            if (popup.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                popup.AddComponent<UnityEngine.UI.GraphicRaycaster>();
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
                case PopupId.ConsentGate:    return _consentGatePopup;
                default:
                    Debug.LogWarning($"[UnityViewContainer] No GameObject registered for PopupId: {popupId}");
                    return null;
            }
        }
    }
}
