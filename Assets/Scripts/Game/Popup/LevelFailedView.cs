using System;
using SimpleGame.Core.MVP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of ILevelFailedView.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class LevelFailedView : PopupViewBase, ILevelFailedView
    {
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _continueCostText;

        public event Action OnRetryClicked;
        public event Action OnWatchAdClicked;
        public event Action OnQuitClicked;
        public event Action OnContinueClicked;

        private void Awake()
        {
            _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            _watchAdButton.onClick.AddListener(() => OnWatchAdClicked?.Invoke());
            _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void UpdateScore(string text) => _scoreText.text = text;
        public void UpdateLevel(string text) => _levelText.text = text;
        public void UpdateContinueCost(string text)
        {
            if (_continueCostText != null) _continueCostText.text = text;
        }
    }
}
