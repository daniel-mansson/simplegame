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
    /// See <see cref="IPlayFabCatalogService"/> for merge rules and error-handling contract.
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
                return results; // network failure — fallbacks already built

            // Build a lookup by ItemId for O(1) merge.
            var byId = new Dictionary<string, CatalogItem>(catalog.Count, StringComparer.Ordinal);
            foreach (var item in catalog)
            {
                if (!string.IsNullOrEmpty(item.ItemId))
                    byId[item.ItemId] = item;
            }

            // Merge into results, which are already ordered by local catalog index.
            for (int i = 0; i < results.Count; i++)
            {
                var info = results[i];
                if (!byId.TryGetValue(info.ProductId, out var playfab))
                    continue; // product missing from PlayFab — keep local fallback

                if (!string.IsNullOrEmpty(playfab.DisplayName))
                    info.DisplayName = playfab.DisplayName;

                if (!string.IsNullOrEmpty(playfab.Description))
                    info.Description = playfab.Description;

                if (!string.IsNullOrEmpty(playfab.ItemImageUrl))
                    info.IconUrl = playfab.ItemImageUrl;

                int playfabCoins = ParseCoins(playfab.CustomData, info.ProductId);
                if (playfabCoins > 0)
                    info.CoinsAmount = playfabCoins;
            }

            return results;
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
