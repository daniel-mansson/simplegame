using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.TransitionManagement;
using SimpleGame.Core.Unity.TransitionManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Puzzle;
using SimpleGame.Game.Services;
using SimpleGame.Puzzle;
using SimpleJigsaw;
using UnityEngine;
namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// SceneController for the InGame scene. Owns the InGamePresenter lifetime.
    /// RunAsync reads level context from GameSessionService, runs the gameplay
    /// loop (piece placement + hearts), shows outcome popups, and returns ScreenId for navigation.
    ///
    /// Win: earns golden pieces, calls ProgressionService.RegisterWin, shows LevelComplete popup.
    /// Lose: shows LevelFailed popup — Retry resets and loops, WatchAd grants extra hearts and loops, Quit returns MainMenu.
    ///
    /// Play-from-editor: when GameSessionService has no level set (CurrentLevelId == 0),
    /// the serialized defaults are used as fallback.
    /// </summary>
    public class InGameSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private InGameView _inGameView;
        [SerializeField] private int _defaultLevelId = 1;
        [SerializeField] private int _defaultTotalPieces = 10;
        [SerializeField] private int _goldenPiecesPerWin = 5;

        [Header("Puzzle Rendering")]
        [SerializeField] private SimpleJigsaw.GridLayoutConfig _gridLayoutConfig;
        [SerializeField] private SimpleJigsaw.PieceRenderConfig _pieceRenderConfig;
        [SerializeField] private Transform _puzzleParent;
        [SerializeField] private int _puzzleSeed = 42;
        [SerializeField] private int _seedPieceId = 0;

        /// <summary>Delay before win popup appears. Set to 0 in tests via SetWinPopupDelay().</summary>
        private float _winPopupDelaySec = 1.0f;

        [Header("Puzzle Model")]
        [SerializeField] private PuzzleModelConfig _puzzleModelConfig;

        [Header("Transitions")]
        [SerializeField] private UnityTransitionPlayer _transitionPlayer;

        /// <summary>Spawned piece GameObjects — destroyed on scene unload.</summary>
        private List<GameObject> _spawnedPieces;

        /// <summary>Piece id → spawned GameObject (populated in SpawnPieces).</summary>
        private Dictionary<int, GameObject> _pieceObjects;

        /// <summary>Piece id → solved world position (populated in SpawnPieces).</summary>
        private Dictionary<int, Vector3> _solvedWorldPositions;

        /// <summary>Non-seed piece id → (tray world position, tray local scale) for reset on Retry.</summary>
        private Dictionary<int, (Vector3 pos, Vector3 scale)> _traySlotData;

        /// <summary>Snapshot of each piece's initial tray position at spawn — used by Retry reset.</summary>
        private Dictionary<int, (Vector3 pos, Vector3 scale)> _initialTrayData;

        /// <summary>World-space centre positions of the 3 visible tray slots (set in SpawnPieces).</summary>
        private Vector3[] _traySlotPositions;

        /// <summary>Scale for each of the 3 tray slots (front is largest).</summary>
        private Vector3[] _traySlotScales;

        /// <summary>
        /// Optional model factory — overrides stub generation.
        /// Called at the start of each retry to produce a fresh PuzzleModel with reset deck state.
        /// </summary>
        private System.Func<SimpleGame.Puzzle.PuzzleModel> _modelFactory;

        /// <summary>Runtime GridLayoutConfig created per RunAsync; reused across retries, destroyed on next RunAsync entry.</summary>
        private SimpleJigsaw.GridLayoutConfig _runtimeGridConfig;

        /// <summary>Debug override — when set, bypasses LevelProgression and uses these values directly.</summary>
        private (int rows, int cols, int slots)? _debugOverride;

        /// <summary>
        /// Cancels any in-flight RunAsync (self-bootstrap or previous GameBootstrapper call)
        /// when a new RunAsync is entered. Ensures only one game loop runs at a time.
        /// </summary>
        private CancellationTokenSource _runCts;

        private IViewResolver _viewResolver;
        private IInGameView _viewOverride;
        private ILevelCompleteView _levelCompleteViewOverride;
        private ILevelFailedView _levelFailedViewOverride;

        private IInGameView ActiveView => _viewOverride != null ? _viewOverride : _inGameView;

        private ILevelCompleteView ActiveLevelCompleteView
        {
            get
            {
                if (_levelCompleteViewOverride != null) return _levelCompleteViewOverride;
                var found = _viewResolver?.Get<ILevelCompleteView>();
                if (found == null)
                    Debug.LogError("[InGameSceneController] LevelCompleteView not found in any loaded scene.");
                return found;
            }
        }

        private ILevelFailedView ActiveLevelFailedView
        {
            get
            {
                if (_levelFailedViewOverride != null) return _levelFailedViewOverride;
                var found = _viewResolver?.Get<ILevelFailedView>();
                if (found == null)
                    Debug.LogError("[InGameSceneController] LevelFailedView not found in any loaded scene.");
                return found;
            }
        }

        private UIFactory _uiFactory;
        private ProgressionService _progression;
        private GameSessionService _session;
        private IGoldenPieceService _goldenPieces;
        private IHeartService _hearts;
        private ICoinsService _coins;
        private ICurrencyOverlay _overlay;
        private PopupManager<PopupId> _popupManager;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, ProgressionService progression,
                               GameSessionService session, PopupManager<PopupId> popupManager,
                               IGoldenPieceService goldenPieces = null, IHeartService hearts = null,
                               ICoinsService coins = null, IViewResolver viewResolver = null,
                               ICurrencyOverlay overlay = null)
        {
            // Cancel any in-flight RunAsync before GameBootstrapper takes over.
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = null;

            _uiFactory = uiFactory;
            _progression = progression;
            _session = session;
            _popupManager = popupManager;
            _goldenPieces = goldenPieces;
            _hearts = hearts;
            _coins = coins;
            _viewResolver = viewResolver;
            _overlay = overlay;
        }

        /// <summary>
        /// For editor / test use: supply mock views that override the serialized fields.
        /// </summary>
        public void SetViewsForTesting(IInGameView inGameView,
                                        ILevelCompleteView levelCompleteView = null,
                                        ILevelFailedView levelFailedView = null)
        {
            _viewOverride = inGameView;
            _levelCompleteViewOverride = levelCompleteView;
            _levelFailedViewOverride = levelFailedView;
            _winPopupDelaySec = 0f;  // no delay in tests — UniTask.Delay(0) completes immediately
        }

        /// <summary>
        /// Play-from-editor bootstrap: called by Unity on Start when the InGame scene
        /// is entered directly (i.e. GameBootstrapper never ran). Creates stub services
        /// so RunAsync can proceed without the full boot chain.
        /// </summary>
        private void Start()
        {
            if (_uiFactory != null) return; // GameBootstrapper already called Initialize() — it will call RunAsync()

            // Boot is loading additively (BootInjector). Wait one frame so GameBootstrapper
            // gets a chance to call Initialize(). If it does, _uiFactory will be set and we skip.
            // If Boot is not in the build settings (pure editor play without Boot scene), self-bootstrap.
            WaitForBootOrSelfBootstrap().Forget();
        }

        private async UniTaskVoid WaitForBootOrSelfBootstrap()
        {
            // Give BootInjector + GameBootstrapper time to initialize us.
            // Use a longer wait and re-check — the Boot scene loads additively and may take several frames.
            for (int i = 0; i < 10; i++)
            {
                await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
                if (_uiFactory != null) return; // GameBootstrapper got here first — all good
            }

            Debug.Log("[InGameSceneController] Play-from-editor: bootstrapping with stub services.");
            var session         = new GameSessionService();
            var hearts          = new HeartService();
            var progression     = new ProgressionService();
            IGoldenPieceService goldenPieces = new NullGoldenPieceService();
            _uiFactory   = new UIFactory(new GameService(), progression, session, hearts, null, goldenPieces, null);
            _progression  = progression;
            _session      = session;
            _goldenPieces = goldenPieces;
            _hearts       = hearts;
            _popupManager = null;

            RunAsync().Forget();
        }

        /// <summary>No-op golden piece service for play-from-editor preview.</summary>
        private sealed class NullGoldenPieceService : IGoldenPieceService
        {
            public int Balance => 0;
            public void Earn(int amount) { }
            public bool TrySpend(int amount) => false;
            public void Save() { }
            public void ResetAll() { }
        }


        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            // Single-flight guard: cancel any previous RunAsync (self-bootstrap or stale call)
            // and replace _runCts so this invocation is the only live one.
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            ct = _runCts.Token;

            // Destroy any GridLayoutConfig from a previous run
            if (_runtimeGridConfig != null)
            {
                UnityEngine.Object.Destroy(_runtimeGridConfig);
                _runtimeGridConfig = null;
            }

            // Play-from-editor fallback: if no level was set via session, use defaults.
            if (_session.CurrentLevelId == 0)
            {
                _session.ResetForNewGame(_defaultLevelId, _defaultTotalPieces);
            }

            // Determine slot count from config (default 3 if no config assigned)
            // Slot count: debug override takes priority, then formula levelId/3+3 clamped to [1,5]
            int slotCount = _debugOverride.HasValue
                ? Mathf.Max(1, _debugOverride.Value.slots)
                : Mathf.Clamp(_session.CurrentLevelId / 3 + 3, 1, 5);

            // Determine the model factory: use injected factory, or real jigsaw, or fallback stub.
            System.Func<SimpleGame.Puzzle.PuzzleModel> modelFactory;
            if (_modelFactory != null)
            {
                // Test seam — use injected factory directly
                modelFactory = _modelFactory;
            }
            else if (_gridLayoutConfig != null)
            {
                // Derive grid size from level progression (level 1 = 3×3, level 2 = 3×4, etc.)
                // Derive grid size from level progression, unless a debug override is active
                var gridSize = _debugOverride.HasValue
                    ? new LevelProgression.GridSize(_debugOverride.Value.rows, _debugOverride.Value.cols)
                    : LevelProgression.GetGridSize(_session.CurrentLevelId);
                _runtimeGridConfig = UnityEngine.ScriptableObject.CreateInstance<SimpleJigsaw.GridLayoutConfig>();
                _runtimeGridConfig.Rows           = gridSize.Rows;
                _runtimeGridConfig.Columns        = gridSize.Cols;
                _runtimeGridConfig.EdgeProfile    = _gridLayoutConfig.EdgeProfile;
                _runtimeGridConfig.PieceThickness = _gridLayoutConfig.PieceThickness;

                // modelFactory re-builds with a fresh random seed each call (first run + every retry).
                // BuildSolvable runs a greedy solver and retries up to 100 seeds to guarantee solvability.
                // SpawnPieces is called inside so the piece GameObjects match the new layout.
                modelFactory = () =>
                {
                    int initialSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                    var result = JigsawLevelFactory.BuildSolvable(_runtimeGridConfig, slotCount, initialSeed, maxAttempts: 1000);
                    UnityEngine.Debug.Log($"[InGameSceneController] Level {_session.CurrentLevelId} seed={result.Seed} grid={gridSize.Rows}x{gridSize.Cols} slots={slotCount}");
                    if (_session.TotalPieces != result.PieceList.Count)
                        _session.ResetForNewGame(_session.CurrentLevelId, result.PieceList.Count);
                    SpawnPieces(result.RawBoard, result.SeedIds[0], slotCount, result.DeckOrder);
                    return new SimpleGame.Puzzle.PuzzleModel(result.PieceList, result.SeedIds, result.DeckOrder, slotCount);
                };
            }
            else
            {
                modelFactory = () => BuildStubModel(_session.TotalPieces, slotCount);
            }

            while (true)
            {
                // Rebuild model each retry — ensures deck state is fresh (new seed, new spawn)
                var model     = modelFactory();
                var presenter = _uiFactory.CreateInGamePresenter(ActiveView, model);
                presenter.Initialize();
                try
                {
                    // Inner loop: handles WatchAd continuation without restarting
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        var action = await presenter.WaitForAction(ct);

                        if (action == InGameAction.Win)
                        {
                            _goldenPieces?.Earn(_goldenPiecesPerWin);
                            _goldenPieces?.Save();

                            _progression.RegisterWin(_session.CurrentScore);
                            _session.Outcome = GameOutcome.Win;
                            // Brief pause so the player sees the last piece land before the popup
                            if (_winPopupDelaySec > 0f)
                                await UniTask.Delay(System.TimeSpan.FromSeconds(_winPopupDelaySec), cancellationToken: ct);
                            await HandleLevelCompletePopupAsync(ct);
                            return ScreenId.MainMenu;
                        }

                        if (action == InGameAction.Lose)
                        {
                            _session.Outcome = GameOutcome.Lose;
                            var choice = await HandleLevelFailedPopupAsync(ct);

                            if (choice == LevelFailedChoice.Quit)
                                return ScreenId.MainMenu;

                            if (choice == LevelFailedChoice.WatchAd)
                            {
                                await HandleRewardedAdAsync(ct);
                                // Continue with same presenter — restore hearts, keep piece progress
                                presenter.RestoreHeartsAndContinue();
                                continue;
                            }

                            if (choice == LevelFailedChoice.Continue)
                            {
                                // Coins already spent in HandleLevelFailedPopupAsync
                                presenter.RestoreHeartsAndContinue();
                                continue;
                            }

                            // Retry: fade out, reset pieces to tray, fade back in, fresh presenter
                            _session.CurrentScore = 0;
                            await RetryTransitionAsync(ct);
                            break;
                        }
                    }
                }
                finally
                {
                    presenter.Dispose();
                }
            }
        }

        private async UniTask HandleLevelCompletePopupAsync(CancellationToken ct)
        {
            var view = ActiveLevelCompleteView;
            if (view == null) return;

            var presenter = _uiFactory.CreateLevelCompletePresenter(view);
            presenter.Initialize(_session.CurrentScore, _session.CurrentLevelId, _goldenPiecesPerWin);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.LevelComplete, ct);
                await presenter.WaitForContinue();
            }
            finally
            {
                presenter.Dispose();
            }
        }

        private async UniTask<LevelFailedChoice> HandleLevelFailedPopupAsync(CancellationToken ct)
        {
            var view = ActiveLevelFailedView;
            if (view == null) return LevelFailedChoice.Quit;

            var presenter = _uiFactory.CreateLevelFailedPresenter(view);
            presenter.Initialize(_session.CurrentScore, _session.CurrentLevelId);

            // Show overlay with current coin balance
            if (_overlay != null)
            {
                _overlay.UpdateBalance($"Coins: {_coins?.Balance ?? 0}");
                await _overlay.ShowAsync(ct);
            }

            try
            {
                await _popupManager.ShowPopupAsync(PopupId.LevelFailed, ct);

                while (true)
                {
                    var choice = await presenter.WaitForChoice();

                    if (choice == LevelFailedChoice.Retry || choice == LevelFailedChoice.WatchAd || choice == LevelFailedChoice.Quit)
                    {
                        await _popupManager.DismissPopupAsync(ct);
                        return choice;
                    }

                    if (choice == LevelFailedChoice.Continue)
                    {
                        const int continueCost = 100;
                        if (_coins != null && _coins.TrySpend(continueCost))
                        {
                            _coins.Save();
                            _overlay?.UpdateBalance($"Coins: {_coins.Balance}");
                            await _popupManager.DismissPopupAsync(ct);
                            return LevelFailedChoice.Continue;
                        }
                        else
                        {
                            // Can't afford — open shop popup stacked on top
                            await HandleShopPopupAsync(ct);
                            // Update balance display after shop
                            _overlay?.UpdateBalance($"Coins: {_coins?.Balance ?? 0}");
                        }
                    }
                }
            }
            finally
            {
                presenter.Dispose();
                // Hide overlay when LevelFailed is dismissed
                if (_overlay != null)
                    _overlay.HideAsync(ct).Forget();
            }
        }

        private async UniTask HandleShopPopupAsync(CancellationToken ct)
        {
            var view = _viewResolver?.Get<IShopView>();
            if (view == null)
            {
                Debug.LogWarning("[InGameSceneController] ShopView not found — cannot open shop.");
                return;
            }

            var presenter = _uiFactory.CreateShopPresenter(view);
            presenter.Initialize();
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.Shop, ct);
                await presenter.WaitForResult();
                await _popupManager.DismissPopupAsync(ct);
            }
            finally
            {
                presenter.Dispose();
            }
        }

        private async UniTask HandleRewardedAdAsync(CancellationToken ct)
        {
            // Stub: show rewarded ad popup, wait for completion, grant retry
            await _popupManager.ShowPopupAsync(PopupId.RewardedAd, ct);
            // In a real implementation, the RewardedAdPresenter would handle
            // the ad flow. For the stub, we just dismiss after a beat.
            // The S06 integration will wire the real presenter.
            Debug.Log("[InGameSceneController] Rewarded ad stub — granting retry with extra hearts.");
            await _popupManager.DismissPopupAsync(ct);
        }

        /// <summary>
        /// Injects a level factory function, bypassing stub generation.
        /// Used by S04 (and tests) to supply real JigsawLevelFactory-built levels.
        /// The factory is called at the start of each retry to ensure fresh deck state.
        /// </summary>
        public void SetModelFactory(System.Func<SimpleGame.Puzzle.PuzzleModel> factory)
            => _modelFactory = factory;

        /// <summary>Test seam — remove the win popup delay so tests don't hang.</summary>
        public void SetWinPopupDelay(float seconds) => _winPopupDelaySec = seconds;

        /// <summary>
        /// Debug override — bypasses LevelProgression for the next RunAsync invocation.
        /// Pass null to clear and return to normal progression.
        /// </summary>
        public void SetDebugOverride(int rows, int cols, int slots)
            => _debugOverride = (rows, cols, slots);

        public void ClearDebugOverride() => _debugOverride = null;

        /// <summary>
        /// Spawns piece GameObjects using PieceObjectFactory and attaches PieceTapHandler to each.
        /// Called once at scene start when a GridLayoutConfig is assigned.
        /// </summary>
        private void SpawnPieces(SimpleJigsaw.PuzzleBoard rawBoard, int seedPieceId, int slotCount, System.Collections.Generic.IReadOnlyList<int> deckOrder)
        {
            if (_inGameView == null) return;

            // Destroy any pieces from a previous level before spawning new ones.
            // The InGame scene stays loaded across levels, so this prevents accumulation.
            if (_spawnedPieces != null)
            {
                foreach (var old in _spawnedPieces)
                    if (old != null) UnityEngine.Object.Destroy(old);
                _spawnedPieces = null;
            }

            var parent = _puzzleParent != null ? _puzzleParent : transform;
            var config = _pieceRenderConfig;

            List<GameObject> pieces;
            if (config != null && config.PieceShader != null)
                pieces = SimpleJigsaw.PieceObjectFactory.CreateAll(rawBoard, config, parent);
            else
                pieces = SimpleJigsaw.PieceObjectFactory.CreateAll(rawBoard,
                    new UnityEngine.Material(Shader.Find("Standard") ??
                                             Shader.Find("Universal Render Pipeline/Lit")),
                    parent);

            _spawnedPieces = pieces;

            // ── Layout zones (world units, camera ortho size 5 → ±5 y) ─────
            // Tray: bottom 32% of screen — 3 big visible pieces, no button
            // Board: remaining space above tray, minus top 0.8u for HUD
            var cam = Camera.main;
            float orthoH = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float orthoW = cam != null ? orthoH * cam.aspect : 18f;

            const float trayFraction = 0.32f;
            float trayH        = orthoH * trayFraction;
            float trayY        = -orthoH * 0.5f + trayH * 0.5f;
            float boardBottom  = -orthoH * 0.5f + trayH + 0.15f;
            float boardTop     =  orthoH * 0.5f - 0.8f;
            float boardH       =  boardTop - boardBottom;
            float boardSize    =  Mathf.Min(orthoW * 0.72f, boardH);    // 72% wide — slightly smaller

            // Scale PuzzleParent: [0,1]² → boardSize², z=-2 so pieces render in front of UI canvas
            parent.localScale = Vector3.one * boardSize;
            parent.position   = new Vector3(-boardSize * 0.5f, boardBottom, -2f);

            // ── Build piece lookup + record solved positions ───────────────
            _pieceObjects         = new Dictionary<int, GameObject>(pieces.Count);
            _solvedWorldPositions = new Dictionary<int, Vector3>(pieces.Count);

            for (int i = 0; i < pieces.Count; i++)
            {
                var pid = rawBoard.Pieces[i].Id;
                var go  = pieces[i];
                _pieceObjects[pid]         = go;
                _solvedWorldPositions[pid] = parent.TransformPoint(go.transform.localPosition);

                // BoxCollider sized to mesh bounds — reliable for OnMouseDown on any mesh shape
                var mesh = go.GetComponent<MeshFilter>()?.sharedMesh;
                var box  = go.AddComponent<BoxCollider>();
                if (mesh != null) { box.center = mesh.bounds.center; box.size = mesh.bounds.size; }

                // Seed piece is already placed — disable its collider immediately so it
                // never intercepts rays aimed at tray pieces.
                if (pid == seedPieceId)
                    box.enabled = false;
                else
                    go.AddComponent<PieceTapHandler>().Initialize(pid, _inGameView);
            }

            // ── Tray: slotCount slots — fill available width ──────────────
            // Each slot gets an equal share of screen width minus a small margin.
            float slotSize = (orthoW * 0.92f) / slotCount;

            // Compute normalised scale: pieces from large grids have smaller meshes in [0,1]² space.
            // Sample the first non-seed piece's mesh to derive the world extent at boardSize scale.
            float normSlotScale = slotSize; // fallback: assume mesh fills boardSize
            foreach (var kv in _pieceObjects)
            {
                if (kv.Key == seedPieceId) continue;
                var sampleMesh = kv.Value.GetComponent<MeshFilter>()?.sharedMesh;
                if (sampleMesh == null) continue;
                float extent = Mathf.Max(sampleMesh.bounds.size.x, sampleMesh.bounds.size.y) * boardSize;
                if (extent > 0.0001f) normSlotScale = slotSize / extent;
                break;
            }

            // Very tight packing — slots overlap slightly, held apart by a small fraction of piece size.
            float slotSpacing    = slotSize * 0.45f;
            float totalTrayWidth = slotSpacing * (slotCount - 1) + slotSize;
            float trayStartX     = -totalTrayWidth * 0.5f + slotSize * 0.5f;

            // Stagger: every second slot drops slightly so overlapping pieces read as distinct.
            float staggerY = slotSize * 0.18f;

            _traySlotPositions = new Vector3[slotCount];
            _traySlotScales    = new Vector3[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                float x = trayStartX + i * slotSpacing;
                float y = trayY + (i % 2 == 1 ? -staggerY : 0f);
                _traySlotPositions[i] = new Vector3(x, y, -2f);
                _traySlotScales[i]    = Vector3.one * normSlotScale;
            }

            // Hidden off-screen position for pieces not yet drawn into a slot
            var hiddenPos = new Vector3(orthoW * 2f, trayY, -2f);

            _traySlotData = new Dictionary<int, (Vector3 pos, Vector3 scale)>();

            // Build a set of the first slotCount deck pieces — these are the ones the model
            // puts in visible slots 0..slotCount-1 at start. All others start hidden.
            var initialSlotSet = new HashSet<int>();
            for (int i = 0; i < slotCount && i < deckOrder.Count; i++)
                initialSlotSet.Add(deckOrder[i]);

            // Map deck order index → slot index for positioning
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

            // Snapshot initial positions for Retry reset (before any MovePieceToTraySlot calls)
            _initialTrayData = new Dictionary<int, (Vector3, Vector3)>(_traySlotData);

            // ── Wire piece-position callbacks onto InGameView ──────────────
            _inGameView.RegisterPieceCallbacks(
                onMovePieceToSlot: MovePieceToTraySlot,
                onRevealPiece:     RevealPiece,
                onShakePiece:      ShakePieceInSlot
            );

            Debug.Log($"[InGameSceneController] Spawned {pieces.Count} pieces — 1 seed, {deckOrder.Count} in deck, {slotCount} visible slots.");
        }

        /// <summary>Move a piece from its tray world position to its solved board position.</summary>
        /// <summary>
        /// Move a piece to one of the 3 visible tray slots (called by RefreshTray).
        /// Also updates _traySlotData so Retry reset uses the slot position.
        /// </summary>
        private void MovePieceToTraySlot(int pieceId, int slotIndex)
        {
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            if (_traySlotPositions == null || slotIndex >= _traySlotPositions.Length) return;

            var pos   = _traySlotPositions[slotIndex];
            var scale = _traySlotScales[slotIndex];

            go.transform.SetParent(null, worldPositionStays: false);

            // Animate slide to tray slot (cancel any in-flight tween first)
            PieceTweener.SlideToSlot(go, pos, scale, destroyCancellationToken).Forget();

            if (_traySlotData != null)
                _traySlotData[pieceId] = (pos, scale);
        }

        private void ShakePieceInSlot(int slotIndex)
        {
            if (_traySlotPositions == null || slotIndex >= _traySlotPositions.Length) return;
            // Resolve which piece ID is in this slot via the view's slot contents mirror
            var slotContents = _inGameView?.GetSlotContents();
            if (slotContents == null || slotIndex >= slotContents.Length) return;
            if (!slotContents[slotIndex].HasValue) return;
            int pieceId = slotContents[slotIndex].Value;
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            PieceTweener.ShakePiece(go, _traySlotPositions[slotIndex], destroyCancellationToken).Forget();
        }

        private void RevealPiece(int pieceId)
        {
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            if (!_solvedWorldPositions.TryGetValue(pieceId, out var solved)) return;

            var boardParent = _puzzleParent != null ? _puzzleParent : transform;

            // Disable collider immediately — board pieces must not block tray taps
            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Reparent so localScale=1 lands at the right world size
            go.transform.SetParent(boardParent, worldPositionStays: true);
            var targetLocal = boardParent.InverseTransformPoint(solved);

            // Animate: anticipation wind-up → fly to board → settle
            PieceTweener.PlaceOnBoard(go, targetLocal, destroyCancellationToken).Forget();
        }

        /// <summary>
        /// Fade to black, reset all non-seed pieces back to their tray positions,
        /// then fade back in — gives a clean level-restart feel without a scene reload.
        /// </summary>
        private async UniTask RetryTransitionAsync(CancellationToken ct)
        {
            var tp = GetTransitionPlayer();
            if (tp != null) await tp.FadeOutAsync(ct);
            // Pieces are re-spawned by modelFactory() after this returns — no reset needed.
            if (tp != null) await tp.FadeInAsync(ct);
        }

        /// <summary>
        /// Move all non-seed pieces back to their tray positions and scale.
        /// The seed piece stays at its solved position (it is the board anchor).
        /// </summary>
        private void ResetPiecesToTray()
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

                // Re-enable collider so tray pieces are tappable again
                var col = go.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            // Restore mutable slot data to initial state for next session
            _traySlotData = new Dictionary<int, (Vector3, Vector3)>(_initialTrayData);
        }

        /// <summary>
        /// Returns the wired transition player, or creates a minimal runtime one.
        /// Returns null if running in EditMode test context (no game loop available).
        /// </summary>
        private ITransitionPlayer GetTransitionPlayer()
        {
            if (_transitionPlayer != null) return _transitionPlayer;

            // In EditMode tests there is no game loop — skip the fade.
            if (!Application.isPlaying) return null;

            // Runtime fallback: create a simple fade overlay
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

        /// <summary>
        /// Builds a linear-chain stub <see cref="SimpleGame.Puzzle.PuzzleModel"/> for
        /// play-from-editor bootstrapping when no GridLayoutConfig is assigned.
        /// </summary>
        private static SimpleGame.Puzzle.PuzzleModel BuildStubModel(int totalPieces, int slotCount)
        {
            if (totalPieces <= 0) totalPieces = 1;

            var pieces = new System.Collections.Generic.List<IPuzzlePiece>(totalPieces);
            for (int i = 0; i < totalPieces; i++)
            {
                var neighbors = new System.Collections.Generic.List<int>();
                if (i > 0) neighbors.Add(i - 1);
                if (i < totalPieces - 1) neighbors.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, neighbors));
            }

            var seeds = new[] { 0 };
            var deckOrder = new int[totalPieces - 1];
            for (int i = 0; i < deckOrder.Length; i++) deckOrder[i] = i + 1;

            return new SimpleGame.Puzzle.PuzzleModel(pieces, seeds, deckOrder, slotCount);
        }
    }
}
