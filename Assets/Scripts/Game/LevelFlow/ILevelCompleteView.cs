using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface ILevelCompleteView : IPopupView
    {
        event Action OnContinueClicked;
        void UpdateScore(string text);
        void UpdateLevel(string text);
        void UpdateGoldenPieces(string text);
    }
}
