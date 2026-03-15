using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Runtime.MVP
{
    /// <summary>
    /// Unity MonoBehaviour implementation of ISettingsView.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class SettingsView : MonoBehaviour, ISettingsView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _titleText;

        public event Action OnBackClicked;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public void UpdateTitle(string text)
        {
            _titleText.text = text;
        }
    }
}
