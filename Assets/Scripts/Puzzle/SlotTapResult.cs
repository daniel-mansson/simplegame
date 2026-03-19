namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Result of a <see cref="PuzzleModel.TryPlace"/> call.
    /// </summary>
    public enum SlotTapResult
    {
        /// <summary>
        /// The piece in the tapped slot was legally placed on the board.
        /// The slot has been refilled from the deck (or is now empty if the deck is exhausted).
        /// </summary>
        Placed,

        /// <summary>
        /// The piece in the tapped slot cannot be placed right now — none of its
        /// neighbours are on the board yet. Slot content is unchanged.
        /// </summary>
        Rejected,

        /// <summary>
        /// The tapped slot index is out of range or the slot holds no piece (deck exhausted).
        /// No state change.
        /// </summary>
        Empty,
    }
}
