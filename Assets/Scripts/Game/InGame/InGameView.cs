using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    ///
    /// Layout:
    ///   HUD        — Level label (centre), Hearts (left), Piece counter (right) — top strip
    ///   Board      — 3D jigsaw pieces in world space (managed by PuzzleStageController)
    ///   DeckPanel  — Bottom strip: UGUI HorizontalLayoutGroup with one Button per active
    ///                tray slot. Buttons are created by <see cref="SetupDeckPanel"/> and
    ///                kept in sync by <see cref="RefreshSlot"/>.
    ///
    /// PuzzleStageController calls <see cref="RegisterPieceCallbacks"/> after spawning so
    /// this view can forward 3D-piece reposition requests without owning the GameObjects.
    /// It also calls <see cref="SetupDeckPanel"/> to populate the deck button row.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Deck Panel")]
        [SerializeField] private GameObject    _deckPanel;            // bottom strip container
        [SerializeField] private RectTransform _pieceButtonContainer; // HorizontalLayoutGroup child

        public event Action<int> OnTapPiece;

        // Delegates wired by PuzzleStageController after SpawnLevel
        private Action<int, int> _onMovePieceToSlot;   // (pieceId, slotIndex)
        private Action<int>      _onRevealPiece;        // pieceId → board position
        private Action<int>      _onShakePiece;         // slotIndex → shake the piece in that slot

        // Per-slot tracking: slotIndex → current piece ID (null = empty)
        private int?[] _slotContents;

        // Runtime deck buttons — one per slot, created by SetupDeckPanel
        private Button[] _deckButtons;

        // ── Registration ──────────────────────────────────────────────────

        /// <summary>
        /// Called by PuzzleStageController after piece GameObjects are spawned.
        /// <paramref name="onMovePieceToSlot"/> (pieceId, slotIdx) repositions the 3D piece.
        /// <paramref name="onRevealPiece"/> moves the piece to its solved board position.
        /// </summary>
        public void RegisterPieceCallbacks(
            Action<int, int> onMovePieceToSlot,
            Action<int>      onRevealPiece,
            Action<int>      onShakePiece = null,
            Action           onHideTray   = null)   // onHideTray kept for scene compatibility
        {
            _onMovePieceToSlot = onMovePieceToSlot;
            _onRevealPiece     = onRevealPiece;
            _onShakePiece      = onShakePiece;
        }

        /// <summary>
        /// Creates <paramref name="slotCount"/> UGUI buttons inside
        /// <see cref="_pieceButtonContainer"/>. Call once per level start (or retry)
        /// from <see cref="PuzzleStageController.SpawnLevel"/>.
        /// </summary>
        public void SetupDeckPanel(int slotCount)
        {
            // Destroy buttons from previous level
            if (_pieceButtonContainer != null)
            {
                for (int i = _pieceButtonContainer.childCount - 1; i >= 0; i--)
                    Destroy(_pieceButtonContainer.GetChild(i).gameObject);
            }

            _deckButtons = new Button[slotCount];

            if (_pieceButtonContainer == null) return;

            for (int i = 0; i < slotCount; i++)
            {
                int slotIdx = i; // capture for closure

                // Button root
                var btnGo = new GameObject($"PieceButton_{i}");
                btnGo.transform.SetParent(_pieceButtonContainer, false);

                var le = btnGo.AddComponent<LayoutElement>();
                le.preferredWidth  = 120f;
                le.preferredHeight = 80f;
                le.flexibleWidth   = 1f;

                var img = btnGo.AddComponent<Image>();
                img.color = new Color(0.25f, 0.45f, 0.85f, 1f);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;

                btn.onClick.AddListener(() =>
                {
                    if (_slotContents == null || slotIdx >= _slotContents.Length) return;
                    if (!_slotContents[slotIdx].HasValue) return;
                    OnTapPiece?.Invoke(_slotContents[slotIdx].Value);
                });

                // Label text
                var textGo = new GameObject("Label");
                textGo.transform.SetParent(btnGo.transform, false);

                var textRect = textGo.AddComponent<RectTransform>();
                textRect.anchorMin        = Vector2.zero;
                textRect.anchorMax        = Vector2.one;
                textRect.sizeDelta        = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;

                var text = textGo.AddComponent<Text>();
                text.text           = "";
                text.alignment      = TextAnchor.MiddleCenter;
                text.fontSize       = 20;
                text.color          = Color.white;
                text.raycastTarget  = false;

                _deckButtons[i] = btn;

                // Start hidden — RefreshSlot will show it when a piece is assigned
                btnGo.SetActive(false);
            }
        }

        /// <summary>Called by PieceTapHandler when a 3D tray piece is tapped (legacy path).</summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ───────────────────────────────────────────────────

        /// <summary>
        /// Update a single tray slot. If <paramref name="pieceId"/> is null the slot becomes
        /// visually empty (button hidden). Otherwise the piece is repositioned to that slot
        /// and the deck button is shown and labelled.
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

            // Update deck button for this slot
            if (_deckButtons != null && slotIndex < _deckButtons.Length)
            {
                var btn = _deckButtons[slotIndex];
                if (btn != null)
                {
                    btn.gameObject.SetActive(pieceId.HasValue);
                    var label = btn.GetComponentInChildren<Text>();
                    if (label != null)
                        label.text = pieceId.HasValue ? $"Piece {pieceId.Value + 1}" : "";
                }
            }

            if (pieceId == oldId) return;

            if (pieceId.HasValue)
                _onMovePieceToSlot?.Invoke(pieceId.Value, slotIndex);
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
