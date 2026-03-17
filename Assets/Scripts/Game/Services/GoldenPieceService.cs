using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Manages golden puzzle piece currency. Reads initial balance from
    /// <see cref="IMetaSaveService"/> on construction and writes back on
    /// <see cref="Save"/>. Earn adds pieces, TrySpend deducts if sufficient.
    /// </summary>
    public class GoldenPieceService : IGoldenPieceService
    {
        private readonly IMetaSaveService _saveService;
        private MetaSaveData _saveData;

        public GoldenPieceService(IMetaSaveService saveService)
        {
            _saveService = saveService;
            _saveData = _saveService.Load();
        }

        /// <inheritdoc/>
        public int Balance => _saveData.goldenPieces;

        /// <inheritdoc/>
        public void Earn(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[GoldenPieceService] Earn called with non-positive amount: {amount}");
                return;
            }

            _saveData.goldenPieces += amount;
            Debug.Log($"[GoldenPieceService] Earned {amount} golden pieces. Balance: {_saveData.goldenPieces}");
        }

        /// <inheritdoc/>
        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[GoldenPieceService] TrySpend called with non-positive amount: {amount}");
                return false;
            }

            if (_saveData.goldenPieces < amount)
            {
                Debug.Log($"[GoldenPieceService] Insufficient balance. Need {amount}, have {_saveData.goldenPieces}.");
                return false;
            }

            _saveData.goldenPieces -= amount;
            Debug.Log($"[GoldenPieceService] Spent {amount} golden pieces. Balance: {_saveData.goldenPieces}");
            return true;
        }

        /// <inheritdoc/>
        public void Save()
        {
            // Reload the latest save data to pick up changes made by other
            // services (e.g. MetaProgressionService writing objectProgress),
            // then apply our golden piece balance before persisting.
            var latest = _saveService.Load();
            latest.goldenPieces = _saveData.goldenPieces;
            _saveService.Save(latest);
            _saveData = latest;
        }

        /// <inheritdoc/>
        public void ResetAll()
        {
            _saveData.goldenPieces = 0;
            Save();
            Debug.Log("[GoldenPieceService] Balance reset to 0.");
        }
    }
}
