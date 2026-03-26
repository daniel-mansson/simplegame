using System;
using SimpleGame.Core.MVP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of ILevelCompleteView.
    /// Inherits default bounce-in / scale-out animations from PopupViewBase.
    /// </summary>
    public class LevelCompleteView : PopupViewBase, ILevelCompleteView
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _goldenPiecesText;

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
