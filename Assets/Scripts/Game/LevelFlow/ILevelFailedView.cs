using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface ILevelFailedView : IPopupView
    {
        event Action OnRetryClicked;
        event Action OnWatchAdClicked;
        event Action OnQuitClicked;
        event Action OnContinueClicked;
        void UpdateScore(string text);
        void UpdateLevel(string text);
        void UpdateContinueCost(string text);
    }
}
