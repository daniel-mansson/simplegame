using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// No-op catalog service — returns local fallbacks immediately without
    /// making any PlayFab call. Used in tests and when PlayFab auth is unavailable.
    /// </summary>
    public class NullPlayFabCatalogService : IPlayFabCatalogService
    {
        private readonly IAPProductCatalog _local;

        public NullPlayFabCatalogService(IAPProductCatalog local = null)
        {
            _local = local;
        }

        public UniTask<IReadOnlyList<IAPProductInfo>> FetchAsync()
        {
            if (_local?.Products == null)
                return UniTask.FromResult<IReadOnlyList<IAPProductInfo>>(System.Array.Empty<IAPProductInfo>());

            var list = new List<IAPProductInfo>(_local.Products.Length);
            foreach (var def in _local.Products)
            {
                if (def != null && !string.IsNullOrEmpty(def.ProductId))
                    list.Add(IAPProductInfo.FromLocal(def));
            }
            return UniTask.FromResult<IReadOnlyList<IAPProductInfo>>(list);
        }
    }
}
