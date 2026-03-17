using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IIAPPurchaseView.
    /// Text-stub UI for IAP purchase popup.
    /// </summary>
    public class IAPPurchaseView : MonoBehaviour, IIAPPurchaseView
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
