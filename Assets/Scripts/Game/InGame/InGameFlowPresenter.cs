using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.TransitionManagement;
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
    /// Pure C# presenter that owns the InGame scene's gameplay loop:
    /// model-factory execution, presenter lifecycle, win/lose/retry/ad popup
    /// orchestration, analytics calls, and interstitial scheduling.
    ///
    /// Receives all service dependencies via constructor and a reference to
    /// <see cref="PuzzleStageController"/> for retry reset and transition.
    ///
    /// <para>Call <see cref="RunAsync"/> once per scene entry. It loops internally
    /// until navigation away is decided, then returns the target <see cref="ScreenId"/>.</para>
    /// </summary>
    public class InGameFlowPresenter
    {
        // ── Services ──────────────────────────────────────────────────────
        private readonly UIFactory _uiFactory;
        private readonly ProgressionService _progression;
        private readonly GameSessionService _session;
        private readonly PopupManager<PopupId> _popupManager;
        private readonly IGoldenPieceService _goldenPieces;
        private readonly IHeartService _hearts;
        private readonly ICoinsService _coins;
        private readonly IViewResolver _viewResolver;
        private readonly ICurrencyOverlay _overlay;
        private readonly System.Func<UniTask> _onSessionEnd;
        private readonly IAnalyticsService _analytics;
        private readonly IAdService _adService;
        private readonly PuzzleStageController _stage;

        // ── Config ────────────────────────────────────────────────────────
        private readonly int _defaultLevelId;
        private readonly int _defaultTotalPieces;
        private int _goldenPiecesPerWin;
        private int _continueCostCoins;
        private int _interstitialEveryNLevels;

        // ── Test seams ────────────────────────────────────────────────────
        private float _winPopupDelaySec = 1.0f;
        private int _levelsCompletedThisSession = 0;

        private System.Func<PuzzleModel> _modelFactory;
        private (int rows, int cols, int slots)? _debugOverride;

        // ── View overrides ────────────────────────────────────────────────
        private IInGameView _viewOverride;
        private ILevelCompleteView _levelCompleteViewOverride;
        private ILevelFailedView _levelFailedViewOverride;
        private readonly IInGameView _serializedView;

        private IInGameView ActiveView => _viewOverride ?? _serializedView;

        private ILevelCompleteView ActiveLevelCompleteView
        {
            get
            {
                if (_levelCompleteViewOverride != null) return _levelCompleteViewOverride;
                var found = _viewResolver?.Get<ILevelCompleteView>();
                if (found == null)
                    Debug.LogError("[InGameFlowPresenter] LevelCompleteView not found in any loaded scene.");
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
                    Debug.LogError("[InGameFlowPresenter] LevelFailedView not found in any loaded scene.");
                return found;
            }
        }

        // ── Constructor ───────────────────────────────────────────────────

        public InGameFlowPresenter(
            IInGameView serializedView,
            PuzzleStageController stage,
            UIFactory uiFactory,
            ProgressionService progression,
            GameSessionService session,
            PopupManager<PopupId> popupManager,
            IGoldenPieceService goldenPieces = null,
            IHeartService hearts = null,
            ICoinsService coins = null,
            IViewResolver viewResolver = null,
            ICurrencyOverlay overlay = null,
            System.Func<UniTask> onSessionEnd = null,
            IAnalyticsService analytics = null,
            IAdService adService = null,
            int defaultLevelId = 1,
            int defaultTotalPieces = 10,
            int goldenPiecesPerWin = 5,
            int continueCostCoins = 100,
            int interstitialEveryNLevels = 3)
        {
            _serializedView           = serializedView;
            _stage                    = stage;
            _uiFactory                = uiFactory;
            _progression              = progression;
            _session                  = session;
            _popupManager             = popupManager;
            _goldenPieces             = goldenPieces;
            _hearts                   = hearts;
            _coins                    = coins;
            _viewResolver             = viewResolver;
            _overlay                  = overlay;
            _onSessionEnd             = onSessionEnd;
            _analytics                = analytics;
            _adService                = adService;
            _defaultLevelId           = defaultLevelId;
            _defaultTotalPieces       = defaultTotalPieces;
            _goldenPiecesPerWin       = goldenPiecesPerWin;
            _continueCostCoins        = continueCostCoins;
            _interstitialEveryNLevels = interstitialEveryNLevels;
        }

        // ── Test seam API ─────────────────────────────────────────────────

        public void SetViewsForTesting(IInGameView inGameView,
                                       ILevelCompleteView levelCompleteView = null,
                                       ILevelFailedView levelFailedView = null)
        {
            _viewOverride              = inGameView;
            _levelCompleteViewOverride = levelCompleteView;
            _levelFailedViewOverride   = levelFailedView;
            _winPopupDelaySec          = 0f;
        }

        public void SetModelFactory(System.Func<PuzzleModel> factory) => _modelFactory = factory;

        public void SetWinPopupDelay(float seconds) => _winPopupDelaySec = seconds;

        public void SetDebugOverride(int rows, int cols, int slots) => _debugOverride = (rows, cols, slots);

        public void ClearDebugOverride() => _debugOverride = null;

        public void ApplyRemoteConfig(GameRemoteConfig config)
        {
            _goldenPiecesPerWin       = config.GoldenPiecesPerWin;
            _continueCostCoins        = config.ContinueCostCoins;
            _interstitialEveryNLevels = config.InterstitialEveryNLevels;
            _uiFactory.SetInitialHearts(config.InitialHearts);
        }

        // ── Main game loop ────────────────────────────────────────────────

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            _stage?.Reset();

            if (_session.CurrentLevelId == 0)
                _session.ResetForNewGame(_defaultLevelId, _defaultTotalPieces);

            int slotCount = _debugOverride.HasValue
                ? Mathf.Max(1, _debugOverride.Value.slots)
                : Mathf.Clamp(_session.CurrentLevelId / 3 + 3, 1, 5);

            System.Func<PuzzleModel> modelFactory;
            if (_modelFactory != null)
            {
                modelFactory = _modelFactory;
            }
            else if (_stage != null && _stage.HasGridLayoutConfig)
            {
                var gridSize = _debugOverride.HasValue
                    ? new LevelProgression.GridSize(_debugOverride.Value.rows, _debugOverride.Value.cols)
                    : LevelProgression.GetGridSize(_session.CurrentLevelId);
                var runtimeConfig = _stage.CreateRuntimeGridConfig(gridSize.Rows, gridSize.Cols);

                modelFactory = () =>
                {
                    int initialSeed = Random.Range(int.MinValue, int.MaxValue);
                    var result = JigsawLevelFactory.BuildSolvable(runtimeConfig, slotCount, initialSeed);
                    Debug.Log($"[InGameFlowPresenter] Level {_session.CurrentLevelId} seed={result.Seed} grid={gridSize.Rows}x{gridSize.Cols} slots={slotCount}");
                    if (_session.TotalPieces != result.PieceList.Count)
                        _session.ResetForNewGame(_session.CurrentLevelId, result.PieceList.Count);
                    _stage.SpawnLevel(result.RawBoard, result.SeedIds[0], slotCount, result.DeckOrder, gridSize.Cols);
                    return new PuzzleModel(result.PieceList, result.SeedIds, result.DeckOrder, slotCount);
                };
            }
            else
            {
                modelFactory = () => BuildStubModel(_session.TotalPieces, slotCount);
            }

            while (true)
            {
                var model     = modelFactory();
                var presenter = _uiFactory.CreateInGamePresenter(ActiveView, model);
                presenter.Initialize();
                _analytics?.TrackLevelStarted(_session.CurrentLevelId.ToString());
                try
                {
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
                            if (_winPopupDelaySec > 0f)
                                await UniTask.Delay(System.TimeSpan.FromSeconds(_winPopupDelaySec), cancellationToken: ct);
                            await HandleLevelCompletePopupAsync(ct);
                            _levelsCompletedThisSession++;
                            if (_interstitialEveryNLevels > 0 && _levelsCompletedThisSession % _interstitialEveryNLevels == 0)
                                Debug.Log($"[InGameFlowPresenter] Interstitial result: {await HandleInterstitialAsync(ct)}");
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
                                if (adWatched) { presenter.RestoreHeartsAndContinue(); continue; }
                                _session.CurrentScore = 0;
                                await RetryTransitionAsync(ct);
                                break;
                            }
                            if (choice == LevelFailedChoice.Continue)
                            {
                                presenter.RestoreHeartsAndContinue();
                                continue;
                            }
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

        // ── Popup helpers ─────────────────────────────────────────────────

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
            finally { presenter.Dispose(); }
        }

        private async UniTask<LevelFailedChoice> HandleLevelFailedPopupAsync(CancellationToken ct)
        {
            var view = ActiveLevelFailedView;
            if (view == null) return LevelFailedChoice.Quit;
            var presenter = _uiFactory.CreateLevelFailedPresenter(view);
            presenter.Initialize(_session.CurrentScore, _session.CurrentLevelId);

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
                        await HandleShopPopupAsync(ct);
                        _overlay?.UpdateBalance($"Coins: {_coins?.Balance ?? 0}");
                    }
                }
            }
            finally
            {
                presenter.Dispose();
                if (_overlay != null) _overlay.HideAsync(ct).Forget();
            }
        }

        private async UniTask HandleShopPopupAsync(CancellationToken ct)
        {
            var view = _viewResolver?.Get<IShopView>();
            if (view == null) { Debug.LogWarning("[InGameFlowPresenter] ShopView not found."); return; }
            var presenter = _uiFactory.CreateShopPresenter(view);
            presenter.Initialize();
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.Shop, ct);
                await presenter.WaitForResult();
                await _popupManager.DismissPopupAsync(ct);
            }
            finally { presenter.Dispose(); }
        }

        private async UniTask<AdResult> HandleInterstitialAsync(CancellationToken ct)
        {
            if (_adService == null || !_adService.IsInterstitialLoaded) return AdResult.NotLoaded;
            try { return await _adService.ShowInterstitialAsync(ct); }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[InGameFlowPresenter] Interstitial show exception: {ex.Message}");
                return AdResult.Failed;
            }
        }

        private async UniTask<bool> HandleRewardedAdAsync(CancellationToken ct)
        {
            var view = _viewResolver?.Get<IRewardedAdView>();
            if (view == null) { Debug.LogWarning("[InGameFlowPresenter] IRewardedAdView not found."); return false; }
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
            finally { presenter.Dispose(); }
        }

        private async UniTask RetryTransitionAsync(CancellationToken ct)
        {
            var tp = _stage?.GetTransitionPlayer();
            if (tp != null) await tp.FadeOutAsync(ct);
            if (tp != null) await tp.FadeInAsync(ct);
        }

        // ── Stub model ────────────────────────────────────────────────────

        private static PuzzleModel BuildStubModel(int totalPieces, int slotCount)
        {
            if (totalPieces <= 0) totalPieces = 1;
            var pieces = new List<IPuzzlePiece>(totalPieces);
            for (int i = 0; i < totalPieces; i++)
            {
                var neighbors = new List<int>();
                if (i > 0) neighbors.Add(i - 1);
                if (i < totalPieces - 1) neighbors.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, neighbors));
            }
            var seeds = new[] { 0 };
            var deckOrder = new int[totalPieces - 1];
            for (int i = 0; i < deckOrder.Length; i++) deckOrder[i] = i + 1;
            return new PuzzleModel(pieces, seeds, deckOrder, slotCount);
        }
    }
}
