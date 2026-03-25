using System;
using SimpleGame.Core.MVP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of IShopView.
    /// Displays three coin pack buttons and a cancel button.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class ShopView : PopupViewBase, IShopView
    {
        [SerializeField] private Button[] _packButtons;   // expects 3 elements
        [SerializeField] private TMP_Text[] _packLabels;  // expects 3 elements (children of pack buttons)
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _titleText;

        public event Action<int> OnPackClicked;
        public event Action OnCancelClicked;

        private void Awake()
        {
            if (_packButtons != null)
            {
                for (int i = 0; i < _packButtons.Length; i++)
                {
                    var index = i; // capture for closure
                    if (_packButtons[i] != null)
                        _packButtons[i].onClick.AddListener(() => OnPackClicked?.Invoke(index));
                }
            }

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke());
        }

        public void UpdatePackLabel(int packIndex, string text)
        {
            if (_packLabels != null && packIndex >= 0 && packIndex < _packLabels.Length && _packLabels[packIndex] != null)
                _packLabels[packIndex].text = text;
        }

        public void SetPackVisible(int packIndex, bool visible)
        {
            if (_packButtons != null && packIndex >= 0 && packIndex < _packButtons.Length && _packButtons[packIndex] != null)
                _packButtons[packIndex].gameObject.SetActive(visible);
        }

        public void UpdateStatus(string text)
        {
            if (_statusText != null)
                _statusText.text = text;
        }
    }
}
