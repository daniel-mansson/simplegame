using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
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

        private IInGameView _viewOverride;
        private ILevelCompleteView _levelCompleteViewOverride;
        private ILevelFailedView _levelFailedViewOverride;

        private IInGameView ActiveView => _viewOverride != null ? _viewOverride : _inGameView;

        private ILevelCompleteView ActiveLevelCompleteView
        {
            get
            {
                if (_levelCompleteViewOverride != null) return _levelCompleteViewOverride;
                var found = FindFirstObjectByType<LevelCompleteView>(FindObjectsInactive.Include);
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
                var found = FindFirstObjectByType<LevelFailedView>(FindObjectsInactive.Include);
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
        private PopupManager<PopupId> _popupManager;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, ProgressionService progression,
                               GameSessionService session, PopupManager<PopupId> popupManager,
                               IGoldenPieceService goldenPieces = null, IHeartService hearts = null)
        {
            _uiFactory = uiFactory;
            _progression = progression;
            _session = session;
            _popupManager = popupManager;
            _goldenPieces = goldenPieces;
            _hearts = hearts;
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

            while (true)
            {
                var presenter = _uiFactory.CreateInGamePresenter(ActiveView, _session.TotalPieces);
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
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.LevelFailed, ct);
                var choice = await presenter.WaitForChoice();
                if (choice == LevelFailedChoice.Retry || choice == LevelFailedChoice.WatchAd)
                {
                    await _popupManager.DismissPopupAsync(ct);
                }
                return choice;
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
    }
}
