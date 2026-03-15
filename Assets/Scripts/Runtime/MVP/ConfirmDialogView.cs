using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Runtime.MVP
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IConfirmDialogView.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class ConfirmDialogView : MonoBehaviour, IConfirmDialogView
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Text _messageText;

        public event Action OnConfirmClicked;
        public event Action OnCancelClicked;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(() => OnConfirmClicked?.Invoke());
            _cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
        }

        public void UpdateMessage(string text)
        {
            _messageText.text = text;
        }
    }
}
