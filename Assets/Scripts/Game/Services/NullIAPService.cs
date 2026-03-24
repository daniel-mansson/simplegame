using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// No-op implementation of <see cref="IIAPService"/> for contexts where
    /// IAP is not applicable (e.g. InGame scene standalone launch, test scaffolds).
    /// BuyAsync always returns <see cref="IAPOutcome.PaymentFailed"/> with a warning.
    /// </summary>
    public class NullIAPService : IIAPService
    {
        public bool IsInitialized => false;

        public UniTask InitializeAsync()
        {
            Debug.LogWarning("[NullIAPService] InitializeAsync called — IAP not available in this context.");
            return UniTask.CompletedTask;
        }

        public UniTask<IAPResult> BuyAsync(string productId)
        {
            Debug.LogWarning($"[NullIAPService] BuyAsync({productId}) — IAP not available in this context.");
            return UniTask.FromResult(IAPResult.Failed(IAPOutcome.PaymentFailed));
        }
    }
}
