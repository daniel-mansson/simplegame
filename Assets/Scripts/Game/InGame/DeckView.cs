using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// World-space UI deck bar.  The bar lives as a World Space Canvas that is
    /// repositioned every LateUpdate to exactly cover the bottom portion of the
    /// screen.  A slight X-axis tilt gives it a perspective lean.
    ///
    /// Each slot is a UGUI <see cref="Button"/>.  The piece mesh sits at the same
    /// XY centre in world space but at a closer Z, so it renders on top visually.
    /// Input is captured by the canvas <see cref="GraphicRaycaster"/> before it
    /// reaches the 3D collider, keeping the buttons authoritative.
    ///
    /// Call order each level:
    ///   1. <see cref="Setup"/>          — rebuild buttons for slot count
    ///   2. <see cref="SetSlotActive"/>  — show/hide slots as RefreshSlot fires
    ///   3. <see cref="GetSlotWorldCentre"/> — used by PuzzleStageController to
    ///                                         position piece meshes above buttons
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class DeckView : MonoBehaviour
    {
        // ── Configuration ─────────────────────────────────────────────────────────────

        /// <summary>Fraction of screen height the deck bar occupies.</summary>
        [SerializeField] private float _screenHeightFraction = 0.18f;

        /// <summary>Tilt in degrees around the X axis (positive = top leans away from viewer).</summary>
        [SerializeField] private float _tiltDegrees = 12f;

        /// <summary>
        /// Z distance in front of camera where the canvas plane sits.
        /// Must be between camera near and far clip planes.
        /// </summary>
        [SerializeField] private float _canvasZ = 8f;

        /// <summary>
        /// Z offset of piece meshes relative to the canvas plane (toward camera = negative Z).
        /// Pieces sit just in front so they render on top without blocking input.
        /// </summary>
        public float PieceZOffset => -0.5f;

        // ── State ─────────────────────────────────────────────────────────────────────

        private Canvas    _canvas;
        private Transform _slotContainer;   // HorizontalLayoutGroup parent

        private int       _slotCount;
        private Button[]  _buttons;
        private RectTransform[] _buttonRects;

        // World-space centres of each slot (updated in LateUpdate)
        private Vector3[] _slotWorldCentres;

        // Slot size in world units (updated in LateUpdate)
        private Vector2 _slotWorldSize;

        // Logical contents
        private int?[] _slotContents;

        public event Action<int> OnTapPiece;

        // ── Lifecycle ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode   = RenderMode.WorldSpace;
            _canvas.sortingOrder = 5;
        }

        private void Start()
        {
            // worldCamera must be set on a World Space Canvas so GraphicRaycaster
            // can unproject pointer events into world space. Camera.main is not
            // available during Awake in all scene-load orders, so assign here.
            if (_canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;
        }

        private void LateUpdate()
        {
            RepositionToScreen();
            UpdateSlotWorldCentres();
        }

        // ── Public API ────────────────────────────────────────────────────────────────

        /// <summary>
        /// (Re)creates <paramref name="slotCount"/> buttons.  Safe to call each level start.
        /// </summary>
        public void Setup(int slotCount, int?[] initialContents = null)
        {
            _slotCount    = slotCount;
            _slotContents = new int?[slotCount];
            if (initialContents != null)
                for (int i = 0; i < Mathf.Min(slotCount, initialContents.Length); i++)
                    _slotContents[i] = initialContents[i];

            // Destroy previous buttons
            if (_slotContainer != null)
                for (int i = _slotContainer.childCount - 1; i >= 0; i--)
                    Destroy(_slotContainer.GetChild(i).gameObject);

            // Ensure slot container exists
            if (_slotContainer == null)
            {
                var go = new GameObject("SlotContainer");
                go.transform.SetParent(transform, false);
                _slotContainer = go.transform;

                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin        = Vector2.zero;
                rect.anchorMax        = Vector2.one;
                rect.offsetMin        = Vector2.zero;
                rect.offsetMax        = Vector2.zero;

                var hlg = go.AddComponent<HorizontalLayoutGroup>();
                hlg.childForceExpandWidth  = true;
                hlg.childForceExpandHeight = true;
                hlg.childControlWidth      = true;
                hlg.childControlHeight     = true;
                hlg.childAlignment         = TextAnchor.MiddleCenter;
                hlg.spacing                = 0f;
                hlg.padding                = new RectOffset(4, 4, 4, 4);
            }

            _buttons      = new Button[slotCount];
            _buttonRects  = new RectTransform[slotCount];
            _slotWorldCentres = new Vector3[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                int idx = i;

                var btnGo = new GameObject($"Slot_{i}");
                btnGo.transform.SetParent(_slotContainer, false);

                var rect = btnGo.AddComponent<RectTransform>();
                _buttonRects[i] = rect;

                // Semi-transparent background (diagnostic — easily changed later)
                var img = btnGo.AddComponent<Image>();
                img.color = new Color(0.2f, 0.5f, 1.0f, 0.45f);

                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = img;

                // Button tint colours — keep semi-transparent at all states for diagnostics
                var cols = btn.colors;
                cols.normalColor      = new Color(1f, 1f, 1f, 0.45f);
                cols.highlightedColor = new Color(1f, 1f, 1f, 0.65f);
                cols.pressedColor     = new Color(0.7f, 0.7f, 0.7f, 0.65f);
                cols.selectedColor    = new Color(1f, 1f, 1f, 0.45f);
                cols.disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.20f);
                btn.colors            = cols;

                btn.onClick.AddListener(() =>
                {
                    if (_slotContents == null || idx >= _slotContents.Length) return;
                    if (!_slotContents[idx].HasValue) return;
                    OnTapPiece?.Invoke(_slotContents[idx].Value);
                });

                _buttons[i] = btn;

                // Start hidden
                btnGo.SetActive(false);
            }
        }

        /// <summary>
        /// Show or hide a slot button and record which piece occupies it.
        /// </summary>
        public void SetSlotActive(int slotIndex, int? pieceId)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;

            if (_slotContents == null || slotIndex >= _slotContents.Length)
            {
                var grown = new int?[slotIndex + 1];
                if (_slotContents != null) Array.Copy(_slotContents, grown, _slotContents.Length);
                _slotContents = grown;
            }
            _slotContents[slotIndex] = pieceId;

            if (_buttons != null && slotIndex < _buttons.Length)
                _buttons[slotIndex]?.gameObject.SetActive(pieceId.HasValue);
        }

        /// <summary>
        /// World-space centre of slot <paramref name="index"/>, updated each LateUpdate.
        /// Use this to position the piece mesh on top of the button.
        /// </summary>
        public Vector3 GetSlotWorldCentre(int index)
        {
            if (_slotWorldCentres == null || index >= _slotWorldCentres.Length)
                return Vector3.zero;
            return _slotWorldCentres[index];
        }

        /// <summary>World-space size of one slot button (width, height).</summary>
        public Vector2 GetSlotWorldSize() => _slotWorldSize;

        public int?[] GetSlotContents() => _slotContents;

        // ── Private helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resize and reposition the world-space canvas to cover the bottom
        /// <see cref="_screenHeightFraction"/> of screen, with a perspective tilt.
        /// </summary>
        private void RepositionToScreen()
        {
            var cam = Camera.main;
            if (cam == null) return;

            // Canvas plane distance from camera
            float dist = _canvasZ;

            // Screen-space height of the deck strip in pixels
            float screenH      = Screen.height;
            float screenW      = Screen.width;
            float stripPxH     = screenH * _screenHeightFraction;

            // Convert strip bottom/top screen-y to world space at depth=dist
            // Screen origin is bottom-left; camera looks down -Z
            var bottomScreenPt = new Vector3(screenW * 0.5f, 0f,                          dist);
            var topScreenPt    = new Vector3(screenW * 0.5f, stripPxH,                    dist);
            var leftScreenPt   = new Vector3(0f,             stripPxH * 0.5f,             dist);
            var rightScreenPt  = new Vector3(screenW,        stripPxH * 0.5f,             dist);

            var worldBottom = cam.ScreenToWorldPoint(bottomScreenPt);
            var worldTop    = cam.ScreenToWorldPoint(topScreenPt);
            var worldLeft   = cam.ScreenToWorldPoint(leftScreenPt);
            var worldRight  = cam.ScreenToWorldPoint(rightScreenPt);

            float worldH = Vector3.Distance(worldBottom, worldTop);
            float worldW = Vector3.Distance(worldLeft, worldRight);

            // Centre of the strip in world space
            var worldCentre = cam.ScreenToWorldPoint(
                new Vector3(screenW * 0.5f, stripPxH * 0.5f, dist));

            // Apply position and tilt
            transform.position = worldCentre;
            transform.rotation = cam.transform.rotation * Quaternion.Euler(_tiltDegrees, 0f, 0f);

            // Resize the RectTransform to match the computed world dimensions
            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(worldW, worldH);
        }

        /// <summary>
        /// Compute world-space centres of each slot by transforming their rect centre.
        /// </summary>
        private void UpdateSlotWorldCentres()
        {
            if (_buttonRects == null) return;

            var canvasRT = GetComponent<RectTransform>();
            if (canvasRT == null) return;

            // We need to force the layout to update so button rects are current
            // Canvas.ForceUpdateCanvases only safe outside of layout passes — call once at end of frame is fine
            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < _buttonRects.Length; i++)
            {
                var br = _buttonRects[i];
                if (br == null) { _slotWorldCentres[i] = Vector3.zero; continue; }

                // RectTransform.TransformPoint gives the world position of a local rect point
                var localCentre = (Vector3)br.rect.center + br.localPosition;
                // Walk up to canvas space
                _slotWorldCentres[i] = br.TransformPoint(br.rect.center);

                // Slot world size (use first slot, all equal with ForceExpand)
                if (i == 0)
                    _slotWorldSize = new Vector2(
                        br.rect.width  * br.lossyScale.x,
                        br.rect.height * br.lossyScale.y);
            }
        }
    }
}
