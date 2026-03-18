using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of IObjectRestoredView.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class ObjectRestoredView : PopupViewBase, IObjectRestoredView
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
