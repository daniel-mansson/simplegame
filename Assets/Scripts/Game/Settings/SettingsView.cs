using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Settings
{
    /// <summary>
    /// Unity MonoBehaviour implementation of ISettingsView.
    /// Has zero references to presenters, services, or managers.
    ///
    /// Platform link buttons are optional — if not wired in the scene they are silently ignored.
    /// </summary>
    public class SettingsView : MonoBehaviour, ISettingsView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _titleText;

        [Header("Platform Linking (optional)")]
        [SerializeField] private Button _linkGameCenterButton;
        [SerializeField] private Button _linkGooglePlayButton;
        [SerializeField] private Button _unlinkGameCenterButton;
        [SerializeField] private Button _unlinkGooglePlayButton;
        [SerializeField] private Text _gameCenterStatusText;
        [SerializeField] private Text _googlePlayStatusText;

        public event Action OnBackClicked;
        public event Action OnLinkGameCenterClicked;
        public event Action OnLinkGooglePlayClicked;
        public event Action OnUnlinkGameCenterClicked;
        public event Action OnUnlinkGooglePlayClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());

            _linkGameCenterButton?.onClick.AddListener(() => OnLinkGameCenterClicked?.Invoke());
            _linkGooglePlayButton?.onClick.AddListener(() => OnLinkGooglePlayClicked?.Invoke());
            _unlinkGameCenterButton?.onClick.AddListener(() => OnUnlinkGameCenterClicked?.Invoke());
            _unlinkGooglePlayButton?.onClick.AddListener(() => OnUnlinkGooglePlayClicked?.Invoke());
        }

        public void UpdateTitle(string text)
        {
            _titleText.text = text;
        }

        public void UpdateLinkStatus(bool gameCenterLinked, bool googlePlayLinked)
        {
            if (_gameCenterStatusText != null)
                _gameCenterStatusText.text = gameCenterLinked ? "Game Center: Linked" : "Game Center: Not Linked";
            if (_googlePlayStatusText != null)
                _googlePlayStatusText.text = googlePlayLinked ? "Google Play: Linked" : "Google Play: Not Linked";

            // Toggle link/unlink button visibility
            if (_linkGameCenterButton != null)
                _linkGameCenterButton.gameObject.SetActive(!gameCenterLinked);
            if (_unlinkGameCenterButton != null)
                _unlinkGameCenterButton.gameObject.SetActive(gameCenterLinked);
            if (_linkGooglePlayButton != null)
                _linkGooglePlayButton.gameObject.SetActive(!googlePlayLinked);
            if (_unlinkGooglePlayButton != null)
                _unlinkGooglePlayButton.gameObject.SetActive(googlePlayLinked);
        }
    }
}
