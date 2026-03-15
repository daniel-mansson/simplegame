using System;

namespace SimpleGame.Core.MVP
{
    public interface IMainMenuView : IView
    {
        event Action OnSettingsClicked;
        event Action OnPopupClicked;
        void UpdateTitle(string text);
    }
}
