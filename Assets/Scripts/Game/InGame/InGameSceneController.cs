using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// SceneController for the InGame scene. Owns the InGamePresenter lifetime.
    /// RunAsync reads level context from GameSessionService, runs the gameplay
    /// loop (score + win/lose), and returns ScreenId for navigation.
    ///
    /// Win: calls ProgressionService.RegisterWin, sets outcome, returns MainMenu.
    /// Lose: sets outcome, returns MainMenu. (Popup integration added in S04.)
    ///
    /// Play-from-editor: when GameSessionService has no level set (CurrentLevelId == 0),
    /// the serialized _defaultLevelId is used as fallback.
    /// </summary>
    public class InGameSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private InGameView _inGameView;
        [SerializeField] private int _defaultLevelId = 1;

        private IInGameView _viewOverride;

        private IInGameView ActiveView => _viewOverride != null ? _viewOverride : _inGameView;

        private UIFactory _uiFactory;
        private ProgressionService _progression;
        private GameSessionService _session;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, ProgressionService progression, GameSessionService session)
        {
            _uiFactory = uiFactory;
            _progression = progression;
            _session = session;
        }

        /// <summary>
        /// For editor / test use: supply a mock view that overrides the serialized field.
        /// </summary>
        public void SetViewForTesting(IInGameView view)
        {
            _viewOverride = view;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            // Play-from-editor fallback: if no level was set via session, use default.
            if (_session.CurrentLevelId == 0)
            {
                _session.ResetForNewGame(_defaultLevelId);
            }

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
                        return ScreenId.MainMenu;
                    }

                    if (action == InGameAction.Lose)
                    {
                        _session.Outcome = GameOutcome.Lose;
                        return ScreenId.MainMenu;
                    }
                }
            }
            finally
            {
                presenter.Dispose();
            }
        }
    }
}
