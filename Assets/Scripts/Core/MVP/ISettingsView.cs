using System;

namespace SimpleGame.Core.MVP
{
    public interface ISettingsView : IView
    {
        event Action OnBackClicked;
        void UpdateTitle(string text);
    }
}
