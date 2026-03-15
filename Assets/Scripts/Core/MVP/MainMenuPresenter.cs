using System;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;

namespace SimpleGame.Core.MVP
{
    public class MainMenuPresenter : Presenter<IMainMenuView>
    {
        private readonly Action<ScreenId> _navigateCallback;
        private readonly Action<PopupId> _showPopupCallback;

        public MainMenuPresenter(
            IMainMenuView view,
            Action<ScreenId> navigateCallback,
            Action<PopupId> showPopupCallback) : base(view)
        {
            _navigateCallback = navigateCallback;
            _showPopupCallback = showPopupCallback;
        }

        public override void Initialize()
        {
            View.OnSettingsClicked += HandleSettingsClicked;
            View.OnPopupClicked += HandlePopupClicked;
            View.UpdateTitle("Main Menu");
        }

        public override void Dispose()
        {
            View.OnSettingsClicked -= HandleSettingsClicked;
            View.OnPopupClicked -= HandlePopupClicked;
        }

        private void HandleSettingsClicked()
        {
            _navigateCallback(ScreenId.Settings);
        }

        private void HandlePopupClicked()
        {
            _showPopupCallback(PopupId.ConfirmDialog);
        }
    }
}
