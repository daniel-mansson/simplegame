using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    public class LoseDialogView : MonoBehaviour, ILoseDialogView
    {
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;

        public event Action OnRetryClicked;
        public event Action OnBackClicked;

        private void Awake()
        {
            _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public void UpdateScore(string text) => _scoreText.text = text;
        public void UpdateLevel(string text) => _levelText.text = text;
    }
}
