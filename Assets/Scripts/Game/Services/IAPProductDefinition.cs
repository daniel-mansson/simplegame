using System;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Defines a single coin pack product in the local <see cref="IAPProductCatalog"/>.
    ///
    /// Role after the PlayFab catalog integration:
    ///   - <see cref="ProductId"/> is the stable anchor — it drives Unity Purchasing
    ///     product registration and must match PlayFab ItemId and store listings exactly.
    ///   - <see cref="DisplayName"/> and <see cref="CoinsAmount"/> are offline fallbacks.
    ///     The live values come from <see cref="IAPProductInfo"/> populated by
    ///     <see cref="IPlayFabCatalogService.FetchAsync"/> at runtime.
    ///
    /// Keep these fallback values in sync with PlayFab manually until a first run
    /// with network is guaranteed for all users.
    /// </summary>
    [Serializable]
    public class IAPProductDefinition
    {
        /// <summary>
        /// Store product ID. Must match exactly:
        ///   - Unity Purchasing product registration
        ///   - PlayFab title catalog ItemId
        ///   - Apple App Store / Google Play listing
        /// </summary>
        [Tooltip("Store product ID — must match PlayFab catalog ItemId and store listing exactly.")]
        public string ProductId;

        /// <summary>
        /// Offline fallback: number of coins granted on successful purchase.
        /// Overridden at runtime by PlayFab CustomData["coins"] when reachable.
        /// </summary>
        [Tooltip("Fallback coins granted. Live value comes from PlayFab CustomData[\"coins\"].")]
        public int CoinsAmount;

        /// <summary>
        /// Offline fallback: display name shown in the shop UI (e.g. "500 Coins").
        /// Overridden at runtime by the PlayFab catalog DisplayName when reachable.
        /// </summary>
        [Tooltip("Fallback display name. Live value comes from PlayFab catalog DisplayName.")]
        public string DisplayName;
    }
}
