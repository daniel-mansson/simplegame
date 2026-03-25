using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the Shop popup.
    /// Displays coin pack options read from <see cref="IAPProductCatalog"/> and
    /// delegates all purchase logic to <see cref="IIAPService"/>.
    ///
    /// Coins are granted inside IIAPService after PlayFab validates the receipt —
    /// this presenter never calls ICoinsService directly.
    ///
    /// Pack price strings shown in the UI come from the store (product metadata)
    /// when available; otherwise the catalog DisplayName is used as the label.
    /// </summary>
    public class ShopPresenter : Presenter<IShopView>
    {
        private readonly IIAPService _iap;
        private readonly IAPProductCatalog _catalog;
        private readonly ICoinsService _coins;

        private UniTaskCompletionSource<bool> _resultTcs;
        private bool _purchaseInProgress;

        public ShopPresenter(IShopView view, IIAPService iap, IAPProductCatalog catalog, ICoinsService coins)
            : base(view)
        {
            _iap = iap;
            _catalog = catalog;
            _coins = coins;
        }

        public override void Initialize()
        {
            View.OnPackClicked += HandlePackClicked;
            View.OnCancelClicked += HandleCancelClicked;

            RefreshPackLabels();
            RefreshStatus();
        }

        public override void Dispose()
        {
            View.OnPackClicked -= HandlePackClicked;
            View.OnCancelClicked -= HandleCancelClicked;
            _resultTcs?.TrySetCanceled();
            _resultTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with true if the player made a purchase,
        /// false if they cancelled.
        /// </summary>
        public UniTask<bool> WaitForResult()
        {
            _resultTcs?.TrySetCanceled();
            _resultTcs = new UniTaskCompletionSource<bool>();
            return _resultTcs.Task;
        }

        private void RefreshPackLabels()
        {
            if (_catalog?.Products == null) return;
            for (int i = 0; i < _catalog.Products.Length && i < 3; i++)
            {
                var def = _catalog.Products[i];
                if (def == null) continue;
                // Use display name from catalog — store price is not available until runtime
                // on a real device. Presenters running on device can be extended to read
                // product.metadata.localizedPriceString from the IStoreController if desired.
                View.UpdatePackLabel(i, def.DisplayName);
            }
        }

        private void RefreshStatus()
        {
            var balance = _coins?.Balance ?? 0;
            View.UpdateStatus($"Your balance: {balance} coins");
        }

        private void HandlePackClicked(int packIndex)
        {
            if (_purchaseInProgress)
            {
                Debug.LogWarning("[ShopPresenter] Purchase already in progress.");
                return;
            }

            if (_catalog?.Products == null || packIndex < 0 || packIndex >= _catalog.Products.Length)
            {
                Debug.LogWarning($"[ShopPresenter] Invalid pack index: {packIndex}");
                return;
            }

            var def = _catalog.Products[packIndex];
            if (def == null || string.IsNullOrEmpty(def.ProductId))
            {
                Debug.LogWarning($"[ShopPresenter] No product definition at index {packIndex}.");
                return;
            }

            ExecutePurchaseAsync(def.ProductId).Forget();
        }

        private async UniTaskVoid ExecutePurchaseAsync(string productId)
        {
            _purchaseInProgress = true;
            View.UpdateStatus("Processing...");

            IAPResult result;
            try
            {
                result = await _iap.BuyAsync(productId);

                // Wait one frame so Unity can fully destroy the FakeStore dialog window
                // (Object.Destroy is deferred). Without this, a rapid second tap finds
                // m_UIFakeStoreWindowObject non-null in UIFakeStore.GetOrCreateFakeStoreWindow,
                // reuses the pending-destroy object, and the new dialog is immediately destroyed.
                await UniTask.NextFrame();
            }
            finally
            {
                _purchaseInProgress = false;
            }

            switch (result.Outcome)
            {
                case IAPOutcome.Success:
                    var newBalance = _coins?.Balance ?? result.CoinsGranted;
                    View.UpdateStatus($"Purchase complete! Your balance: {newBalance} coins");
                    _resultTcs?.TrySetResult(true);
                    break;

                case IAPOutcome.Cancelled:
                    View.UpdateStatus("Purchase cancelled.");
                    RefreshStatus();
                    break;

                case IAPOutcome.PaymentFailed:
                    View.UpdateStatus("Purchase failed. Please try again.");
                    break;

                case IAPOutcome.ValidationFailed:
                    View.UpdateStatus("Purchase could not be verified. Please try again.");
                    break;
            }
        }

        private void HandleCancelClicked()
        {
            Debug.Log("[ShopPresenter] Shop cancelled.");
            _resultTcs?.TrySetResult(false);
        }
    }
}
