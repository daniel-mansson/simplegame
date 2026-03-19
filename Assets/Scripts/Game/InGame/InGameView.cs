using System;
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
    ///   Tray  — Bottom strip: slot-indexed 3D meshes; no button
    ///
    /// InGameSceneController calls RegisterPieceCallbacks() after spawning so this view
    /// can reposition GameObjects without holding a reference to them itself.
    ///
    /// <para>The presenter now calls <see cref="RefreshSlot"/> for targeted slot updates
    /// instead of sending the full tray window each time.</para>
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
        private Action<int, int> _onMovePieceToSlot;   // (pieceId, slotIndex)
        private Action<int>      _onRevealPiece;        // pieceId → board position
        private Action<int>      _onShakePiece;         // slotIndex → shake the piece in that slot

        // Per-slot tracking: slotIndex → current piece ID (null = empty)
        private int?[] _slotContents;

        // ── MonoBehaviour ─────────────────────────────────────────────────

        private void Awake()
        {
            if (_deckPanel   != null) _deckPanel.SetActive(false);
            if (_placeButton != null) _placeButton.gameObject.SetActive(false);
        }

        // ── Registration ──────────────────────────────────────────────────

        /// <summary>
        /// Called by InGameSceneController after piece GameObjects are spawned.
        /// <paramref name="onMovePieceToSlot"/> (pieceId, slotIdx) repositions the piece.
        /// <paramref name="onRevealPiece"/> moves the piece to its solved board position.
        /// </summary>
        public void RegisterPieceCallbacks(
            Action<int, int> onMovePieceToSlot,
            Action<int>      onRevealPiece,
            Action<int>      onShakePiece = null,
            Action           onHideTray = null)    // onHideTray kept for scene compatibility
        {
            _onMovePieceToSlot = onMovePieceToSlot;
            _onRevealPiece     = onRevealPiece;
            _onShakePiece      = onShakePiece;
        }

        /// <summary>Called by PieceTapHandler when a tray piece is tapped.</summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ───────────────────────────────────────────────────

        /// <summary>
        /// Update a single tray slot. If <paramref name="pieceId"/> is null the slot
        /// becomes visually empty. Otherwise the piece is repositioned to that slot.
        /// </summary>
        public void RefreshSlot(int slotIndex, int? pieceId)
        {
            // Grow tracking array on demand (slot count varies with config)
            if (_slotContents == null || slotIndex >= _slotContents.Length)
            {
                var grown = new int?[slotIndex + 1];
                if (_slotContents != null)
                    System.Array.Copy(_slotContents, grown, _slotContents.Length);
                _slotContents = grown;
            }

            var oldId = _slotContents[slotIndex];
            _slotContents[slotIndex] = pieceId;

            if (pieceId == oldId) return;

            if (pieceId.HasValue)
                _onMovePieceToSlot?.Invoke(pieceId.Value, slotIndex);

            // Update status label when slot 0 changes (slot 0 = front/most prominent)
            if (slotIndex == 0 && _deckLabel != null)
                _deckLabel.text = pieceId.HasValue ? $"Piece {pieceId.Value + 1}" : "";
        }

        public void RevealPiece(int pieceId)
        {
            // Clear from slot tracking
            if (_slotContents != null)
                for (int i = 0; i < _slotContents.Length; i++)
                    if (_slotContents[i] == pieceId) _slotContents[i] = null;

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

        public int?[] GetSlotContents() => _slotContents;

        public void ShakePiece(int slotIndex)
        {
            // Resolve which piece ID is currently in this slot, then forward to the controller
            if (_slotContents == null || slotIndex >= _slotContents.Length) return;
            if (!_slotContents[slotIndex].HasValue) return;
            _onShakePiece?.Invoke(slotIndex);
        }
    }
}
