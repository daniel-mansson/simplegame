using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IRewardedAdView.
    /// Text-stub UI for rewarded ad popup.
    /// </summary>
    public class RewardedAdView : MonoBehaviour, IRewardedAdView
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
