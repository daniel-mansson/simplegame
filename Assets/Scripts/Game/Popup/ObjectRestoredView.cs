using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IObjectRestoredView.
    /// Text-stub UI for object restored celebration popup.
    /// </summary>
    public class ObjectRestoredView : MonoBehaviour, IObjectRestoredView
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Text _objectNameText;

        public event Action OnContinueClicked;

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void UpdateObjectName(string text) => _objectNameText.text = text;
    }
}
