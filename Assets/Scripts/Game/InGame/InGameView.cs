using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    /// Owns wiring between Unity UI components and view interface events.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Button _scoreButton;
        [SerializeField] private Button _winButton;
        [SerializeField] private Button _loseButton;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _levelText;

        public event Action OnScoreClicked;
        public event Action OnWinClicked;
        public event Action OnLoseClicked;

        private void Awake()
        {
            _scoreButton.onClick.AddListener(() => OnScoreClicked?.Invoke());
            _winButton.onClick.AddListener(() => OnWinClicked?.Invoke());
            _loseButton.onClick.AddListener(() => OnLoseClicked?.Invoke());
        }

        public void UpdateScore(string text)
        {
            _scoreText.text = text;
        }

        public void UpdateLevelLabel(string text)
        {
            _levelText.text = text;
        }
    }
}
