using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Configuration for <see cref="MockIAPService"/> used in the Editor and EditMode tests.
    /// Set <see cref="MockOutcome"/> to the outcome you want <c>BuyAsync</c> to return.
    ///
    /// Create via: Assets → Create → SimpleGame → IAP → Mock Config
    /// Asset location: Assets/Resources/IAPMockConfig.asset
    /// </summary>
    [CreateAssetMenu(menuName = "SimpleGame/IAP/Mock Config", fileName = "IAPMockConfig")]
    public class IAPMockConfig : ScriptableObject
    {
        [Tooltip("Outcome returned by MockIAPService.BuyAsync.")]
        public IAPOutcome MockOutcome = IAPOutcome.Success;

        [Tooltip("Coins granted when MockOutcome is Success.")]
        public int CoinsGranted = 500;
    }
}
