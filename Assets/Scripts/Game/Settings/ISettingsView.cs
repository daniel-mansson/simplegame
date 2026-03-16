using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Settings
{
    public interface ISettingsView : IView
    {
        event Action OnBackClicked;
        void UpdateTitle(string text);
    }
}
