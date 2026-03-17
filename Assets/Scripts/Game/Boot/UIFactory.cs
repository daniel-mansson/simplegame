using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;

namespace SimpleGame.Game.Boot
{
    public class UIFactory
    {
        private readonly GameService _gameService;
        private readonly ProgressionService _progression;
        private readonly GameSessionService _session;
        private readonly IHeartService _hearts;
        private readonly MetaProgressionService _metaProgression;
        private readonly IGoldenPieceService _goldenPieces;

        public UIFactory(GameService gameService, ProgressionService progression,
                         GameSessionService session, IHeartService hearts = null,
                         MetaProgressionService metaProgression = null,
                         IGoldenPieceService goldenPieces = null)
        {
            _gameService = gameService;
            _progression = progression;
            _session = session;
            _hearts = hearts;
            _metaProgression = metaProgression;
            _goldenPieces = goldenPieces;
        }

        public MainMenuPresenter CreateMainMenuPresenter(IMainMenuView view, EnvironmentData currentEnvironment,
                                                          bool hasNextEnvironment = false)
        {
            return new MainMenuPresenter(view, _metaProgression, _goldenPieces, _progression, _session,
                                         currentEnvironment, hasNextEnvironment);
        }

        public SettingsPresenter CreateSettingsPresenter(ISettingsView view)
        {
            return new SettingsPresenter(view);
        }

        public ConfirmDialogPresenter CreateConfirmDialogPresenter(IConfirmDialogView view)
        {
            return new ConfirmDialogPresenter(view);
        }

        public InGamePresenter CreateInGamePresenter(IInGameView view, int totalPieces)
        {
            return new InGamePresenter(view, _session, _hearts, totalPieces);
        }

        public LevelCompletePresenter CreateLevelCompletePresenter(ILevelCompleteView view)
        {
            return new LevelCompletePresenter(view);
        }

        public LevelFailedPresenter CreateLevelFailedPresenter(ILevelFailedView view)
        {
            return new LevelFailedPresenter(view);
        }

        public ObjectRestoredPresenter CreateObjectRestoredPresenter(IObjectRestoredView view)
        {
            return new ObjectRestoredPresenter(view);
        }
    }
}
