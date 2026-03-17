namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for per-level heart management.
    /// Hearts represent remaining mistakes allowed during gameplay.
    /// </summary>
    public interface IHeartService
    {
        /// <summary>Number of hearts remaining.</summary>
        int RemainingHearts { get; }

        /// <summary>Whether the player still has hearts (remaining > 0).</summary>
        bool IsAlive { get; }

        /// <summary>
        /// Reset hearts to the given count. Call at the start of each level.
        /// </summary>
        /// <param name="count">Number of hearts to start with. Must be positive.</param>
        void Reset(int count);

        /// <summary>
        /// Use one heart. Fails if no hearts remain.
        /// </summary>
        /// <returns>true if a heart was consumed, false if already at 0.</returns>
        bool UseHeart();
    }
}
