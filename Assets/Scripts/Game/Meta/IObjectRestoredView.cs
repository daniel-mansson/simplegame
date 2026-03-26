using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface IObjectRestoredView : IPopupView
    {
        event Action OnContinueClicked;
        void UpdateObjectName(string text);
    }
}
