using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface ILoseDialogView : IPopupView
    {
        event Action OnRetryClicked;
        event Action OnBackClicked;
        void UpdateScore(string text);
        void UpdateLevel(string text);
    }
}
