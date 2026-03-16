using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    public class WinDialogView : MonoBehaviour, IWinDialogView
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;

        public event Action OnContinueClicked;

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void UpdateScore(string text) => _scoreText.text = text;
        public void UpdateLevel(string text) => _levelText.text = text;
    }
}
