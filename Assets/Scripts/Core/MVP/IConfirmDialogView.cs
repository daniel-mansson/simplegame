using System;

namespace SimpleGame.Core.MVP
{
    public interface IConfirmDialogView : IPopupView
    {
        event Action OnConfirmClicked;
        event Action OnCancelClicked;
        void UpdateMessage(string text);
    }
}
