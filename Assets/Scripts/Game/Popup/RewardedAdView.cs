using System;
using SimpleGame.Core.MVP;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of IRewardedAdView.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class RewardedAdView : PopupViewBase, IRewardedAdView
    {
        [SerializeField] private Button _watchButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Text _statusText;

        public event Action OnWatchClicked;
        public event Action OnSkipClicked;

        private void Awake()
        {
            _watchButton.onClick.AddListener(() => OnWatchClicked?.Invoke());
            _skipButton.onClick.AddListener(() => OnSkipClicked?.Invoke());
        }

        public void UpdateStatus(string text) => _statusText.text = text;
    }
}
