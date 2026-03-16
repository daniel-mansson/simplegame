using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.MainMenu
{
    public interface IMainMenuView : IView
    {
        event Action OnSettingsClicked;
        event Action OnPopupClicked;
        void UpdateTitle(string text);
    }
}
