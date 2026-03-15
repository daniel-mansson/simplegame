using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using UnityEngine;

namespace SimpleGame.Runtime.PopupManagement
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IPopupContainer.
    /// Shows and hides pre-instantiated popup GameObjects via SetActive.
    /// All popups live in the Boot scene and start inactive.
    /// Uses a switch on PopupId — add new cases as new popups are introduced.
    /// </summary>
    public class UnityPopupContainer : MonoBehaviour, IPopupContainer
    {
        [SerializeField] private GameObject _confirmDialogPopup;

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

        private GameObject GetPopupObject(PopupId popupId)
        {
            switch (popupId)
            {
                case PopupId.ConfirmDialog:
                    return _confirmDialogPopup;
                default:
                    Debug.LogWarning($"[UnityPopupContainer] No GameObject registered for PopupId: {popupId}");
                    return null;
            }
        }
    }
}
