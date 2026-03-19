using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        /// <summary>
        /// Fired when the player taps a tray piece.
        /// Carries the piece ID that was tapped.
        /// </summary>
        event Action<int> OnTapPiece;

        void UpdateHearts(string text);
        void UpdatePieceCounter(string text);
        void UpdateLevelLabel(string text);

        /// <summary>
        /// Refresh the visible tray window. <paramref name="pieceIds"/> has up to 3 entries:
        /// index 0 = front (active, highlighted), index 1 = next, index 2 = one after.
        /// Null entries mean that slot is empty (deck running out).
        /// Pass an empty/null array to hide the tray entirely.
        /// </summary>
        void RefreshTray(int?[] pieceIds);

        /// <summary>
        /// Move piece from its tray position to its solved board position.
        /// Called by the presenter after a successful placement.
        /// </summary>
        void RevealPiece(int pieceId);
    }
}
