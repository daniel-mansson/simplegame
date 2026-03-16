using System;
using Cysharp.Threading.Tasks;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Settings;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;

namespace SimpleGame.Game.Boot
{
    public class UIFactory
    {
        private readonly GameService _gameService;
        private readonly Action<ScreenId> _navigateCallback;
        private readonly Action<PopupId> _showPopupCallback;
        private readonly Func<UniTask> _goBackCallback;
        private readonly Func<UniTask> _dismissPopupCallback;

        public UIFactory(
            GameService gameService,
            Action<ScreenId> navigateCallback,
            Action<PopupId> showPopupCallback,
            Func<UniTask> goBackCallback,
            Func<UniTask> dismissPopupCallback)
        {
            _gameService = gameService;
            _navigateCallback = navigateCallback;
            _showPopupCallback = showPopupCallback;
            _goBackCallback = goBackCallback;
            _dismissPopupCallback = dismissPopupCallback;
        }

        // Legacy overload for backward compatibility with existing tests
        public UIFactory(GameService gameService)
            : this(gameService, _ => { }, _ => { }, () => UniTask.CompletedTask, () => UniTask.CompletedTask)
        {
        }

        public MainMenuPresenter CreateMainMenuPresenter(IMainMenuView view)
        {
            return new MainMenuPresenter(view, _navigateCallback, _showPopupCallback);
        }

        public SettingsPresenter CreateSettingsPresenter(ISettingsView view)
        {
            return new SettingsPresenter(view, _goBackCallback);
        }

        public ConfirmDialogPresenter CreateConfirmDialogPresenter(IConfirmDialogView view)
        {
            return new ConfirmDialogPresenter(view, _dismissPopupCallback);
        }
    }
}
