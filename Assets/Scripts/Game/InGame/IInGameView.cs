using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.InGame
{
    public interface IInGameView : IView
    {
        /// <summary>
        /// Fired when the player taps a piece in a tray slot.
        /// Carries the piece ID that was tapped (bridge until view moves to slot-tap API).
        /// </summary>
        event Action<int> OnTapPiece;

        void UpdateHearts(string text);
        void UpdatePieceCounter(string text);
        void UpdateLevelLabel(string text);

        /// <summary>
        /// Update a single tray slot with a new piece ID (or null to show the slot as empty).
        /// Called by the presenter after any slot content change.
        /// </summary>
        void RefreshSlot(int slotIndex, int? pieceId);

        /// <summary>
        /// Move piece from its tray position to its solved board position.
        /// Called by the presenter after a successful placement.
        /// </summary>
        void RevealPiece(int pieceId);
        /// <summary>
        /// Returns the current slot contents array (read-only mirror for the controller).
        /// Null entries mean the slot is empty.
        /// </summary>
        int?[] GetSlotContents();

        /// <summary>
        /// Shake the piece at the given slot index to signal an incorrect tap.
        /// </summary>
        void ShakePiece(int slotIndex);
    }
}
