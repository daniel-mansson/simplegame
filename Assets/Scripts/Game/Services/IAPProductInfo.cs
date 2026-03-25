namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Runtime-merged product record combining the local <see cref="IAPProductCatalog"/>
    /// (offline fallback) with live data fetched from the PlayFab catalog.
    ///
    /// Constructed by <see cref="IPlayFabCatalogService.FetchAsync"/> and exposed via
    /// <see cref="IIAPService.Products"/>. Presenters read from this; they never
    /// read <see cref="IAPProductDefinition"/> directly.
    ///
    /// Field priority: PlayFab wins when the fetch succeeds; local catalog values
    /// are used as fallbacks when offline or when a field is absent in PlayFab.
    /// </summary>
    public class IAPProductInfo
    {
        /// <summary>
        /// Store product ID. Immutable — comes from the local <see cref="IAPProductCatalog"/>
        /// and must match PlayFab ItemId, App Store, and Google Play exactly.
        /// </summary>
        public string ProductId { get; }

        /// <summary>Display name shown in the shop UI (e.g. "500 Coins").</summary>
        public string DisplayName { get; set; }

        /// <summary>Flavour text shown in the shop or purchase confirmation popup.</summary>
        public string Description { get; set; }

        /// <summary>Coins granted after PlayFab validates the receipt.</summary>
        public int CoinsAmount { get; set; }

        /// <summary>
        /// URL of the product icon hosted on PlayFab (ItemImageUrl).
        /// Null when not set in the catalog or when the fetch failed.
        /// </summary>
        public string IconUrl { get; set; }

        public IAPProductInfo(string productId, string displayName, string description,
                              int coinsAmount, string iconUrl = null)
        {
            ProductId   = productId;
            DisplayName = displayName;
            Description = description;
            CoinsAmount = coinsAmount;
            IconUrl     = iconUrl;
        }

        /// <summary>
        /// Builds an <see cref="IAPProductInfo"/> from a local <see cref="IAPProductDefinition"/>
        /// with no live data — used as the fallback when PlayFab is unreachable.
        /// </summary>
        public static IAPProductInfo FromLocal(IAPProductDefinition def)
        {
            return new IAPProductInfo(
                productId:   def.ProductId,
                displayName: def.DisplayName,
                description: string.Empty,
                coinsAmount: def.CoinsAmount,
                iconUrl:     null
            );
        }
    }
}
