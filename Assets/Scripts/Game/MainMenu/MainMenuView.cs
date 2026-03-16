using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.MainMenu
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
        [SerializeField] private Button _playButton;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _levelText;

        public event Action OnSettingsClicked;
        public event Action OnPopupClicked;
        public event Action OnPlayClicked;

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            _popupButton.onClick.AddListener(() => OnPopupClicked?.Invoke());
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        }

        public void UpdateTitle(string text)
        {
            _titleText.text = text;
        }

        public void UpdateLevelDisplay(string text)
        {
            _levelText.text = text;
        }
    }
}
