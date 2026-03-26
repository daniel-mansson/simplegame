using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IInGameView.
    ///
    /// Layout:
    ///   HUD       — Level label (centre), Hearts (left), Piece counter (right) — top strip
    ///   Board     — 3D jigsaw pieces in world space (managed by PuzzleStageController)
    ///   DeckPanel — Transparent container; holds invisible UGUI hit-target buttons that
    ///               are repositioned each frame (via <see cref="UpdateDeckButtonScreenPositions"/>)
    ///               to exactly overlay the 3D tray-slot pieces in world space.
    ///               The 3D pieces ARE the visual; the UGUI buttons are the tap surface.
    ///
    /// PuzzleStageController calls:
    ///   <see cref="RegisterPieceCallbacks"/> — after spawning, to wire 3D piece repositioning
    ///   <see cref="SetupDeckPanel"/>         — to create N invisible hit-target buttons
    ///   <see cref="UpdateDeckButtonScreenPositions"/> — every LateUpdate to track piece positions
    /// </summary>
    public class InGameView : MonoBehaviour, IInGameView
    {
        [SerializeField] private Text _heartsText;
        [SerializeField] private Text _pieceCounterText;
        [SerializeField] private Text _levelText;

        [Header("Deck Panel")]
        [SerializeField] private GameObject    _deckPanel;            // transparent container (no visual)
        [SerializeField] private RectTransform _pieceButtonContainer; // plain RectTransform parent for buttons

        public event Action<int> OnTapPiece;

        // Delegates wired by PuzzleStageController after SpawnLevel
        private Action<int, int> _onMovePieceToSlot;   // (pieceId, slotIndex)
        private Action<int>      _onRevealPiece;        // pieceId → board position
        private Action<int>      _onShakePiece;         // slotIndex → shake the piece in that slot

        // Per-slot tracking: slotIndex → current piece ID (null = empty)
        private int?[] _slotContents;

        // Runtime hit-target buttons — one per slot, positioned in LateUpdate to overlay 3D pieces
        private Button[]        _deckButtons;
        private RectTransform[] _deckButtonRects;

        // ── Registration ──────────────────────────────────────────────────

        /// <summary>
        /// Called by PuzzleStageController after piece GameObjects are spawned.
        /// </summary>
        public void RegisterPieceCallbacks(
            Action<int, int> onMovePieceToSlot,
            Action<int>      onRevealPiece,
            Action<int>      onShakePiece = null,
            Action           onHideTray   = null)   // kept for scene compatibility
        {
            _onMovePieceToSlot = onMovePieceToSlot;
            _onRevealPiece     = onRevealPiece;
            _onShakePiece      = onShakePiece;
        }

        /// <summary>
        /// Creates <paramref name="slotCount"/> invisible UGUI hit-target buttons inside
        /// <see cref="_pieceButtonContainer"/>. Buttons are transparent — the 3D pieces
        /// are the visual. Positions are updated each frame by
        /// <see cref="UpdateDeckButtonScreenPositions"/>.
        /// </summary>
        public void SetupDeckPanel(int slotCount)
        {
            // Destroy buttons from previous level
            if (_pieceButtonContainer != null)
            {
                for (int i = _pieceButtonContainer.childCount - 1; i >= 0; i--)
                    Destroy(_pieceButtonContainer.GetChild(i).gameObject);
            }

            _deckButtons      = new Button[slotCount];
            _deckButtonRects  = new RectTransform[slotCount];

            if (_pieceButtonContainer == null) return;

            for (int i = 0; i < slotCount; i++)
            {
                int slotIdx = i;

                var btnGo = new GameObject($"PieceButton_{i}");
                btnGo.transform.SetParent(_pieceButtonContainer, false);

                // Anchor to bottom-left; position set each frame from world-space piece coords
                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin        = Vector2.zero;
                rt.anchorMax        = Vector2.zero;
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta        = new Vector2(100f, 100f);

                // Transparent Image — required for Button raycast but visually invisible
                var img = btnGo.AddComponent<Image>();
                img.color = Color.clear;

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.transition    = Selectable.Transition.None;

                btn.onClick.AddListener(() =>
                {
                    if (_slotContents == null || slotIdx >= _slotContents.Length) return;
                    if (!_slotContents[slotIdx].HasValue) return;
                    OnTapPiece?.Invoke(_slotContents[slotIdx].Value);
                });

                _deckButtons[i]     = btn;
                _deckButtonRects[i] = rt;

                // Start disabled — RefreshSlot enables when a piece occupies the slot
                btnGo.SetActive(false);
            }
        }

        /// <summary>
        /// Repositions deck hit-target buttons to overlay the 3D tray-slot pieces.
        /// Call from PuzzleStageController.LateUpdate after updating _traySlotPositions.
        /// </summary>
        /// <param name="cam">The scene camera (used for WorldToScreenPoint).</param>
        /// <param name="slotWorldPositions">World-space centre of each tray slot.</param>
        /// <param name="slotWorldW">World-space width of one slot (for button sizing).</param>
        /// <param name="slotWorldH">World-space height of one slot (for button sizing).</param>
        /// <param name="canvasScaleFactor">Canvas.scaleFactor of the parent canvas.</param>
        public void UpdateDeckButtonScreenPositions(
            Camera cam,
            Vector3[] slotWorldPositions,
            float slotWorldW,
            float slotWorldH,
            float canvasScaleFactor)
        {
            if (_deckButtonRects == null || cam == null) return;
            if (canvasScaleFactor < 1e-4f) canvasScaleFactor = 1f;

            int count = Mathf.Min(_deckButtonRects.Length, slotWorldPositions.Length);
            for (int i = 0; i < count; i++)
            {
                var rt = _deckButtonRects[i];
                if (rt == null) continue;

                Vector3 screenPos  = cam.WorldToScreenPoint(slotWorldPositions[i]);
                rt.anchoredPosition = new Vector2(screenPos.x / canvasScaleFactor,
                                                  screenPos.y / canvasScaleFactor);

                // Size button to match the world-space piece footprint
                Vector3 rightEdge = cam.WorldToScreenPoint(slotWorldPositions[i] + Vector3.right * slotWorldW * 0.5f);
                Vector3 leftEdge  = cam.WorldToScreenPoint(slotWorldPositions[i] - Vector3.right * slotWorldW * 0.5f);
                Vector3 topEdge   = cam.WorldToScreenPoint(slotWorldPositions[i] + Vector3.up    * slotWorldH * 0.5f);
                Vector3 botEdge   = cam.WorldToScreenPoint(slotWorldPositions[i] - Vector3.up    * slotWorldH * 0.5f);

                float pxW = Mathf.Abs(rightEdge.x - leftEdge.x);
                float pxH = Mathf.Abs(topEdge.y   - botEdge.y);
                rt.sizeDelta = new Vector2(pxW / canvasScaleFactor, pxH / canvasScaleFactor);
            }
        }

        /// <summary>Called by PieceTapHandler when a 3D tray piece is tapped (legacy path).</summary>
        public void NotifyPieceTapped(int pieceId) => OnTapPiece?.Invoke(pieceId);

        // ── IInGameView ───────────────────────────────────────────────────

        /// <summary>
        /// Update a single tray slot. If <paramref name="pieceId"/> is null the slot becomes
        /// empty (hit-target hidden). Otherwise the deck button is enabled and the 3D piece
        /// is repositioned to that slot via the registered callback.
        /// </summary>
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

            // Show/hide the hit-target button for this slot
            if (_deckButtons != null && slotIndex < _deckButtons.Length)
            {
                var btn = _deckButtons[slotIndex];
                if (btn != null)
                    btn.gameObject.SetActive(pieceId.HasValue);
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
            if (_slotContents == null || slotIndex >= _slotContents.Length) return;
            if (!_slotContents[slotIndex].HasValue) return;
            _onShakePiece?.Invoke(slotIndex);
        }
    }
}
