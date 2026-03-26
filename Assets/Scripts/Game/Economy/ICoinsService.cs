namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for managing the coins currency.
    /// Coins are separate from golden pieces and are used for the Continue flow and shop purchases.
    /// Persistence is delegated to the underlying save service.
    /// </summary>
    public interface ICoinsService
    {
        /// <summary>Current coin balance.</summary>
        int Balance { get; }

        /// <summary>Add coins to the balance.</summary>
        /// <param name="amount">Must be positive.</param>
        void Earn(int amount);

        /// <summary>
        /// Attempt to spend coins.
        /// Returns false without modifying balance if the balance is insufficient.
        /// </summary>
        /// <param name="amount">Must be positive.</param>
        /// <returns>true if the spend succeeded; false if insufficient balance.</returns>
        bool TrySpend(int amount);

        /// <summary>Persist the current balance to the save service.</summary>
        void Save();

        /// <summary>Reset balance to zero and persist.</summary>
        void ResetAll();
    }
}
