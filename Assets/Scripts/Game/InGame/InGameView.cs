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
    ///   HUD  — Level label (centre), Hearts (left), Piece counter (right) — top strip
    ///   Board — 3D jigsaw pieces in world space (managed by InGameSceneController)
    ///   Tray  — Bottom strip: up to 3 visible deck pieces as 3D meshes, no button
    ///
    /// InGameSceneController calls RegisterPieceCallbacks() after spawning so this view
    /// can reposition GameObjects without holding a reference to them itself.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Tray — serialized for scene wiring, not used at runtime")]
        [SerializeField] private GameObject _deckPanel;   // kept for SceneSetup wiring; not toggled
        [SerializeField] private Text       _deckLabel;   // optional status label
        [SerializeField] private Button     _placeButton; // disabled at runtime

        public event Action<int> OnTapPiece;

        // Delegates wired by InGameSceneController after SpawnPieces
        private Action<int, int> _onMovePieceToSlot;   // (pieceId, slotIndex 0-2)
        private Action<int>      _onRevealPiece;        // pieceId → board position
        private Action           _onHideTray;

        // Current tray window — up to 3 piece IDs (null = empty slot)
        private int?[] _trayWindow = new int?[3];

        // ── MonoBehaviour ─────────────────────────────────────────────────

        private void Awake()
        {
            // Deck panel and place button not used — 3D tray pieces handle all interaction
            if (_deckPanel   != null) _deckPanel.SetActive(false);
            if (_placeButton != null) _placeButton.gameObject.SetActive(false);
        }

        // ── Registration ──────────────────────────────────────────────────

        /// <summary>
        /// Called by InGameSceneController after piece GameObjects are spawned.
        /// <paramref name="onMovePieceToSlot"/> (pieceId, slotIdx) repositions the piece
        /// into tray slot 0, 1, or 2.
        /// <paramref name="onRevealPiece"/> moves the piece to its solved board position.
        /// </summary>
        public void RegisterPieceCallbacks(
            Action<int, int> onMovePieceToSlot,
            Action<int>      onRevealPiece,
            Action           onHideTray = null)
        {
            _onMovePieceToSlot = onMovePieceToSlot;
            _onRevealPiece     = onRevealPiece;
            _onHideTray        = onHideTray;
        }

        /// <summary>Called by PieceTapHandler when a board or tray piece is tapped.</summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ───────────────────────────────────────────────────

        public void RefreshTray(int?[] pieceIds)
        {
            if (pieceIds == null || pieceIds.Length == 0)
            {
                _onHideTray?.Invoke();
                if (_deckLabel != null) _deckLabel.text = "";
                _trayWindow = new int?[3];
                return;
            }

            // Move pieces that have changed slot positions
            for (int slot = 0; slot < 3; slot++)
            {
                var newId = slot < pieceIds.Length ? pieceIds[slot] : null;
                var oldId = _trayWindow[slot];

                if (newId == oldId) continue;

                // A piece just left this slot — nothing to do here (caller handles reveal)
                if (newId.HasValue)
                    _onMovePieceToSlot?.Invoke(newId.Value, slot);

                _trayWindow[slot] = newId;
            }

            // Update status label with front piece
            if (_deckLabel != null)
            {
                var front = pieceIds.Length > 0 ? pieceIds[0] : null;
                _deckLabel.text = front.HasValue ? $"Piece {front.Value + 1}" : "";
            }
        }

        public void RevealPiece(int pieceId)
        {
            // Clear it from tray window tracking
            for (int i = 0; i < 3; i++)
                if (_trayWindow[i] == pieceId) _trayWindow[i] = null;

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
