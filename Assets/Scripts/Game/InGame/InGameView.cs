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
        [SerializeField] private Button _placeCorrectButton;
        [SerializeField] private Button _placeIncorrectButton;
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        public event Action OnPlaceCorrect;
        public event Action OnPlaceIncorrect;

        private void Awake()
        {
            _placeCorrectButton.onClick.AddListener(() => OnPlaceCorrect?.Invoke());
            _placeIncorrectButton.onClick.AddListener(() => OnPlaceIncorrect?.Invoke());
        }

        public void UpdateHearts(string text)
        {
            _heartsText.text = text;
        }

        public void UpdatePieceCounter(string text)
        {
            _pieceCounterText.text = text;
        }

        public void UpdateLevelLabel(string text)
        {
            _levelText.text = text;
        }
    }
}
