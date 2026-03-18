namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Result of a placement attempt via <see cref="PuzzleSession.TryPlace"/>.
    /// </summary>
    public enum PlacementResult
    {
        /// <summary>Piece was accepted and added to the board.</summary>
        Placed,

        /// <summary>
        /// Piece was rejected — none of its neighbors are currently on the board.
        /// </summary>
        Rejected,

        /// <summary>Piece was already placed on the board; state unchanged.</summary>
        AlreadyPlaced,
    }
}
