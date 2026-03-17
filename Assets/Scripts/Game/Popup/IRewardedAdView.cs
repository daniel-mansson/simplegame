using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface IRewardedAdView : IPopupView
    {
        event Action OnWatchClicked;
        event Action OnSkipClicked;
        void UpdateStatus(string text);
    }
}
