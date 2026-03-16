using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Tracks meta-game progression — the player's current level.
    /// In-memory only; resets when the service is recreated (app restart).
    ///
    /// When the player wins, call <see cref="RegisterWin"/> with the achieved
    /// score. The service logs the result and advances the level counter.
    /// </summary>
    public class ProgressionService
    {
        private int _currentLevel;

        /// <summary>The player's current level. Starts at 1.</summary>
        public int CurrentLevel => _currentLevel;

        public ProgressionService()
        {
            _currentLevel = 1;
        }

        /// <summary>
        /// Records a win. Logs the score and level, then advances to the next level.
        /// </summary>
        /// <param name="score">The score achieved during the completed level.</param>
        public void RegisterWin(int score)
        {
            Debug.Log($"[ProgressionService] Level {_currentLevel} complete — score: {score}");
            _currentLevel++;
        }
    }
}
