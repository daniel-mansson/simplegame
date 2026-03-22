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
        private int _continueCostCoins = 100;

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

        /// <summary>Grid dimensions of the current level — stored for camera framing and tray sizing.</summary>
        private int _currentGridRows;
        private int _currentGridCols;

        /// <summary>UGUI Buttons for each tray slot — one per slot, repositioned each LateUpdate.</summary>
        private UnityEngine.UI.Button[] _slotButtons;

        /// <summary>Canvas that hosts the slot buttons (Screen Space Overlay).</summary>
        private Canvas _slotButtonCanvas;

        /// <summary>
        /// Piece IDs currently mid-shake. LateUpdate skips repositioning these so the
        /// shake tween can animate position freely without being overwritten each frame.
        /// </summary>
        private readonly System.Collections.Generic.HashSet<int> _shakingPieces = new();

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
        private System.Func<Cysharp.Threading.Tasks.UniTask> _onSessionEnd;
        private IAnalyticsService _analytics;
        private IAdService _adService;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, ProgressionService progression,
                               GameSessionService session, PopupManager<PopupId> popupManager,
                               IGoldenPieceService goldenPieces = null, IHeartService hearts = null,
                               ICoinsService coins = null, IViewResolver viewResolver = null,
                               ICurrencyOverlay overlay = null,
                               System.Func<Cysharp.Threading.Tasks.UniTask> onSessionEnd = null,
                               IAnalyticsService analytics = null,
                               GameRemoteConfig? remoteConfig = null,
                               IAdService adService = null)
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
            _onSessionEnd = onSessionEnd;
            _analytics = analytics;
            _adService = adService;

            // Remote config overrides SerializeField defaults when provided
            if (remoteConfig.HasValue)
            {
                _goldenPiecesPerWin = remoteConfig.Value.GoldenPiecesPerWin;
                _continueCostCoins  = remoteConfig.Value.ContinueCostCoins;
                _uiFactory.SetInitialHearts(remoteConfig.Value.InitialHearts);
            }
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
        /// Each frame: reposition tray slot pieces and UGUI buttons relative to camera-bottom.
        /// Slot pieces are placed at localScale=1 (they are already 1 world unit).
        /// Horizontal spacing shrinks to fit screen width if there are many slots.
        /// Button anchoredPosition is in canvas units (screen pixels / canvas.scaleFactor).
        /// </summary>
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

            // Pieces are 1 world unit at scale 1. Display them at natural size in the tray.
            // Shrink only if they would not all fit across 92% of screen width.
            float slotScale  = 1f;
            float slotWorldW = cellW * slotScale;
            float maxByWidth = slotCount > 0 ? (orthoW * 0.92f) / slotCount : orthoW;
            if (slotWorldW > maxByWidth) slotScale = maxByWidth / cellW;

            float slotWorldWFinal = cellW  * slotScale;
            float slotWorldHFinal = cellH  * slotScale;
            // Tray centre-Y: one half-piece height above the camera bottom edge
            float trayY = camY - orthoH * 0.5f + slotWorldHFinal * 0.5f + 0.1f;

            // Even spacing across 92% of screen width
            float totalTrayW  = orthoW * 0.92f;
            float slotSpacing = slotCount > 1
                ? (totalTrayW - slotWorldWFinal) / (slotCount - 1)
                : 0f;
            float trayStartX  = camX - (slotSpacing * (slotCount - 1)) * 0.5f;

            // Fetch slot contents once per frame (avoid repeated calls inside loops)
            var slotContents = _inGameView?.GetSlotContents();

            // Update 3D tray piece positions and scales each frame
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
                        // Skip repositioning pieces that are mid-shake so the tween can run freely
                        if (!_shakingPieces.Contains(pid))
                        {
                            go.transform.position   = newPos;
                            go.transform.localScale = newScale;
                        }
                        if (_traySlotData != null) _traySlotData[pid] = (newPos, newScale);
                    }
                }
            }

            // Reposition UGUI slot buttons to match 3D slot world positions.
            // anchoredPosition must be in canvas units, not screen pixels.
            // With ScreenSpaceOverlay + ScaleWithScreenSize, canvas units = screen pixels / scaleFactor.
            if (_slotButtons == null) return;

            float canvasScale = _slotButtonCanvas != null ? _slotButtonCanvas.scaleFactor : 1f;
            if (canvasScale < 1e-4f) canvasScale = 1f;

            for (int i = 0; i < _slotButtons.Length && i < slotCount; i++)
            {
                var btn = _slotButtons[i];
                if (btn == null) continue;

                var rt = btn.GetComponent<RectTransform>();

                // World → screen pixel → canvas unit
                Vector3 screenPos = cam.WorldToScreenPoint(_traySlotPositions[i]);
                rt.anchoredPosition = new Vector2(screenPos.x / canvasScale,
                                                  screenPos.y / canvasScale);

                // Size via projected pixel extents, then convert to canvas units
                Vector3 rightEdge = cam.WorldToScreenPoint(_traySlotPositions[i] + Vector3.right * slotWorldWFinal * 0.5f);
                Vector3 leftEdge  = cam.WorldToScreenPoint(_traySlotPositions[i] - Vector3.right * slotWorldWFinal * 0.5f);
                Vector3 topEdge   = cam.WorldToScreenPoint(_traySlotPositions[i] + Vector3.up    * slotWorldHFinal * 0.5f);
                Vector3 botEdge   = cam.WorldToScreenPoint(_traySlotPositions[i] - Vector3.up    * slotWorldHFinal * 0.5f);

                float pxW = Mathf.Abs(rightEdge.x - leftEdge.x);
                float pxH = Mathf.Abs(topEdge.y   - botEdge.y);
                rt.sizeDelta = new Vector2(pxW / canvasScale, pxH / canvasScale);
            }
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
                    var result = JigsawLevelFactory.BuildSolvable(_runtimeGridConfig, slotCount, initialSeed);
                    UnityEngine.Debug.Log($"[InGameSceneController] Level {_session.CurrentLevelId} seed={result.Seed} grid={gridSize.Rows}x{gridSize.Cols} slots={slotCount}");
                    if (_session.TotalPieces != result.PieceList.Count)
                        _session.ResetForNewGame(_session.CurrentLevelId, result.PieceList.Count);
                    SpawnPieces(result.RawBoard, result.SeedIds[0], slotCount, result.DeckOrder, gridSize.Cols);
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
                _analytics?.TrackLevelStarted(_session.CurrentLevelId.ToString());
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
                            _analytics?.TrackLevelCompleted(_session.CurrentLevelId.ToString());
                            // Brief pause so the player sees the last piece land before the popup
                            if (_winPopupDelaySec > 0f)
                                await UniTask.Delay(System.TimeSpan.FromSeconds(_winPopupDelaySec), cancellationToken: ct);
                            await HandleLevelCompletePopupAsync(ct);
                            if (_onSessionEnd != null) await _onSessionEnd();
                            return ScreenId.MainMenu;
                        }

                        if (action == InGameAction.Lose)
                        {
                            _session.Outcome = GameOutcome.Lose;
                            _analytics?.TrackLevelFailed(_session.CurrentLevelId.ToString());
                            var choice = await HandleLevelFailedPopupAsync(ct);

                            if (choice == LevelFailedChoice.Quit)
                            {
                                if (_onSessionEnd != null) await _onSessionEnd();
                                return ScreenId.MainMenu;
                            }

                            if (choice == LevelFailedChoice.WatchAd)
                            {
                                bool adWatched = await HandleRewardedAdAsync(ct);
                                if (adWatched)
                                {
                                    // Ad completed — restore hearts and continue from current state
                                    presenter.RestoreHeartsAndContinue();
                                    continue;
                                }
                                // Ad skipped, failed, or unavailable — treat as retry
                                _session.CurrentScore = 0;
                                await RetryTransitionAsync(ct);
                                break;
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
                        if (_coins != null && _coins.TrySpend(_continueCostCoins))
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

        private async UniTask<bool> HandleRewardedAdAsync(CancellationToken ct)
        {
            var view = _viewResolver?.Get<IRewardedAdView>();
            if (view == null)
            {
                Debug.LogWarning("[InGameSceneController] IRewardedAdView not found — cannot show rewarded ad popup.");
                return false;
            }

            var adService = _adService ?? new NullAdService();
            var presenter = new RewardedAdPresenter(view, adService);
            presenter.Initialize();
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.RewardedAd, ct);
                bool result = await presenter.WaitForResult();
                await _popupManager.DismissPopupAsync(ct);
                return result;
            }
            finally
            {
                presenter.Dispose();
            }
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
        /// Spawns piece GameObjects using PieceObjectFactory. Board parent at natural scale,
        /// hint surface behind pieces, tray slots at camera-relative positions.
        /// Called once per level start when a GridLayoutConfig is assigned.
        /// </summary>
        private void SpawnPieces(SimpleJigsaw.PuzzleBoard rawBoard, int seedPieceId, int slotCount, System.Collections.Generic.IReadOnlyList<int> deckOrder, int gridCols)
        {
            if (_inGameView == null) return;

            // Destroy any pieces from a previous level before spawning new ones.
            if (_spawnedPieces != null)
            {
                foreach (var old in _spawnedPieces)
                    if (old != null) UnityEngine.Object.Destroy(old);
                _spawnedPieces = null;
            }

            // Destroy previous hint surface if any
            var parent = _puzzleParent != null ? _puzzleParent : transform;
            var oldHint = parent.Find("HintSurface");
            if (oldHint != null) UnityEngine.Object.Destroy(oldHint.gameObject);

            var config = _pieceRenderConfig;

            // Board parent at natural scale, world origin
            // Pieces live in unit-scale space (longest piece edge = 1 world unit).
            // Camera frames the board by panning; no scaling transform needed.
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

            // Build piece lookup + record solved world positions
            _pieceObjects         = new Dictionary<int, GameObject>(pieces.Count);
            _solvedWorldPositions = new Dictionary<int, Vector3>(pieces.Count);

            for (int i = 0; i < pieces.Count; i++)
            {
                var pid = rawBoard.Pieces[i].Id;
                var go  = pieces[i];
                _pieceObjects[pid]         = go;
                // Board parent is identity -- world position == local position
                _solvedWorldPositions[pid] = go.transform.position;

                // BoxCollider sized to mesh bounds
                var mesh = go.GetComponent<MeshFilter>()?.sharedMesh;
                var box  = go.AddComponent<BoxCollider>();
                if (mesh != null) { box.center = mesh.bounds.center; box.size = mesh.bounds.size; }

                // Seed piece: already placed, collider disabled
                if (pid == seedPieceId)
                    box.enabled = false;
                // Non-seed board pieces have no tap handler -- tray input via UGUI buttons (S03)
            }

            // Hint surface -- behind pieces at z = +0.1
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

            // Initial tray slot positions -- same formula as LateUpdate so the
            // first frame matches subsequent frames without a pop.
            var cam = Camera.main;
            float orthoH = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float orthoW = cam != null ? orthoH * cam.aspect : 18f;
            float camX   = cam != null ? cam.transform.position.x : 0f;
            float camY   = cam != null ? cam.transform.position.y : 0f;

            // Pieces are 1 world unit -- display at natural scale, shrink only if too wide.
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

            // Map deck order index to slot index for initial positioning
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

            // Snapshot initial positions for Retry reset
            _initialTrayData = new Dictionary<int, (Vector3, Vector3)>(_traySlotData);

            // Wire piece-position callbacks onto InGameView
            _inGameView.RegisterPieceCallbacks(
                onMovePieceToSlot: MovePieceToTraySlot,
                onRevealPiece:     RevealPiece,
                onShakePiece:      ShakePieceInSlot
            );

            // Spawn UGUI slot buttons on the slot button canvas
            SpawnSlotButtons(slotCount);

            Debug.Log($"[InGameSceneController] Spawned {pieces.Count} pieces -- 1 seed, {deckOrder.Count} in deck, {slotCount} visible slots. Board: {gridRows}x{gridCols} unitScale={unitScale}");
        }

        /// <summary>
        /// Creates one UGUI Button per slot on the slot button canvas.
        /// Buttons are invisible (no image) and positioned over the 3D slot pieces.
        /// onClick fires OnTapPiece with the piece ID currently in that slot.
        /// </summary>
        private void SpawnSlotButtons(int slotCount)
        {
            // Destroy previous slot buttons
            if (_slotButtons != null)
            {
                foreach (var b in _slotButtons)
                    if (b != null) Destroy(b.gameObject);
            }
            _slotButtons = null;

            // Always create a dedicated overlay canvas for slot buttons.
            // Never reuse an existing canvas — FindObjectOfType<Canvas>() is unreliable
            // with multiple scenes loaded (Boot scene's InputBlocker canvas has a CanvasGroup
            // whose blocksRaycasts=false causes Graphic.Raycast() to reject all child Graphics).
            if (_slotButtonCanvas == null)
            {
                var canvasGo = new GameObject("SlotButtonCanvas");
                _slotButtonCanvas = canvasGo.AddComponent<Canvas>();
                _slotButtonCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
                _slotButtonCanvas.sortingOrder = 10; // above InGame HUD (0), below InputBlocker (100)
                canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            _slotButtons = new UnityEngine.UI.Button[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                int slotIdx = i; // capture for lambda

                // Create button GameObject
                var btnGo = new GameObject($"SlotButton_{i}");
                btnGo.transform.SetParent(_slotButtonCanvas.transform, false);

                // RectTransform: anchored to bottom-left (pixel coordinates)
                var rt = btnGo.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(100f, 100f); // initial size; updated in LateUpdate

                // Transparent Image required for Button to receive raycasts
                var img = btnGo.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(1f, 1f, 1f, 0f); // fully transparent

                // Button component
                var btn = btnGo.AddComponent<UnityEngine.UI.Button>();
                btn.transition = UnityEngine.UI.Selectable.Transition.None;

                // onClick: resolve current piece in slot and fire OnTapPiece
                btn.onClick.AddListener(() =>
                {
                    if (_popupManager != null && _popupManager.HasActivePopup) return;
                    var contents = _inGameView?.GetSlotContents();
                    if (contents == null || slotIdx >= contents.Length) return;
                    if (!contents[slotIdx].HasValue) return;
                    int pid = contents[slotIdx].Value;
                    Debug.Log($"[SlotButton] Slot {slotIdx} tapped, piece {pid}");
                    (_inGameView as InGameView)?.NotifyPieceTapped(pid);
                });

                _slotButtons[i] = btn;
            }
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
            var slotContents = _inGameView?.GetSlotContents();
            if (slotContents == null || slotIndex >= slotContents.Length) return;
            if (!slotContents[slotIndex].HasValue) return;
            int pieceId = slotContents[slotIndex].Value;
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;

            // Lock this piece so LateUpdate doesn't overwrite its position during the shake.
            _shakingPieces.Add(pieceId);
            ShakePieceAsync(go, pieceId, _traySlotPositions[slotIndex], destroyCancellationToken).Forget();
        }

        private async UniTaskVoid ShakePieceAsync(GameObject go, int pieceId, Vector3 restPos, System.Threading.CancellationToken ct)
        {
            try
            {
                await PieceTweener.ShakePiece(go, restPos, ct);
            }
            finally
            {
                _shakingPieces.Remove(pieceId);
                // Snap back to current slot position in case it drifted while camera panned
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
