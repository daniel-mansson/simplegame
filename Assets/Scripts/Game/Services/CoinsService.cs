using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Manages the coins currency. Reads initial balance from
    /// <see cref="IMetaSaveService"/> on construction and writes back on
    /// <see cref="Save"/>. Earn adds coins, TrySpend deducts if sufficient.
    ///
    /// Follows the same pattern as <see cref="GoldenPieceService"/>.
    /// </summary>
    public class CoinsService : ICoinsService
    {
        private readonly IMetaSaveService _saveService;
        private MetaSaveData _saveData;

        public CoinsService(IMetaSaveService saveService)
        {
            _saveService = saveService;
            _saveData = _saveService.Load();
        }

        /// <inheritdoc/>
        public int Balance => _saveData.coins;

        /// <inheritdoc/>
        public void Earn(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CoinsService] Earn called with non-positive amount: {amount}");
                return;
            }

            _saveData.coins += amount;
            Debug.Log($"[CoinsService] Earned {amount} coins. Balance: {_saveData.coins}");
        }

        /// <inheritdoc/>
        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CoinsService] TrySpend called with non-positive amount: {amount}");
                return false;
            }

            if (_saveData.coins < amount)
            {
                Debug.Log($"[CoinsService] Insufficient balance. Need {amount}, have {_saveData.coins}.");
                return false;
            }

            _saveData.coins -= amount;
            Debug.Log($"[CoinsService] Spent {amount} coins. Balance: {_saveData.coins}");
            return true;
        }

        /// <inheritdoc/>
        public void Save()
        {
            // Reload the latest save data to pick up changes made by other services,
            // then apply our coin balance before persisting.
            var latest = _saveService.Load();
            latest.coins = _saveData.coins;
            _saveService.Save(latest);
            _saveData = latest;
        }

        /// <inheritdoc/>
        public void ResetAll()
        {
            _saveData.coins = 0;
            Save();
            Debug.Log("[CoinsService] Balance reset to 0.");
        }
    }
}
