using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Puzzle;
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

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            // Play-from-editor fallback: if no level was set via session, use defaults.
            if (_session.CurrentLevelId == 0)
            {
                _session.ResetForNewGame(_defaultLevelId, _defaultTotalPieces);
            }

            // Determine the level factory: use injected factory, or fall back to stub.
            // S04 replaces the stub factory with JigsawLevelFactory.Build.
            var factory = _levelFactory ?? (() => BuildStubLevel(_session.TotalPieces));

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
