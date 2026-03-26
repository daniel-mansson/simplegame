using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface IConfirmDialogView : IPopupView
    {
        event Action OnConfirmClicked;
        event Action OnCancelClicked;
        void UpdateMessage(string text);
    }
}
