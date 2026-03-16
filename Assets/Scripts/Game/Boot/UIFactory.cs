using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;

namespace SimpleGame.Game.Boot
{
    public class UIFactory
    {
        private readonly GameService _gameService;

        public UIFactory(GameService gameService)
        {
            _gameService = gameService;
        }

        public MainMenuPresenter CreateMainMenuPresenter(IMainMenuView view)
        {
            return new MainMenuPresenter(view);
        }

        public SettingsPresenter CreateSettingsPresenter(ISettingsView view)
        {
            return new SettingsPresenter(view);
        }

        public ConfirmDialogPresenter CreateConfirmDialogPresenter(IConfirmDialogView view)
        {
            return new ConfirmDialogPresenter(view);
        }
    }
}
