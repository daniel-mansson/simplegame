using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface IIAPPurchaseView : IPopupView
    {
        event Action OnPurchaseClicked;
        event Action OnCancelClicked;
        void UpdateItemName(string text);
        void UpdatePrice(string text);
        void UpdateStatus(string text);
    }
}
