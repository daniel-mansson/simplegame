using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    ///
    /// Layout:
    ///   HUD      — Level label (centre), Hearts (left), Piece counter (right) — top strip (Screen Space Overlay)
    ///   Board    — 3D jigsaw pieces in world space (managed by PuzzleStageController)
    ///   DeckView — World Space Canvas at the bottom; one Button per active slot.
    ///              Piece meshes float on top of buttons (same XY, closer Z).
    ///
    /// PuzzleStageController calls:
    ///   <see cref="RegisterPieceCallbacks"/> — wire 3D-piece animation callbacks
    ///   <see cref="SetupDeckPanel"/>         — rebuild buttons for new slot count
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Deck View (World Space)")]
        [SerializeField] private DeckView _deckView;

        public event Action<int> OnTapPiece;

        // Callbacks wired by PuzzleStageController after SpawnLevel
        private Action<int, int> _onMovePieceToSlot;
        private Action<int>      _onRevealPiece;
        private Action<int>      _onShakePiece;

        // ── Registration ──────────────────────────────────────────────────────────────

        public void RegisterPieceCallbacks(
            Action<int, int> onMovePieceToSlot,
            Action<int>      onRevealPiece,
            Action<int>      onShakePiece = null,
            Action           onHideTray   = null)
        {
            _onMovePieceToSlot = onMovePieceToSlot;
            _onRevealPiece     = onRevealPiece;
            _onShakePiece      = onShakePiece;
        }

        // ── Setup ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Rebuild slot buttons for a new level. Called from PuzzleStageController.SpawnLevel.
        /// </summary>
        public void SetupDeckPanel(int slotCount)
        {
            if (_deckView == null) return;

            _deckView.Setup(slotCount);

            // Forward tap events
            _deckView.OnTapPiece -= HandleTapPiece;
            _deckView.OnTapPiece += HandleTapPiece;
        }

        private void HandleTapPiece(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ───────────────────────────────────────────────────────────────

        public void RefreshSlot(int slotIndex, int? pieceId)
        {
            _deckView?.SetSlotActive(slotIndex, pieceId);

            if (pieceId.HasValue)
                _onMovePieceToSlot?.Invoke(pieceId.Value, slotIndex);
        }

        public void RevealPiece(int pieceId)
        {
            // Find and clear the slot
            var contents = _deckView?.GetSlotContents();
            if (contents != null)
                for (int i = 0; i < contents.Length; i++)
                    if (contents[i] == pieceId)
                        _deckView.SetSlotActive(i, null);

            _onRevealPiece?.Invoke(pieceId);
        }

        public void UpdateHearts(string text)       { if (_heartsText       != null) _heartsText.text       = text; }
        public void UpdatePieceCounter(string text) { if (_pieceCounterText != null) _pieceCounterText.text = text; }
        public void UpdateLevelLabel(string text)   { if (_levelText        != null) _levelText.text        = text; }

        public int?[] GetSlotContents() => _deckView?.GetSlotContents();

        public void ShakePiece(int slotIndex)
        {
            var contents = _deckView?.GetSlotContents();
            if (contents == null || slotIndex >= contents.Length) return;
            if (!contents[slotIndex].HasValue) return;
            _onShakePiece?.Invoke(slotIndex);
        }

        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);
    }
}
