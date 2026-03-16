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
    /// loop (score + win/lose), shows outcome popups, and returns ScreenId for navigation.
    ///
    /// Win: calls ProgressionService.RegisterWin, shows WinDialog popup, returns MainMenu.
    /// Lose: shows LoseDialog popup — Retry resets score and loops, Back returns MainMenu.
    ///
    /// Play-from-editor: when GameSessionService has no level set (CurrentLevelId == 0),
    /// the serialized _defaultLevelId is used as fallback.
    /// </summary>
    public class InGameSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private InGameView _inGameView;
        [SerializeField] private int _defaultLevelId = 1;

        private IInGameView _viewOverride;
        private IWinDialogView _winDialogViewOverride;
        private ILoseDialogView _loseDialogViewOverride;

        private IInGameView ActiveView => _viewOverride != null ? _viewOverride : _inGameView;

        private IWinDialogView ActiveWinDialogView
        {
            get
            {
                if (_winDialogViewOverride != null) return _winDialogViewOverride;
                var found = FindFirstObjectByType<WinDialogView>(FindObjectsInactive.Include);
                if (found == null)
                    Debug.LogError("[InGameSceneController] WinDialogView not found in any loaded scene.");
                return found;
            }
        }

        private ILoseDialogView ActiveLoseDialogView
        {
            get
            {
                if (_loseDialogViewOverride != null) return _loseDialogViewOverride;
                var found = FindFirstObjectByType<LoseDialogView>(FindObjectsInactive.Include);
                if (found == null)
                    Debug.LogError("[InGameSceneController] LoseDialogView not found in any loaded scene.");
                return found;
            }
        }

        private UIFactory _uiFactory;
        private ProgressionService _progression;
        private GameSessionService _session;
        private PopupManager<PopupId> _popupManager;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, ProgressionService progression,
                               GameSessionService session, PopupManager<PopupId> popupManager)
        {
            _uiFactory = uiFactory;
            _progression = progression;
            _session = session;
            _popupManager = popupManager;
        }

        /// <summary>
        /// For editor / test use: supply mock views that override the serialized fields.
        /// </summary>
        public void SetViewsForTesting(IInGameView inGameView,
                                        IWinDialogView winDialogView = null,
                                        ILoseDialogView loseDialogView = null)
        {
            _viewOverride = inGameView;
            _winDialogViewOverride = winDialogView;
            _loseDialogViewOverride = loseDialogView;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            // Play-from-editor fallback: if no level was set via session, use default.
            if (_session.CurrentLevelId == 0)
            {
                _session.ResetForNewGame(_defaultLevelId);
            }

            while (true)
            {
                var presenter = _uiFactory.CreateInGamePresenter(ActiveView);
                presenter.Initialize();
                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        var action = await presenter.WaitForAction();

                        if (action == InGameAction.Win)
                        {
                            _progression.RegisterWin(_session.CurrentScore);
                            _session.Outcome = GameOutcome.Win;
                            await HandleWinPopupAsync(ct);
                            return ScreenId.MainMenu;
                        }

                        if (action == InGameAction.Lose)
                        {
                            _session.Outcome = GameOutcome.Lose;
                            var choice = await HandleLosePopupAsync(ct);
                            if (choice == LoseDialogChoice.Back)
                                return ScreenId.MainMenu;

                            // Retry: reset score, break inner loop to create fresh presenter
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

        private async UniTask HandleWinPopupAsync(CancellationToken ct)
        {
            var winView = ActiveWinDialogView;
            if (winView == null) return;

            var winPresenter = _uiFactory.CreateWinDialogPresenter(winView);
            winPresenter.Initialize(_session.CurrentScore, _session.CurrentLevelId);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.WinDialog, ct);
                await winPresenter.WaitForContinue();
                // Popup stays open — ScreenManager dismisses it behind the transition
            }
            finally
            {
                winPresenter.Dispose();
            }
        }

        private async UniTask<LoseDialogChoice> HandleLosePopupAsync(CancellationToken ct)
        {
            var loseView = ActiveLoseDialogView;
            if (loseView == null) return LoseDialogChoice.Back;

            var losePresenter = _uiFactory.CreateLoseDialogPresenter(loseView);
            losePresenter.Initialize(_session.CurrentScore, _session.CurrentLevelId);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.LoseDialog, ct);
                var choice = await losePresenter.WaitForChoice();
                if (choice == LoseDialogChoice.Retry)
                {
                    // Retry stays in-scene — dismiss popup normally
                    await _popupManager.DismissPopupAsync(ct);
                }
                // Back leaves popup open — ScreenManager dismisses it behind the transition
                return choice;
            }
            finally
            {
                losePresenter.Dispose();
            }
        }
    }
}
