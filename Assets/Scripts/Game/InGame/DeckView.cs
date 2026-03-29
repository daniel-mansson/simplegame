using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// World-space deck bar.  The scene provides the Canvas, HorizontalLayoutGroup,
    /// and a fixed pool of slot Buttons wired via <see cref="_slots"/>.
    ///
    /// Call <see cref="Init"/> once per level with the active slot count — buttons
    /// beyond that count are deactivated.  Call <see cref="SetSlot"/> as the model
    /// pushes piece assignments.  <see cref="OnPiecePressed"/> fires the piece ID
    /// whenever a slot button is tapped.
    ///
    /// For 3D piece positioning: <see cref="GetSlotButton"/> returns the Button so
    /// the caller can read its RectTransform world position each frame.
    /// </summary>
    public class DeckView : MonoBehaviour
    {
        // ── Inspector wiring ──────────────────────────────────────────────────────────

        /// <summary>
        /// Pre-placed slot buttons inside the HorizontalLayoutGroup.
        /// Wire as many as the maximum slot count the game will ever use.
        /// </summary>
        [SerializeField] private Button[] _slots;

        // ── Events ────────────────────────────────────────────────────────────────────

        /// <summary>Fired when a slot button is tapped. Argument is the piece ID.</summary>
        public event Action<int> OnPiecePressed;

        // ── State ─────────────────────────────────────────────────────────────────────

        private int    _activeSlots;
        private int?[] _slotContents;   // slotIndex → pieceId (null = empty)

        // ── Lifecycle ─────────────────────────────────────────────────────────────────

        private void Start()
        {
            // World Space Canvas needs worldCamera set so GraphicRaycaster can
            // unproject pointer events. Assign here in case SceneSetup didn't serialise it.
            var canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;
        }

        // ── Public API ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Activate the first <paramref name="slotCount"/> buttons and deactivate the rest.
        /// Wires onClick listeners. Safe to call again each level start.
        /// </summary>
        public void Init(int slotCount)
        {
            _activeSlots  = slotCount;
            _slotContents = new int?[slotCount];

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) continue;

                bool active = i < slotCount;
                _slots[i].gameObject.SetActive(active);

                if (!active) continue;

                // Capture for closure
                int idx = i;
                _slots[i].onClick.RemoveAllListeners();
                _slots[i].onClick.AddListener(() => HandleSlotTapped(idx));
            }
        }

        /// <summary>
        /// Assign or clear a piece from a slot.
        /// Passing null marks the slot empty (button stays visible but won't fire).
        /// </summary>
        public void SetSlot(int slotIndex, int? pieceId)
        {
            if (slotIndex < 0 || slotIndex >= _activeSlots) return;
            if (_slotContents != null && slotIndex < _slotContents.Length)
                _slotContents[slotIndex] = pieceId;
        }

        /// <summary>
        /// Returns the Button for slot <paramref name="index"/>, or null if out of range.
        /// Use the Button's RectTransform to position 3D pieces above it each frame.
        /// </summary>
        public Button GetSlotButton(int index)
        {
            if (_slots == null || index < 0 || index >= _slots.Length) return null;
            return _slots[index];
        }

        /// <summary>Current piece IDs per slot (read-only snapshot).</summary>
        public int?[] GetSlotContents() => _slotContents;

        // ── Private ───────────────────────────────────────────────────────────────────

        private void HandleSlotTapped(int slotIndex)
        {
            if (_slotContents == null || slotIndex >= _slotContents.Length) return;
            var pieceId = _slotContents[slotIndex];
            if (!pieceId.HasValue) return;
            OnPiecePressed?.Invoke(pieceId.Value);
        }
    }
}
