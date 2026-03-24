using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Abstraction over the in-app purchase system.
    /// Implementations: <see cref="MockIAPService"/> (Editor/tests),
    /// <c>UnityIAPService</c> (device — added in M019/S02).
    ///
    /// Coins are granted internally by the implementation after server-side
    /// validation. Callers read the <see cref="IAPResult"/> to determine
    /// what to show in the UI — they do NOT call ICoinsService directly.
    /// </summary>
    public interface IIAPService
    {
        /// <summary>
        /// True after <see cref="InitializeAsync"/> has completed successfully.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initialise the underlying store connection and load the product list.
        /// Must be awaited before calling <see cref="BuyAsync"/>.
        /// Safe to call multiple times — subsequent calls are no-ops if already initialised.
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// Initiate a purchase for the given product ID.
        /// Returns after the full flow completes (store transaction + server validation).
        /// Callers must not call <see cref="ICoinsService"/> directly — coins are
        /// granted inside the implementation on validation success.
        /// </summary>
        /// <param name="productId">
        /// Must match a product ID in <see cref="IAPProductCatalog"/> and in the
        /// PlayFab title catalog.
        /// </param>
        UniTask<IAPResult> BuyAsync(string productId);
    }
}
