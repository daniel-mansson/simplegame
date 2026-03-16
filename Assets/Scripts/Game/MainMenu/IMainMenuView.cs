using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.MainMenu
{
    public interface IMainMenuView : IView
    {
        event Action OnSettingsClicked;
        event Action OnPopupClicked;
        event Action OnPlayClicked;
        void UpdateTitle(string text);
        void UpdateLevelDisplay(string text);
    }
}
