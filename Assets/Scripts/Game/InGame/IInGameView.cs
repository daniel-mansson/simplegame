using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        /// <summary>
        /// Fired when the player taps a piece. Carries the piece ID.
        /// The presenter validates placement via PuzzleSession — the view has no knowledge
        /// of whether the tap was correct or incorrect.
        /// </summary>
        event Action<int> OnTapPiece;

        void UpdateHearts(string text);
        void UpdatePieceCounter(string text);
        void UpdateLevelLabel(string text);
    }
}
