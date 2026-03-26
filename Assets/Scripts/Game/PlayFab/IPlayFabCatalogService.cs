using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Fetches the IAP product catalog from PlayFab and merges it with the local
    /// <see cref="IAPProductCatalog"/> to produce runtime <see cref="IAPProductInfo"/> records.
    ///
    /// Merge rules:
    ///   - Only products whose <c>ItemId</c> exists in the local catalog are included
    ///     (the local catalog is the registration manifest for Unity Purchasing).
    ///   - PlayFab <c>DisplayName</c>, <c>Description</c>, <c>ItemImageUrl</c> win when present.
    ///   - <c>CoinsAmount</c> is read from PlayFab <c>CustomData["coins"]</c>; falls back to
    ///     the local <see cref="IAPProductDefinition.CoinsAmount"/>.
    ///   - Order follows the local catalog (display order is controlled in Unity, not PlayFab).
    ///
    /// On network failure or parse error, returns a list built entirely from local fallbacks
    /// so the shop is always functional offline.
    /// </summary>
    public interface IPlayFabCatalogService
    {
        /// <summary>
        /// Fetches live product data from PlayFab and returns merged <see cref="IAPProductInfo"/>
        /// records for every product in the local catalog.
        ///
        /// Never throws — returns local fallbacks on any error.
        /// </summary>
        UniTask<IReadOnlyList<IAPProductInfo>> FetchAsync();
    }
}
