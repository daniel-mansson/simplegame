using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Runtime.MVP
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IMainMenuView.
    /// Owns wiring between Unity UI components and view interface events.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _popupButton;
        [SerializeField] private Text _titleText;

        public event Action OnSettingsClicked;
        public event Action OnPopupClicked;

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            _popupButton.onClick.AddListener(() => OnPopupClicked?.Invoke());
        }

        public void UpdateTitle(string text)
        {
            _titleText.text = text;
        }
    }
}
