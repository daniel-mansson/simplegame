using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Editor/test implementation of <see cref="IIAPService"/>.
    /// Returns a fixed outcome driven by an <see cref="IAPMockConfig"/> ScriptableObject.
    ///
    /// Usage in Editor: set MockOutcome on the IAPMockConfig asset at
    /// Assets/Resources/IAPMockConfig.asset to simulate any purchase outcome.
    ///
    /// Usage in tests: pass a config created via ScriptableObject.CreateInstance.
    ///
    /// Coins are NOT granted by this service — it returns CoinsGranted in the
    /// IAPResult and expects the caller (presenter) to read it. This mirrors
    /// how UnityIAPService works: it grants internally and returns the result.
    /// For consistency, MockIAPService also grants via ICoinsService if one is provided.
    /// </summary>
    public class MockIAPService : IIAPService
    {
        private readonly IAPMockConfig _config;
        private readonly ICoinsService _coins;

        public bool IsInitialized { get; private set; }

        /// <param name="config">Mock outcome configuration. Must not be null.</param>
        /// <param name="coins">Optional — if provided, Earn+Save called on Success.</param>
        public MockIAPService(IAPMockConfig config, ICoinsService coins = null)
        {
            _config = config;
            _coins = coins;
        }

        /// <inheritdoc/>
        public UniTask InitializeAsync()
        {
            IsInitialized = true;
            Debug.Log("[MockIAP] InitializeAsync — mock initialized immediately.");
            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public UniTask<IAPResult> BuyAsync(string productId)
        {
            var outcome = _config != null ? _config.MockOutcome : IAPOutcome.Success;
            var coinsGranted = (outcome == IAPOutcome.Success && _config != null)
                ? _config.CoinsGranted
                : 0;

            Debug.Log($"[MockIAP] BuyAsync({productId}) → {outcome}" +
                      (outcome == IAPOutcome.Success ? $" (+{coinsGranted} coins)" : ""));

            if (outcome == IAPOutcome.Success && coinsGranted > 0 && _coins != null)
            {
                _coins.Earn(coinsGranted);
                _coins.Save();
            }

            var result = outcome == IAPOutcome.Success
                ? IAPResult.Succeeded(coinsGranted)
                : IAPResult.Failed(outcome);

            return UniTask.FromResult(result);
        }
    }
}
