using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        event Action OnPlaceCorrect;
        event Action OnPlaceIncorrect;
        void UpdateHearts(string text);
        void UpdatePieceCounter(string text);
        void UpdateLevelLabel(string text);
    }
}
