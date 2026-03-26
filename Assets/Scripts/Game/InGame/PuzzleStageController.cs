using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.TransitionManagement;
using SimpleGame.Core.Unity.TransitionManagement;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Puzzle;
using SimpleJigsaw;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// MonoBehaviour that owns all 3D puzzle stage concerns:
    /// piece GameObject spawning, tray slot layout (LateUpdate), UGUI slot buttons,
    /// piece reveal/shake/slide tweens, and retry reset.
    ///
    /// Wired via [SerializeField] on InGameSceneController. Call SpawnLevel() once
    /// per level start (or retry) to populate the stage. Reset() tears down and
    /// prepares for the next SpawnLevel call.
    ///
    /// <para>The stage does not own game rules — it only knows about GameObjects,
    /// transforms, and how to animate them. All rule decisions are made by
    /// InGameFlowPresenter and routed through InGameView callbacks.</para>
    /// </summary>
    public class PuzzleStageController : MonoBehaviour
    {
        [Header("Puzzle Rendering")]
        [SerializeField] private SimpleJigsaw.GridLayoutConfig _gridLayoutConfig;
        [SerializeField] private SimpleJigsaw.PieceRenderConfig _pieceRenderConfig;
        [SerializeField] private Transform _puzzleParent;

        [Header("Transitions")]
        [SerializeField] private UnityTransitionPlayer _transitionPlayer;

        // ── View reference ────────────────────────────────────────────────────────────
        [SerializeField] private InGameView _inGameView;
        private SimpleGame.Core.PopupManagement.PopupManager<PopupId> _popupManager;

        // ── Piece tracking ────────────────────────────────────────────────────────────

        /// <summary>Spawned piece GameObjects — destroyed on Reset.</summary>
        private List<GameObject> _spawnedPieces;

        /// <summary>Piece id → spawned GameObject.</summary>
        private Dictionary<int, GameObject> _pieceObjects;

        /// <summary>Piece id → solved world position.</summary>
        private Dictionary<int, Vector3> _solvedWorldPositions;

        /// <summary>Non-seed piece id → (tray world position, tray local scale) for reset on Retry.</summary>
        private Dictionary<int, (Vector3 pos, Vector3 scale)> _traySlotData;

        /// <summary>Snapshot of each piece's initial tray position at spawn — used by Retry reset.</summary>
        private Dictionary<int, (Vector3 pos, Vector3 scale)> _initialTrayData;

        /// <summary>World-space centre positions of the visible tray slots (updated each LateUpdate).</summary>
        private Vector3[] _traySlotPositions;

        /// <summary>Scale for each tray slot.</summary>
        private Vector3[] _traySlotScales;

        /// <summary>Grid dimensions of the current level — stored for camera framing and tray sizing.</summary>
        private int _currentGridRows;
        private int _currentGridCols;

        /// <summary>UGUI Buttons for each tray slot — invisible, positioned over 3D slot pieces.</summary>
        private UnityEngine.UI.Button[] _slotButtons;

        /// <summary>Canvas that hosts the slot buttons (Screen Space Overlay).</summary>
        private Canvas _slotButtonCanvas;

        /// <summary>
        /// Piece IDs currently mid-shake. LateUpdate skips repositioning these so the
        /// shake tween can animate position freely without being overwritten each frame.
        /// </summary>
        private readonly HashSet<int> _shakingPieces = new();

        /// <summary>Runtime GridLayoutConfig created per SpawnLevel; destroyed on Reset.</summary>
        private SimpleJigsaw.GridLayoutConfig _runtimeGridConfig;

        // ── Public API ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Wire the popup manager at runtime. Called by InGameSceneController.Initialize().
        /// _inGameView is wired via [SerializeField] in the scene.
        /// </summary>
        public void SetContext(SimpleGame.Core.PopupManagement.PopupManager<PopupId> popupManager)
        {
            _popupManager = popupManager;
        }

        /// <summary>
        /// Spawn all piece GameObjects for this level, lay out the initial tray, wire
        /// piece-position callbacks on InGameView, and create slot buttons.
        ///
        /// Returns the model factory closure that InGameFlowPresenter should call per retry
        /// (the factory itself calls SpawnLevel again with a fresh board).
        /// </summary>
        public void SpawnLevel(SimpleJigsaw.PuzzleBoard rawBoard, int seedPieceId, int slotCount,
                               IReadOnlyList<int> deckOrder, int gridCols)
        {
            // Destroy pieces from previous level
            if (_spawnedPieces != null)
            {
                foreach (var old in _spawnedPieces)
                    if (old != null) Destroy(old);
                _spawnedPieces = null;
            }

            // Destroy previous hint surface
            var parent = _puzzleParent != null ? _puzzleParent : transform;
            var oldHint = parent.Find("HintSurface");
            if (oldHint != null) Destroy(oldHint.gameObject);

            var config = _pieceRenderConfig;

            parent.localScale = Vector3.one;
            parent.position   = Vector3.zero;

            // Store grid dims for camera framing and tray sizing
            int gridRows = rawBoard.Pieces.Count > 0
                ? Mathf.Max(1, Mathf.RoundToInt(rawBoard.Pieces.Count / (float)gridCols))
                : 1;
            _currentGridRows = gridRows;
            _currentGridCols = gridCols;

            // Spawn piece GameObjects
            List<GameObject> pieces;
            if (config != null && config.PieceShader != null)
                pieces = SimpleJigsaw.PieceObjectFactory.CreateAll(rawBoard, config, parent);
            else
                pieces = SimpleJigsaw.PieceObjectFactory.CreateAll(rawBoard,
                    new UnityEngine.Material(Shader.Find("Standard") ??
                                             Shader.Find("Universal Render Pipeline/Lit")),
                    parent);

            _spawnedPieces = pieces;

            _pieceObjects         = new Dictionary<int, GameObject>(pieces.Count);
            _solvedWorldPositions = new Dictionary<int, Vector3>(pieces.Count);

            for (int i = 0; i < pieces.Count; i++)
            {
                var pid = rawBoard.Pieces[i].Id;
                var go  = pieces[i];
                _pieceObjects[pid]         = go;
                _solvedWorldPositions[pid] = go.transform.position;

                var mesh = go.GetComponent<MeshFilter>()?.sharedMesh;
                var box  = go.AddComponent<BoxCollider>();
                if (mesh != null) { box.center = mesh.bounds.center; box.size = mesh.bounds.size; }

                if (pid == seedPieceId)
                    box.enabled = false;
            }

            // Hint surface — behind pieces at z = +0.1
            var hintMesh = SimpleJigsaw.HintSurfaceBuilder.Build(rawBoard.Pieces, thickness: 0.02f, zDepth: 0.1f);
            if (hintMesh != null)
            {
                var hintGo = new GameObject("HintSurface");
                hintGo.transform.SetParent(parent, worldPositionStays: false);
                hintGo.transform.localPosition = Vector3.zero;
                hintGo.transform.localScale    = Vector3.one;
                hintGo.AddComponent<MeshFilter>().sharedMesh = hintMesh;
                var hintRenderer = hintGo.AddComponent<MeshRenderer>();
                hintRenderer.sharedMaterial = new UnityEngine.Material(
                    Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            }

            // Initial tray slot positions — same formula as LateUpdate
            var cam = Camera.main;
            float orthoH = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float orthoW = cam != null ? orthoH * cam.aspect : 18f;
            float camX   = cam != null ? cam.transform.position.x : 0f;
            float camY   = cam != null ? cam.transform.position.y : 0f;

            float unitScale = Mathf.Max(gridRows, gridCols);
            float cellH     = unitScale / gridRows;
            float cellW     = unitScale / gridCols;
            float slotScale  = 1f;
            float slotWorldW = cellW * slotScale;
            float maxByWidth = slotCount > 0 ? (orthoW * 0.92f) / slotCount : orthoW;
            if (slotWorldW > maxByWidth) slotScale = maxByWidth / cellW;

            float slotWorldWFinal = cellW * slotScale;
            float slotWorldHFinal = cellH * slotScale;
            float trayY      = camY - orthoH * 0.5f + slotWorldHFinal * 0.5f + 0.1f;
            float totalTrayW = orthoW * 0.92f;
            float slotSpacing = slotCount > 1
                ? (totalTrayW - slotWorldWFinal) / (slotCount - 1)
                : 0f;
            float trayStartX  = camX - (slotSpacing * (slotCount - 1)) * 0.5f;

            _traySlotPositions = new Vector3[slotCount];
            _traySlotScales    = new Vector3[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                float x = trayStartX + i * slotSpacing;
                _traySlotPositions[i] = new Vector3(x, trayY, -2f);
                _traySlotScales[i]    = Vector3.one * slotScale;
            }

            // Hidden off-screen position for pieces not yet drawn into a slot
            var hiddenPos = new Vector3(camX + orthoW * 2f, trayY, -2f);

            _traySlotData = new Dictionary<int, (Vector3 pos, Vector3 scale)>();

            var deckSlotIndex = new Dictionary<int, int>();
            for (int i = 0; i < slotCount && i < deckOrder.Count; i++)
                deckSlotIndex[deckOrder[i]] = i;

            foreach (var desc in rawBoard.Pieces)
            {
                if (desc.Id == seedPieceId) continue;
                if (!_pieceObjects.TryGetValue(desc.Id, out var go)) continue;

                Vector3 pos, scale;
                if (deckSlotIndex.TryGetValue(desc.Id, out int slotIdx))
                {
                    pos   = _traySlotPositions[slotIdx];
                    scale = _traySlotScales[slotIdx];
                }
                else
                {
                    pos   = hiddenPos;
                    scale = _traySlotScales[slotCount - 1];
                }

                go.transform.SetParent(null, worldPositionStays: false);
                go.transform.position   = pos;
                go.transform.localScale = scale;

                _traySlotData[desc.Id] = (pos, scale);
            }

            _initialTrayData = new Dictionary<int, (Vector3, Vector3)>(_traySlotData);

            // Wire piece-position callbacks onto InGameView
            if (_inGameView != null)
            {
                _inGameView.RegisterPieceCallbacks(
                    onMovePieceToSlot: MovePieceToTraySlot,
                    onRevealPiece:     RevealPiece,
                    onShakePiece:      ShakePieceInSlot
                );
            }

            SpawnSlotButtons(slotCount);

            Debug.Log($"[PuzzleStageController] Spawned {pieces.Count} pieces — 1 seed, {deckOrder.Count} in deck, {slotCount} visible slots. Board: {gridRows}x{gridCols}");
        }

        /// <summary>
        /// Reset all non-seed pieces back to their initial tray positions.
        /// Called before a retry so the stage is ready for the next SpawnLevel.
        /// </summary>
        public void ResetPiecesToTray()
        {
            if (_pieceObjects == null || _initialTrayData == null) return;

            foreach (var kv in _initialTrayData)
            {
                int pieceId      = kv.Key;
                var (pos, scale) = kv.Value;
                if (!_pieceObjects.TryGetValue(pieceId, out var go)) continue;

                go.transform.SetParent(null, worldPositionStays: false);
                go.transform.position   = pos;
                go.transform.localScale = scale;

                var col = go.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            _traySlotData = new Dictionary<int, (Vector3, Vector3)>(_initialTrayData);
        }

        /// <summary>
        /// Destroy the runtime GridLayoutConfig (if any) and clear spawned pieces.
        /// Called at the start of each RunAsync to clean up from the previous session.
        /// </summary>
        public void Reset()
        {
            if (_runtimeGridConfig != null)
            {
                Destroy(_runtimeGridConfig);
                _runtimeGridConfig = null;
            }
        }

        /// <summary>
        /// Create (or reuse) a runtime GridLayoutConfig derived from the serialized template.
        /// Returns null if no _gridLayoutConfig is assigned (stub/test path).
        /// </summary>
        public SimpleJigsaw.GridLayoutConfig CreateRuntimeGridConfig(int rows, int cols)
        {
            if (_gridLayoutConfig == null) return null;

            if (_runtimeGridConfig != null)
            {
                Destroy(_runtimeGridConfig);
            }

            _runtimeGridConfig = ScriptableObject.CreateInstance<SimpleJigsaw.GridLayoutConfig>();
            _runtimeGridConfig.Rows           = rows;
            _runtimeGridConfig.Columns        = cols;
            _runtimeGridConfig.EdgeProfile    = _gridLayoutConfig.EdgeProfile;
            _runtimeGridConfig.PieceThickness = _gridLayoutConfig.PieceThickness;
            return _runtimeGridConfig;
        }

        /// <summary>Whether a GridLayoutConfig template is assigned (real jigsaw path vs stub).</summary>
        public bool HasGridLayoutConfig => _gridLayoutConfig != null;

        /// <summary>
        /// Returns the wired transition player, or creates a minimal runtime one.
        /// Returns null if running in EditMode test context (no game loop available).
        /// </summary>
        public ITransitionPlayer GetTransitionPlayer()
        {
            if (_transitionPlayer != null) return _transitionPlayer;

            if (!Application.isPlaying) return null;

            var go     = new GameObject("RetryFadeOverlay");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            go.AddComponent<UnityEngine.UI.Image>().color = Color.black;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            var tp = go.AddComponent<UnityTransitionPlayer>();
            var field = typeof(UnityTransitionPlayer)
                .GetField("_canvasGroup",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(tp, cg);
            _transitionPlayer = tp;
            return tp;
        }

        // ── LateUpdate: reposition tray slot pieces and UGUI buttons ─────────────────

        private void LateUpdate()
        {
            if (_traySlotPositions == null || _traySlotPositions.Length == 0) return;

            var cam = Camera.main;
            if (cam == null) return;

            float orthoH = cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float orthoW = orthoH * cam.aspect;
            float camX   = cam.transform.position.x;
            float camY   = cam.transform.position.y;

            int   slotCount = _traySlotPositions.Length;
            float unitScale = Mathf.Max(_currentGridRows, _currentGridCols);
            float cellH     = _currentGridRows > 0 ? unitScale / _currentGridRows : 1f;
            float cellW     = _currentGridCols > 0 ? unitScale / _currentGridCols : 1f;

            float slotScale  = 1f;
            float slotWorldW = cellW * slotScale;
            float maxByWidth = slotCount > 0 ? (orthoW * 0.92f) / slotCount : orthoW;
            if (slotWorldW > maxByWidth) slotScale = maxByWidth / cellW;

            float slotWorldWFinal = cellW  * slotScale;
            float slotWorldHFinal = cellH  * slotScale;
            float trayY = camY - orthoH * 0.5f + slotWorldHFinal * 0.5f + 0.1f;

            float totalTrayW  = orthoW * 0.92f;
            float slotSpacing = slotCount > 1
                ? (totalTrayW - slotWorldWFinal) / (slotCount - 1)
                : 0f;
            float trayStartX  = camX - (slotSpacing * (slotCount - 1)) * 0.5f;

            var slotContents = _inGameView?.GetSlotContents();

            for (int i = 0; i < slotCount; i++)
            {
                float x      = trayStartX + i * slotSpacing;
                var newPos   = new Vector3(x, trayY, -2f);
                var newScale = Vector3.one * slotScale;
                _traySlotPositions[i] = newPos;
                _traySlotScales[i]    = newScale;

                if (slotContents != null && i < slotContents.Length && slotContents[i].HasValue)
                {
                    int pid = slotContents[i].Value;
                    if (_pieceObjects != null && _pieceObjects.TryGetValue(pid, out var go))
                    {
                        if (!_shakingPieces.Contains(pid))
                        {
                            go.transform.position   = newPos;
                            go.transform.localScale = newScale;
                        }
                        if (_traySlotData != null) _traySlotData[pid] = (newPos, newScale);
                    }
                }
            }

            if (_slotButtons == null) return;

            float canvasScale = _slotButtonCanvas != null ? _slotButtonCanvas.scaleFactor : 1f;
            if (canvasScale < 1e-4f) canvasScale = 1f;

            for (int i = 0; i < _slotButtons.Length && i < slotCount; i++)
            {
                var btn = _slotButtons[i];
                if (btn == null) continue;

                var rt = btn.GetComponent<RectTransform>();

                Vector3 screenPos = cam.WorldToScreenPoint(_traySlotPositions[i]);
                rt.anchoredPosition = new Vector2(screenPos.x / canvasScale,
                                                  screenPos.y / canvasScale);

                Vector3 rightEdge = cam.WorldToScreenPoint(_traySlotPositions[i] + Vector3.right * slotWorldWFinal * 0.5f);
                Vector3 leftEdge  = cam.WorldToScreenPoint(_traySlotPositions[i] - Vector3.right * slotWorldWFinal * 0.5f);
                Vector3 topEdge   = cam.WorldToScreenPoint(_traySlotPositions[i] + Vector3.up    * slotWorldHFinal * 0.5f);
                Vector3 botEdge   = cam.WorldToScreenPoint(_traySlotPositions[i] - Vector3.up    * slotWorldHFinal * 0.5f);

                float pxW = Mathf.Abs(rightEdge.x - leftEdge.x);
                float pxH = Mathf.Abs(topEdge.y   - botEdge.y);
                rt.sizeDelta = new Vector2(pxW / canvasScale, pxH / canvasScale);
            }
        }

        // ── Piece callbacks (wired into InGameView.RegisterPieceCallbacks) ───────────

        private void MovePieceToTraySlot(int pieceId, int slotIndex)
        {
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            if (_traySlotPositions == null || slotIndex >= _traySlotPositions.Length) return;

            var pos   = _traySlotPositions[slotIndex];
            var scale = _traySlotScales[slotIndex];

            go.transform.SetParent(null, worldPositionStays: false);
            PieceTweener.SlideToSlot(go, pos, scale, destroyCancellationToken).Forget();

            if (_traySlotData != null)
                _traySlotData[pieceId] = (pos, scale);
        }

        private void ShakePieceInSlot(int slotIndex)
        {
            if (_traySlotPositions == null || slotIndex >= _traySlotPositions.Length) return;
            var slotContents = _inGameView?.GetSlotContents();
            if (slotContents == null || slotIndex >= slotContents.Length) return;
            if (!slotContents[slotIndex].HasValue) return;
            int pieceId = slotContents[slotIndex].Value;
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;

            _shakingPieces.Add(pieceId);
            ShakePieceAsync(go, pieceId, _traySlotPositions[slotIndex], destroyCancellationToken).Forget();
        }

        private async UniTaskVoid ShakePieceAsync(GameObject go, int pieceId, Vector3 restPos,
                                                   CancellationToken ct)
        {
            try
            {
                await PieceTweener.ShakePiece(go, restPos, ct);
            }
            finally
            {
                _shakingPieces.Remove(pieceId);
                if (go != null && _traySlotPositions != null)
                {
                    var contents = _inGameView?.GetSlotContents();
                    if (contents != null)
                    {
                        for (int i = 0; i < contents.Length; i++)
                        {
                            if (contents[i] == pieceId && i < _traySlotPositions.Length)
                            {
                                go.transform.position   = _traySlotPositions[i];
                                go.transform.localScale = _traySlotScales[i];
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void RevealPiece(int pieceId)
        {
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            if (!_solvedWorldPositions.TryGetValue(pieceId, out var solved)) return;

            var boardParent = _puzzleParent != null ? _puzzleParent : transform;

            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            go.transform.SetParent(boardParent, worldPositionStays: true);
            var targetLocal = boardParent.InverseTransformPoint(solved);

            PieceTweener.PlaceOnBoard(go, targetLocal, destroyCancellationToken).Forget();
        }

        // ── Slot button spawning ──────────────────────────────────────────────────────

        private void SpawnSlotButtons(int slotCount)
        {
            if (_slotButtons != null)
            {
                foreach (var b in _slotButtons)
                    if (b != null) Destroy(b.gameObject);
            }
            _slotButtons = null;

            if (_slotButtonCanvas == null)
            {
                var canvasGo = new GameObject("SlotButtonCanvas");
                _slotButtonCanvas = canvasGo.AddComponent<Canvas>();
                _slotButtonCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
                _slotButtonCanvas.sortingOrder = 10;
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            _slotButtons = new UnityEngine.UI.Button[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                int slotIdx = i;

                var btnGo = new GameObject($"SlotButton_{i}");
                btnGo.transform.SetParent(_slotButtonCanvas.transform, false);

                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100f, 100f);

                var img = btnGo.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(1f, 1f, 1f, 0f);

                var btn = btnGo.AddComponent<UnityEngine.UI.Button>();
                btn.transition = UnityEngine.UI.Selectable.Transition.None;

                btn.onClick.AddListener(() =>
                {
                    if (_popupManager != null && _popupManager.HasActivePopup) return;
                    var contents = _inGameView?.GetSlotContents();
                    if (contents == null || slotIdx >= contents.Length) return;
                    if (!contents[slotIdx].HasValue) return;
                    int pid = contents[slotIdx].Value;
                    Debug.Log($"[SlotButton] Slot {slotIdx} tapped, piece {pid}");
                    _inGameView?.NotifyPieceTapped(pid);
                });

                _slotButtons[i] = btn;
            }
        }
    }
}
