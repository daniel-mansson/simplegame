using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Single source of truth for all purchasable coin pack definitions.
    /// Both <c>ShopPresenter</c> and <c>IAPPurchasePresenter</c> read from this asset.
    ///
    /// Product IDs here MUST match the PlayFab title catalog ItemIds and the
    /// store listings in App Store Connect / Google Play Console.
    ///
    /// Create via: Assets → Create → SimpleGame → IAP → Product Catalog
    /// Asset location: Assets/Resources/IAPProductCatalog.asset
    /// </summary>
    [CreateAssetMenu(menuName = "SimpleGame/IAP/Product Catalog", fileName = "IAPProductCatalog")]
    public class IAPProductCatalog : ScriptableObject
    {
        [Tooltip("All purchasable coin packs. Order determines display order in the shop.")]
        public IAPProductDefinition[] Products = System.Array.Empty<IAPProductDefinition>();

        /// <summary>
        /// Returns the product definition for the given product ID, or null if not found.
        /// </summary>
        public IAPProductDefinition FindById(string productId)
        {
            if (Products == null) return null;
            foreach (var p in Products)
            {
                if (p != null && p.ProductId == productId)
                    return p;
            }
            return null;
        }
    }
}
