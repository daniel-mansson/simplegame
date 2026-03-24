using System;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Defines a single coin pack product available for purchase.
    /// Lives inside <see cref="IAPProductCatalog"/>.
    /// </summary>
    [Serializable]
    public class IAPProductDefinition
    {
        /// <summary>
        /// Store product ID. Must match exactly:
        /// - Unity Purchasing product registration
        /// - PlayFab title catalog ItemId
        /// - Apple App Store / Google Play listing
        /// </summary>
        [Tooltip("Store product ID — must match PlayFab catalog ItemId and store listing exactly.")]
        public string ProductId;

        /// <summary>Number of coins granted to the player on successful purchase.</summary>
        [Tooltip("Coins granted after PlayFab validates the receipt.")]
        public int CoinsAmount;

        /// <summary>Display name shown in the shop UI (e.g. "500 Coins").</summary>
        [Tooltip("Label shown on the purchase button in the shop.")]
        public string DisplayName;
    }
}
