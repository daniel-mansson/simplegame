using System;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Manages per-slot orthographic preview cameras and RenderTextures so that each
    /// deck-panel button can display the actual jigsaw piece mesh it represents.
    ///
    /// One camera + one RenderTexture per slot. Each camera renders only its assigned
    /// layer (DeckPreview0–DeckPreview4, Unity layers 6–10). The piece GameObject is
    /// moved to an isolated off-screen position for its preview camera and its layer is
    /// set accordingly. When the slot is cleared the piece layer is reset to Default and
    /// the RenderTexture is blanked.
    ///
    /// Maximum 5 slots supported (constrained by the 5 reserved DeckPreview layers).
    /// </summary>
    public sealed class DeckPreviewManager : MonoBehaviour
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unity layer index of the first DeckPreview layer (DeckPreview0).
        /// Layers DeckPreviewN = <see cref="BaseLayer"/> + N.
        /// </summary>
        public const int BaseLayer = 6;

        /// <summary>Maximum number of supported deck slots.</summary>
        public const int MaxSlots = 5;

        /// <summary>
        /// World-space X origin of the preview staging area.
        /// Pieces in preview are placed far off to the side, beyond the visible play area.
        /// </summary>
        private const float PreviewOriginX = -5000f;

        /// <summary>Spacing between preview slots on the X axis.</summary>
        private const float PreviewSpacingX = 100f;

        /// <summary>Z position of pieces in preview (in front of the preview cameras).</summary>
        private const float PreviewZ = -2f;

        /// <summary>Z position of each preview camera.</summary>
        private const float CameraZ = 5f;

        /// <summary>RenderTexture resolution (pixels).</summary>
        private const int TexSize = 256;

        // ── Slot state ────────────────────────────────────────────────────────────────

        private int          _slotCount;
        private Camera[]     _cameras;
        private RenderTexture[] _textures;
        private GameObject[] _slotPieces;  // current piece GO per slot (null = empty)

        // ── Lifecycle ─────────────────────────────────────────────────────────────────

        private void OnDestroy() => ReleaseAll();

        // ── Public API ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises preview cameras for <paramref name="slotCount"/> slots.
        /// Tears down any existing cameras first. Safe to call multiple times (one per level).
        /// </summary>
        public void Setup(int slotCount)
        {
            ReleaseAll();

            _slotCount  = Mathf.Min(slotCount, MaxSlots);
            _cameras    = new Camera[_slotCount];
            _textures   = new RenderTexture[_slotCount];
            _slotPieces = new GameObject[_slotCount];

            for (int i = 0; i < _slotCount; i++)
            {
                int layer = BaseLayer + i;
                var rt    = CreateRT();
                _textures[i] = rt;

                var camGo = new GameObject($"DeckPreviewCam_{i}");
                camGo.transform.SetParent(transform, false);

                float worldX = PreviewOriginX + i * PreviewSpacingX;
                camGo.transform.position = new Vector3(worldX, 0f, CameraZ);
                camGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // look toward -Z

                var cam = camGo.AddComponent<Camera>();
                cam.orthographic      = true;
                cam.orthographicSize  = 0.6f;   // will be fitted per-piece when assigned
                cam.clearFlags        = CameraClearFlags.SolidColor;
                cam.backgroundColor   = Color.clear;
                cam.cullingMask       = 1 << layer;
                cam.targetTexture     = rt;
                cam.enabled           = false;  // enabled only when a piece is assigned
                cam.nearClipPlane     = 0.1f;
                cam.farClipPlane      = 20f;
                cam.depth             = -10f;   // render before main camera

                _cameras[i] = cam;
            }
        }

        /// <summary>
        /// Returns the <see cref="RenderTexture"/> for slot <paramref name="slotIndex"/>,
        /// or null if the index is out of range.
        /// </summary>
        public RenderTexture GetTexture(int slotIndex) =>
            _textures != null && slotIndex < _textures.Length ? _textures[slotIndex] : null;

        /// <summary>
        /// Assigns <paramref name="pieceGo"/> to slot <paramref name="slotIndex"/> for preview.
        /// Moves the piece to the preview staging area, sets its layer, and enables the camera.
        /// </summary>
        public void SetSlotPiece(int slotIndex, GameObject pieceGo)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;

            // Clear previous occupant if any
            ClearSlotInternal(slotIndex, disableCamera: false);

            if (pieceGo == null) { ClearSlotInternal(slotIndex, disableCamera: true); return; }

            int   layer  = BaseLayer + slotIndex;
            float worldX = PreviewOriginX + slotIndex * PreviewSpacingX;
            var   pos    = new Vector3(worldX, 0f, PreviewZ);

            // Position piece in front of its preview camera
            pieceGo.transform.position   = pos;
            pieceGo.transform.rotation   = Quaternion.identity;

            // Set layer recursively so child renderers (outline etc.) are also captured
            SetLayerRecursive(pieceGo, layer);

            // Fit camera orthographic size to piece bounds
            FitCamera(_cameras[slotIndex], pieceGo, pos);

            _cameras[slotIndex].enabled = true;
            _slotPieces[slotIndex] = pieceGo;
        }

        /// <summary>
        /// Clears slot <paramref name="slotIndex"/>: resets the piece layer to Default,
        /// moves it back to hidden, disables the camera, and clears the RenderTexture.
        /// </summary>
        public void ClearSlot(int slotIndex, Vector3 hiddenWorldPos)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;

            var go = _slotPieces[slotIndex];
            if (go != null)
            {
                SetLayerRecursive(go, 0); // back to Default
                go.transform.position = hiddenWorldPos;
            }

            ClearSlotInternal(slotIndex, disableCamera: true);
        }

        // ── Internals ─────────────────────────────────────────────────────────────────

        private void ClearSlotInternal(int slotIndex, bool disableCamera)
        {
            _slotPieces[slotIndex] = null;
            if (disableCamera && _cameras[slotIndex] != null)
                _cameras[slotIndex].enabled = false;

            // Blank the RenderTexture so the button shows nothing while empty
            if (_textures[slotIndex] != null)
            {
                var prev = RenderTexture.active;
                RenderTexture.active = _textures[slotIndex];
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = prev;
            }
        }

        private void FitCamera(Camera cam, GameObject pieceGo, Vector3 centrePos)
        {
            // Compute bounds of all MeshRenderers in the piece
            var renderers = pieceGo.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0) { cam.orthographicSize = 0.6f; return; }

            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            float halfW = bounds.extents.x;
            float halfH = bounds.extents.y;
            float fit   = Mathf.Max(halfW, halfH) * 1.15f; // 15% padding
            cam.orthographicSize = Mathf.Max(fit, 0.1f);

            // Centre camera on piece centroid (X/Y; Z stays fixed)
            var camPos = cam.transform.position;
            cam.transform.position = new Vector3(centrePos.x, centrePos.y, camPos.z);
        }

        private static RenderTexture CreateRT()
        {
            var rt = new RenderTexture(TexSize, TexSize, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 2,
                filterMode   = FilterMode.Bilinear,
                wrapMode     = TextureWrapMode.Clamp
            };
            rt.Create();
            return rt;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        private void ReleaseAll()
        {
            if (_cameras != null)
                foreach (var cam in _cameras)
                    if (cam != null) Destroy(cam.gameObject);

            if (_textures != null)
                foreach (var rt in _textures)
                    if (rt != null) rt.Release();

            _cameras    = null;
            _textures   = null;
            _slotPieces = null;
            _slotCount  = 0;
        }
    }
}
