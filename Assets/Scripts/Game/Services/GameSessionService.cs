namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Holds the current game session context. Mediates context passing between
    /// scenes — the sender writes before navigation, the receiver reads on arrival.
    /// All state is in-memory; resets when the service is recreated (app restart).
    /// </summary>
    public class GameSessionService
    {
        /// <summary>The level ID for the current or upcoming game session.</summary>
        public int CurrentLevelId { get; set; }

        /// <summary>The accumulated score during the current game session.</summary>
        public int CurrentScore { get; set; }

        /// <summary>The outcome of the most recent game session.</summary>
        public GameOutcome Outcome { get; set; }

        /// <summary>
        /// Prepares the session for a new game. Sets the level ID and resets
        /// score and outcome to their initial values.
        /// </summary>
        public void ResetForNewGame(int levelId)
        {
            CurrentLevelId = levelId;
            CurrentScore = 0;
            Outcome = GameOutcome.None;
        }
    }
}
