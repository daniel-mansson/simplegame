using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
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

        public UIFactory(GameService gameService, ProgressionService progression, GameSessionService session)
        {
            _gameService = gameService;
            _progression = progression;
            _session = session;
        }

        public MainMenuPresenter CreateMainMenuPresenter(IMainMenuView view)
        {
            return new MainMenuPresenter(view, _progression, _session);
        }

        public SettingsPresenter CreateSettingsPresenter(ISettingsView view)
        {
            return new SettingsPresenter(view);
        }

        public ConfirmDialogPresenter CreateConfirmDialogPresenter(IConfirmDialogView view)
        {
            return new ConfirmDialogPresenter(view);
        }

        public InGamePresenter CreateInGamePresenter(IInGameView view)
        {
            return new InGamePresenter(view, _session);
        }

        public WinDialogPresenter CreateWinDialogPresenter(IWinDialogView view)
        {
            return new WinDialogPresenter(view);
        }

        public LoseDialogPresenter CreateLoseDialogPresenter(ILoseDialogView view)
        {
            return new LoseDialogPresenter(view);
        }
    }
}
