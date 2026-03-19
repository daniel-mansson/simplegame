using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        /// <summary>
        /// Fired when the player taps the current tray piece button.
        /// Carries the piece ID of the front deck piece.
        /// </summary>
        event Action<int> OnTapPiece;

        void UpdateHearts(string text);
        void UpdatePieceCounter(string text);
        void UpdateLevelLabel(string text);

        /// <summary>
        /// Show the tray piece button for the given piece ID.
        /// Called by the presenter each time the deck advances.
        /// </summary>
        void ShowDeckPiece(int pieceId);

        /// <summary>Hide the tray panel (deck exhausted).</summary>
        void HideDeckPanel();

        /// <summary>
        /// Move piece from tray to its solved board position.
        /// Called by the presenter after a successful placement.
        /// </summary>
        void RevealPiece(int pieceId);
    }
}
