using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
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

        /// <summary>Spawned piece GameObjects — destroyed on scene unload.</summary>
        private List<GameObject> _spawnedPieces;

        /// <summary>Piece id → spawned GameObject (populated in SpawnPieces).</summary>
        private Dictionary<int, GameObject> _pieceObjects;

        /// <summary>Piece id → solved world position (populated in SpawnPieces).</summary>
        private Dictionary<int, Vector3> _solvedWorldPositions;

        /// <summary>The active puzzle level for the current run. Set in RunAsync.</summary>
        private IPuzzleLevel _currentLevel;

        /// <summary>
        /// Optional level factory — overrides stub generation.
        /// Called at the start of each retry to produce a fresh level with reset deck state.
        /// </summary>
        private System.Func<IPuzzleLevel> _levelFactory;

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
            // Give BootInjector + GameBootstrapper time to initialize us (~2 frames is enough).
            await UniTask.DelayFrame(3);
            if (_uiFactory != null) return; // GameBootstrapper got here first — all good

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
            // Play-from-editor fallback: if no level was set via session, use defaults.
            if (_session.CurrentLevelId == 0)
            {
                _session.ResetForNewGame(_defaultLevelId, _defaultTotalPieces);
            }

            // Determine the level factory: use injected factory, or real jigsaw, or fallback stub.
            // S04: use JigsawLevelFactory when a GridLayoutConfig is assigned.
            System.Func<IPuzzleLevel> factory;
            if (_levelFactory != null)
            {
                factory = _levelFactory;
            }
            else if (_gridLayoutConfig != null)
            {
                var buildResult = JigsawLevelFactory.Build(
                    _gridLayoutConfig, _puzzleSeed,
                    seedPieceIds: new[] { _seedPieceId });

                // Fix piece count in session to match actual grid (e.g. 3×3 = 9, not _defaultTotalPieces)
                if (_session.TotalPieces != buildResult.Level.TotalPieceCount)
                    _session.ResetForNewGame(_session.CurrentLevelId, buildResult.Level.TotalPieceCount);

                // Spawn piece GameObjects with tap handlers (once per scene load)
                SpawnPieces(buildResult.RawBoard);

                factory = () => JigsawLevelFactory.Build(
                    _gridLayoutConfig, _puzzleSeed,
                    seedPieceIds: new[] { _seedPieceId }).Level;
            }
            else
            {
                factory = () => BuildStubLevel(_session.TotalPieces);
            }

            while (true)
            {
                // Rebuild level each retry — ensures deck state is fresh
                _currentLevel = factory();
                var presenter = _uiFactory.CreateInGamePresenter(ActiveView, _currentLevel);
                presenter.Initialize();
                try
                {
                    // Inner loop: handles WatchAd continuation without restarting
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        var action = await presenter.WaitForAction();

                        if (action == InGameAction.Win)
                        {
                            _goldenPieces?.Earn(_goldenPiecesPerWin);
                            _goldenPieces?.Save();

                            _progression.RegisterWin(_session.CurrentScore);
                            _session.Outcome = GameOutcome.Win;
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

                            // Retry: break inner loop to create fresh presenter
                            _session.CurrentScore = 0;
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
        public void SetLevelFactory(System.Func<IPuzzleLevel> factory) => _levelFactory = factory;

        /// <summary>
        /// Spawns piece GameObjects using PieceObjectFactory and attaches PieceTapHandler to each.
        /// Called once at scene start when a GridLayoutConfig is assigned.
        /// </summary>
        private void SpawnPieces(SimpleJigsaw.PuzzleBoard rawBoard)
        {
            if (_inGameView == null) return;

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
            // Tray: bottom 22% of screen
            // Board: everything above tray, minus top 0.8u for HUD
            var cam = Camera.main;
            float orthoH = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            float orthoW = cam != null ? orthoH * cam.aspect : 18f;

            float trayFraction = 0.22f;
            float trayH        = orthoH * trayFraction;
            float trayY        = -orthoH * 0.5f + trayH * 0.5f;
            float boardBottom  = -orthoH * 0.5f + trayH + 0.15f;
            float boardTop     =  orthoH * 0.5f - 0.8f;
            float boardH       =  boardTop - boardBottom;
            float boardSize    =  Mathf.Min(orthoW * 0.9f, boardH);

            // Scale PuzzleParent: [0,1]² → boardSize²
            parent.localScale = Vector3.one * boardSize;
            parent.position   = new Vector3(-boardSize * 0.5f, boardBottom, 0f);

            // ── Build piece lookup + record solved positions ───────────────
            _pieceObjects         = new Dictionary<int, GameObject>(pieces.Count);
            _solvedWorldPositions = new Dictionary<int, Vector3>(pieces.Count);

            for (int i = 0; i < pieces.Count; i++)
            {
                var pid = rawBoard.Pieces[i].Id;
                var go  = pieces[i];
                _pieceObjects[pid]         = go;
                _solvedWorldPositions[pid] = parent.TransformPoint(go.transform.localPosition);

                var mesh = go.GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh != null)
                    go.AddComponent<MeshCollider>().sharedMesh = mesh;

                go.AddComponent<PieceTapHandler>().Initialize(pid, _inGameView);
            }

            // ── Tray: move non-seed pieces below the board ─────────────────
            int nonSeedCount   = rawBoard.Pieces.Count - 1; // one seed
            float slotW        = Mathf.Min(trayH * 0.85f, orthoW / Mathf.Max(nonSeedCount, 1) * 0.88f);
            float trayStartX   = -(slotW * nonSeedCount) * 0.5f + slotW * 0.5f;

            int trayIdx = 0;
            foreach (var desc in rawBoard.Pieces)
            {
                if (desc.Id == _seedPieceId) continue;
                if (!_pieceObjects.TryGetValue(desc.Id, out var go)) continue;

                go.transform.SetParent(null, worldPositionStays: false);
                go.transform.position   = new Vector3(trayStartX + trayIdx * slotW, trayY, 0f);
                go.transform.localScale = Vector3.one * slotW;
                trayIdx++;
            }

            // ── Wire piece-position callbacks onto InGameView ──────────────
            _inGameView.RegisterPieceCallbacks(
                onRevealPiece:   RevealPiece,
                onShowDeckPiece: _ => { /* tray pieces stay put; front piece is implicit */ },
                onHideDeckPanel: null
            );

            Debug.Log($"[InGameSceneController] Spawned {pieces.Count} pieces — 1 seed, {nonSeedCount} in tray.");
        }

        /// <summary>Move a piece from its tray world position to its solved board position.</summary>
        private void RevealPiece(int pieceId)
        {
            if (!_pieceObjects.TryGetValue(pieceId, out var go)) return;
            if (!_solvedWorldPositions.TryGetValue(pieceId, out var solved)) return;

            var boardParent = _puzzleParent != null ? _puzzleParent : transform;
            go.transform.SetParent(boardParent, worldPositionStays: false);
            go.transform.localPosition = boardParent.InverseTransformPoint(solved);
            go.transform.localScale    = Vector3.one;
        }

        /// <summary>
        /// Builds a stub linear-chain level: piece 0 is the seed, each subsequent
        /// piece neighbors the previous one. Replaced by JigsawLevelFactory in S04.
        /// </summary>
        private static IPuzzleLevel BuildStubLevel(int totalPieces)
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
            var deck = new Deck(deckOrder);

            return new PuzzleLevel(pieces, seeds, new IDeck[] { deck });
        }
    }
}
