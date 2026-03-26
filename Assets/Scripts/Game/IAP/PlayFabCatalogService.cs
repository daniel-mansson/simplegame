using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Fetches the PlayFab Classic catalog and merges it with the local
    /// <see cref="IAPProductCatalog"/> to produce <see cref="IAPProductInfo"/> records.
    ///
    /// PlayFab is authoritative: only items returned by PlayFab appear in the output.
    /// Local catalog values are used as fallbacks for fields missing from PlayFab
    /// (DisplayName, CoinsAmount). Products absent from PlayFab are not shown.
    ///
    /// On network failure, returns the full local catalog as fallback so the shop
    /// remains functional offline.
    /// </summary>
    public class PlayFabCatalogService : IPlayFabCatalogService
    {
        private readonly IAPProductCatalog _local;

        /// <param name="local">
        /// Local catalog — drives product registration order and provides offline fallbacks.
        /// May be null; if so, FetchAsync returns an empty list.
        /// </param>
        public PlayFabCatalogService(IAPProductCatalog local)
        {
            _local = local;
        }

        /// <inheritdoc/>
        public async UniTask<IReadOnlyList<IAPProductInfo>> FetchAsync()
        {
            if (_local?.Products == null || _local.Products.Length == 0)
                return Array.Empty<IAPProductInfo>();

            // Build a local-fallback list first — returned as-is on any fetch failure.
            var results = BuildLocalFallbacks();

            var tcs = new UniTaskCompletionSource<List<CatalogItem>>();

            PlayFabClientAPI.GetCatalogItems(
                new GetCatalogItemsRequest { CatalogVersion = null }, // null = title default catalog
                result => tcs.TrySetResult(result.Catalog),
                error =>
                {
                    Debug.LogWarning($"[PlayFabCatalogService] GetCatalogItems failed: {error.ErrorMessage}. Using local fallbacks.");
                    tcs.TrySetResult(null);
                });

            var catalog = await tcs.Task;

            if (catalog == null)
                return results; // network failure — return local fallbacks

            // PlayFab is authoritative: build output from PlayFab items only.
            // Products not listed in PlayFab are not shown (even if in the local catalog).
            // Local catalog provides fallback field values for matched products.

            // Build a local lookup for O(1) fallback access.
            var localById = new Dictionary<string, IAPProductInfo>(results.Count, StringComparer.Ordinal);
            foreach (var info in results)
                localById[info.ProductId] = info;

            var merged = new List<IAPProductInfo>(catalog.Count);
            foreach (var item in catalog)
            {
                if (string.IsNullOrEmpty(item.ItemId))
                    continue;

                // Use local fallback values as the base, or create a blank entry.
                localById.TryGetValue(item.ItemId, out var local);

                var displayName = string.IsNullOrEmpty(item.DisplayName)
                    ? (local?.DisplayName ?? item.ItemId)
                    : item.DisplayName;
                var description  = string.IsNullOrEmpty(item.Description)  ? (local?.Description  ?? string.Empty) : item.Description;
                var iconUrl      = string.IsNullOrEmpty(item.ItemImageUrl)  ? (local?.IconUrl      ?? string.Empty) : item.ItemImageUrl;

                int coins = ParseCoins(item.CustomData, item.ItemId);
                if (coins <= 0)
                    coins = local?.CoinsAmount ?? 0;

                merged.Add(new IAPProductInfo(item.ItemId, displayName, description, coins, iconUrl));
            }

            return merged;
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private List<IAPProductInfo> BuildLocalFallbacks()
        {
            var list = new List<IAPProductInfo>(_local.Products.Length);
            foreach (var def in _local.Products)
            {
                if (def != null && !string.IsNullOrEmpty(def.ProductId))
                    list.Add(IAPProductInfo.FromLocal(def));
            }
            return list;
        }

        /// <summary>
        /// Parses the <c>coins</c> field from PlayFab CustomData JSON.
        /// CustomData is a plain string: <c>{"coins":500}</c>
        /// Returns 0 on any parse failure so the caller keeps the local fallback.
        /// </summary>
        private static int ParseCoins(string customData, string productId)
        {
            if (string.IsNullOrEmpty(customData)) return 0;
            try
            {
                // Minimal parse — avoids adding a JSON library dependency.
                // CustomData is a simple flat object; just find the "coins" key.
                const string key = "\"coins\"";
                int keyIdx = customData.IndexOf(key, StringComparison.Ordinal);
                if (keyIdx < 0) return 0;

                int colonIdx = customData.IndexOf(':', keyIdx + key.Length);
                if (colonIdx < 0) return 0;

                // Skip whitespace after the colon.
                int numStart = colonIdx + 1;
                while (numStart < customData.Length && customData[numStart] == ' ')
                    numStart++;

                // Read digits.
                int numEnd = numStart;
                while (numEnd < customData.Length && char.IsDigit(customData[numEnd]))
                    numEnd++;

                if (numEnd == numStart) return 0;

                return int.Parse(customData.Substring(numStart, numEnd - numStart));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayFabCatalogService] CustomData parse error for {productId}: {ex.Message}");
                return 0;
            }
        }
    }
}
