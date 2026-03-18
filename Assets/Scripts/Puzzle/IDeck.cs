namespace SimpleGame.Puzzle
{
    /// <summary>
    /// An ordered sequence of piece IDs that the player draws from.
    /// The front of the deck is what the player currently sees.
    /// Advancing the deck exposes the next piece.
    /// </summary>
    public interface IDeck
    {
        /// <summary>
        /// The piece ID at the front of the deck, or null if the deck is exhausted.
        /// </summary>
        int? Peek();

        /// <summary>
        /// Advances the deck to the next piece.
        /// Returns true if there is a next piece; false if the deck is now exhausted.
        /// </summary>
        bool Advance();

        /// <summary>True when all pieces in this deck have been drawn.</summary>
        bool IsEmpty { get; }

        /// <summary>Total number of pieces in this deck.</summary>
        int Count { get; }

        /// <summary>Number of pieces not yet drawn (including the current front piece).</summary>
        int RemainingCount { get; }
    }
}
