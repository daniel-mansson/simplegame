using System;
using SimpleGame.Core.MVP;
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
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;

        public event Action OnRetryClicked;
        public event Action OnWatchAdClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            _watchAdButton.onClick.AddListener(() => OnWatchAdClicked?.Invoke());
            _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void UpdateScore(string text) => _scoreText.text = text;
        public void UpdateLevel(string text) => _levelText.text = text;
    }
}
