using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity MonoBehaviour implementation of ILevelCompleteView.
    /// Text-stub UI for level complete popup.
    /// </summary>
    public class LevelCompleteView : MonoBehaviour, ILevelCompleteView
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _goldenPiecesText;

        public event Action OnContinueClicked;

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void UpdateScore(string text) => _scoreText.text = text;
        public void UpdateLevel(string text) => _levelText.text = text;
        public void UpdateGoldenPieces(string text) => _goldenPiecesText.text = text;
    }
}
