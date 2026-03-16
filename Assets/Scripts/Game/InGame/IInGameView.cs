using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        event Action OnScoreClicked;
        event Action OnWinClicked;
        event Action OnLoseClicked;
        void UpdateScore(string text);
        void UpdateLevelLabel(string text);
    }
}
