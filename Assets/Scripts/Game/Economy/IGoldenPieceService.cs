namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for managing golden puzzle piece currency.
    /// Handles earning, spending, and balance queries.
    /// Persistence is delegated to the underlying save service.
    /// </summary>
    public interface IGoldenPieceService
    {
        /// <summary>Current golden piece balance.</summary>
        int Balance { get; }

        /// <summary>Add golden pieces to the balance.</summary>
        /// <param name="amount">Must be positive.</param>
        void Earn(int amount);

        /// <summary>
        /// Attempt to spend golden pieces.
        /// Fails if the balance is insufficient.
        /// </summary>
        /// <param name="amount">Must be positive.</param>
        /// <returns>true if the spend succeeded, false if insufficient balance.</returns>
        bool TrySpend(int amount);

        /// <summary>Persist the current balance to the save service.</summary>
        void Save();

        /// <summary>Reset balance to zero and persist (for debug/reset).</summary>
        void ResetAll();
    }
}
