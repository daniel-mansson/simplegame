using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    ///
    /// Layout:
    ///   Top strip  — Level label (centre), Hearts (left), Piece counter (right)
    ///   Board area — 3D jigsaw pieces in world space (managed by InGameSceneController)
    ///   Tray strip — Deck panel at bottom: "Piece X" label + "Place" button
    ///
    /// InGameSceneController calls RegisterPieceCallbacks() after spawning pieces
    /// so that ShowDeckPiece / RevealPiece can move GameObjects.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Tray / Deck")]
        [SerializeField] private GameObject _deckPanel;
        [SerializeField] private Text _deckLabel;
        [SerializeField] private Button _placeButton;

        public event Action<int> OnTapPiece;

        // Piece position delegates — wired by InGameSceneController after spawning
        private Action<int>        _onRevealPiece;     // pieceId → move to board position
        private Action<int>        _onShowDeckPiece;   // pieceId → move to tray highlight slot
        private Action             _onHideDeckPanel;

        private int _currentDeckPieceId = -1;

        private void Awake()
        {
            if (_placeButton != null)
                _placeButton.onClick.AddListener(OnPlaceButtonClicked);
        }

        private void OnDestroy()
        {
            if (_placeButton != null)
                _placeButton.onClick.RemoveListener(OnPlaceButtonClicked);
        }

        /// <summary>
        /// Called by InGameSceneController after piece GameObjects are spawned.
        /// Wires piece-visibility callbacks so the view can move pieces without
        /// knowing about Unity GameObjects.
        /// </summary>
        public void RegisterPieceCallbacks(
            Action<int> onRevealPiece,
            Action<int> onShowDeckPiece,
            Action      onHideDeckPanel)
        {
            _onRevealPiece   = onRevealPiece;
            _onShowDeckPiece = onShowDeckPiece;
            _onHideDeckPanel = onHideDeckPanel;
        }

        private void OnPlaceButtonClicked()
        {
            if (_currentDeckPieceId >= 0)
                OnTapPiece?.Invoke(_currentDeckPieceId);
        }

        /// <summary>Called by PieceTapHandler when a board piece is tapped directly.</summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ──────────────────────────────────────────────────────

        public void ShowDeckPiece(int pieceId)
        {
            _currentDeckPieceId = pieceId;
            if (_deckPanel  != null) _deckPanel.SetActive(true);
            if (_deckLabel  != null) _deckLabel.text = $"Next: Piece {pieceId + 1}";
            _onShowDeckPiece?.Invoke(pieceId);
        }

        public void HideDeckPanel()
        {
            _currentDeckPieceId = -1;
            if (_deckPanel != null) _deckPanel.SetActive(false);
            _onHideDeckPanel?.Invoke();
        }

        public void RevealPiece(int pieceId)
        {
            _onRevealPiece?.Invoke(pieceId);
        }

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
