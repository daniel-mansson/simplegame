using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of IIAPPurchaseView.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class IAPPurchaseView : PopupViewBase, IIAPPurchaseView
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _statusText;

        public event Action OnPurchaseClicked;
        public event Action OnCancelClicked;

        private void Awake()
        {
            _purchaseButton.onClick.AddListener(() => OnPurchaseClicked?.Invoke());
            _cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
        }

        public void UpdateItemName(string text) => _itemNameText.text = text;
        public void UpdatePrice(string text) => _priceText.text = text;
        public void UpdateStatus(string text) => _statusText.text = text;
    }
}
