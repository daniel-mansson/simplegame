using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    /// Owns wiring between Unity UI components and view interface events.
    /// Has zero references to presenters, services, or managers.
    ///
    /// OnTapPiece is fired by PieceTapHandler components on each puzzle piece GameObject
    /// (wired in S04). Until then it can be fired programmatically for testing.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        /// <summary>
        /// Fired by PieceTapHandler on each piece GameObject when the player taps a piece.
        /// Carries the piece ID. Wired externally in S04 — no direct UI button for this.
        /// </summary>
        public event Action<int> OnTapPiece;

        /// <summary>
        /// Called by PieceTapHandler (or test code) to notify the presenter of a tap.
        /// </summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        public void UpdateHearts(string text)
        {
            if (_heartsText != null) _heartsText.text = text;
        }

        public void UpdatePieceCounter(string text)
        {
            if (_pieceCounterText != null) _pieceCounterText.text = text;
        }

        public void UpdateLevelLabel(string text)
        {
            if (_levelText != null) _levelText.text = text;
        }
    }
}
