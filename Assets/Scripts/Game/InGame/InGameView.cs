using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    ///
    /// Deck panel: one UGUI Button per active tray slot.  Each button has a
    /// <see cref="RawImage"/> that displays a <see cref="RenderTexture"/> produced by
    /// <see cref="DeckPreviewManager"/>.  The 3D piece is rendered into that texture by
    /// a dedicated off-screen orthographic camera; the piece is invisible to the main
    /// camera while in the deck.
    ///
    /// Tap flow: Button.onClick → OnTapPiece(pieceId) → presenter → model.
    /// Piece flow: RefreshSlot calls SetSlotPiece/ClearSlot on the preview manager.
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Deck Panel")]
        [SerializeField] private GameObject    _deckPanel;
        [SerializeField] private RectTransform _pieceButtonContainer;

        public event Action<int> OnTapPiece;

        // Callbacks wired by PuzzleStageController
        private Action<int, int> _onMovePieceToSlot;  // (pieceId, slotIndex)
        private Action<int>      _onRevealPiece;
        private Action<int>      _onShakePiece;

        // Per-slot tracking
        private int?[] _slotContents;

        // Button array — one per slot
        private Button[]   _deckButtons;
        private RawImage[] _deckPreviews;  // RawImage inside each button

        // Preview manager — renders piece mesh into RenderTextures
        private DeckPreviewManager _previewManager;

        // Piece GameObject lookup — set once by PuzzleStageController after SpawnLevel
        private Func<int, GameObject> _getPieceGo;

        // Hidden world position — pieces go here when NOT in preview (only board pieces visible)
        private Vector3 _hiddenPos = new Vector3(-9999f, -9999f, -2f);

        // ── Registration ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Provides a lookup so InGameView can retrieve piece GameObjects by ID when
        /// assigning them to preview cameras. Call once after piece GameObjects are spawned.
        /// </summary>
        public void SetPieceGoLookup(Func<int, GameObject> lookup) => _getPieceGo = lookup;

        /// <summary>
        /// Provides the world-space hidden position used when pulling a piece out of preview
        /// (e.g. when a slot is cleared before RevealPiece moves it to the board).
        /// </summary>
        public void SetHiddenPos(Vector3 pos) => _hiddenPos = pos;

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
        /// Creates <paramref name="slotCount"/> UGUI buttons with RenderTexture previews.
        /// Called once per level from PuzzleStageController.SpawnLevel.
        /// </summary>
        public void SetupDeckPanel(int slotCount)
        {
            // Destroy buttons from previous level
            if (_pieceButtonContainer != null)
                for (int i = _pieceButtonContainer.childCount - 1; i >= 0; i--)
                    Destroy(_pieceButtonContainer.GetChild(i).gameObject);

            // Ensure preview manager exists on same GameObject
            if (_previewManager == null)
                _previewManager = gameObject.AddComponent<DeckPreviewManager>();
            _previewManager.Setup(slotCount);

            _deckButtons  = new Button[slotCount];
            _deckPreviews = new RawImage[slotCount];

            if (_pieceButtonContainer == null) return;

            for (int i = 0; i < slotCount; i++)
            {
                int slotIdx = i;

                // ── Button root ───────────────────────────────────────────────────
                var btnGo = new GameObject($"PieceButton_{i}");
                btnGo.transform.SetParent(_pieceButtonContainer, false);

                var le = btnGo.AddComponent<LayoutElement>();
                le.preferredWidth  = 120f;
                le.preferredHeight = 100f;
                le.flexibleWidth   = 1f;

                // Background image (button visual / hit target)
                var bg = btnGo.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.20f, 0.90f);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = bg;

                btn.onClick.AddListener(() =>
                {
                    if (_slotContents == null || slotIdx >= _slotContents.Length) return;
                    if (!_slotContents[slotIdx].HasValue) return;
                    OnTapPiece?.Invoke(_slotContents[slotIdx].Value);
                });

                // ── RawImage for piece preview ────────────────────────────────────
                var rawGo   = new GameObject("Preview");
                rawGo.transform.SetParent(btnGo.transform, false);

                var rawRect = rawGo.AddComponent<RectTransform>();
                rawRect.anchorMin        = new Vector2(0.05f, 0.05f);
                rawRect.anchorMax        = new Vector2(0.95f, 0.95f);
                rawRect.sizeDelta        = Vector2.zero;
                rawRect.anchoredPosition = Vector2.zero;

                var raw = rawGo.AddComponent<RawImage>();
                raw.texture        = _previewManager.GetTexture(i);
                raw.raycastTarget  = false;

                _deckButtons[i]  = btn;
                _deckPreviews[i] = raw;

                // Start hidden — RefreshSlot activates when a piece occupies the slot
                btnGo.SetActive(false);
            }
        }

        // ── IInGameView ───────────────────────────────────────────────────────────────

        public void RefreshSlot(int slotIndex, int? pieceId)
        {
            // Grow tracking array on demand
            if (_slotContents == null || slotIndex >= _slotContents.Length)
            {
                var grown = new int?[slotIndex + 1];
                if (_slotContents != null)
                    System.Array.Copy(_slotContents, grown, _slotContents.Length);
                _slotContents = grown;
            }

            var oldId = _slotContents[slotIndex];
            _slotContents[slotIndex] = pieceId;

            // ── Update preview camera ─────────────────────────────────────────────
            if (_previewManager != null && slotIndex < DeckPreviewManager.MaxSlots)
            {
                if (pieceId.HasValue)
                {
                    var go = _getPieceGo?.Invoke(pieceId.Value);
                    _previewManager.SetSlotPiece(slotIndex, go);
                }
                else
                {
                    _previewManager.ClearSlot(slotIndex, _hiddenPos);
                }
            }

            // ── Update button visibility ──────────────────────────────────────────
            if (_deckButtons != null && slotIndex < _deckButtons.Length)
            {
                var btn = _deckButtons[slotIndex];
                if (btn != null)
                    btn.gameObject.SetActive(pieceId.HasValue);

                // Update RawImage texture in case manager recreated textures
                if (_deckPreviews != null && slotIndex < _deckPreviews.Length)
                {
                    var ri = _deckPreviews[slotIndex];
                    if (ri != null)
                        ri.texture = _previewManager?.GetTexture(slotIndex);
                }
            }

            if (pieceId == oldId) return;

            // Notify stage controller (e.g. for animation hooks or bookkeeping)
            if (pieceId.HasValue)
                _onMovePieceToSlot?.Invoke(pieceId.Value, slotIndex);
        }

        public void RevealPiece(int pieceId)
        {
            // Find which slot this piece was in and clear it
            if (_slotContents != null)
            {
                for (int i = 0; i < _slotContents.Length; i++)
                {
                    if (_slotContents[i] == pieceId)
                    {
                        _slotContents[i] = null;
                        // Clear preview — piece will be moved to board by RevealPiece callback
                        // Don't call ClearSlot (which hides the piece) — RevealPiece moves it
                        if (_previewManager != null && i < DeckPreviewManager.MaxSlots)
                        {
                            // Reset layer only; position is handled by RevealPiece tween
                            _previewManager.ClearSlot(i, _hiddenPos);
                        }
                        if (_deckButtons != null && i < _deckButtons.Length)
                            _deckButtons[i]?.gameObject.SetActive(false);
                        break;
                    }
                }
            }

            _onRevealPiece?.Invoke(pieceId);
        }

        public void UpdateHearts(string text)       { if (_heartsText       != null) _heartsText.text       = text; }
        public void UpdatePieceCounter(string text) { if (_pieceCounterText != null) _pieceCounterText.text = text; }
        public void UpdateLevelLabel(string text)   { if (_levelText        != null) _levelText.text        = text; }

        public int?[] GetSlotContents() => _slotContents;

        public void ShakePiece(int slotIndex)
        {
            // Visual shake on the deck button — future polish task.
            // Audio/haptic feedback can be wired here without the 3D piece.
            if (_slotContents == null || slotIndex >= _slotContents.Length) return;
            if (!_slotContents[slotIndex].HasValue) return;
            _onShakePiece?.Invoke(slotIndex);
        }

        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);
    }
}
