using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the IAPPurchase popup — a single-item coin purchase confirmation.
    /// Delegates all purchase logic to <see cref="IIAPService"/>.
    ///
    /// Coins are granted inside IIAPService after PlayFab validates the receipt —
    /// this presenter never calls ICoinsService directly.
    ///
    /// The product ID and display info come from an <see cref="IAPProductDefinition"/>
    /// passed at construction time. The caller decides which product to show.
    /// </summary>
    public class IAPPurchasePresenter : Presenter<IIAPPurchaseView>
    {
        private readonly IIAPService _iap;
        private readonly IAPProductDefinition _product;
        private readonly ICoinsService _coins;

        private UniTaskCompletionSource<bool> _resultTcs;

        public IAPPurchasePresenter(IIAPPurchaseView view, IIAPService iap, IAPProductDefinition product, ICoinsService coins)
            : base(view)
        {
            _iap = iap;
            _product = product;
            _coins = coins;
        }

        public override void Initialize()
        {
            View.OnPurchaseClicked += HandlePurchase;
            View.OnCancelClicked += HandleCancel;

            var itemName = _product?.DisplayName ?? "Coin Pack";
            var coinsAmt = _product?.CoinsAmount ?? 0;
            View.UpdateItemName(itemName);
            View.UpdatePrice($"{coinsAmt} coins");
            View.UpdateStatus("Tap Purchase to buy.");
        }

        public override void Dispose()
        {
            View.OnPurchaseClicked -= HandlePurchase;
            View.OnCancelClicked -= HandleCancel;
            _resultTcs?.TrySetCanceled();
            _resultTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with true if purchased, false if cancelled.
        /// </summary>
        public UniTask<bool> WaitForResult()
        {
            _resultTcs?.TrySetCanceled();
            _resultTcs = new UniTaskCompletionSource<bool>();
            return _resultTcs.Task;
        }

        private void HandlePurchase()
        {
            if (_product == null || string.IsNullOrEmpty(_product.ProductId))
            {
                Debug.LogWarning("[IAPPurchasePresenter] No product configured.");
                View.UpdateStatus("Purchase unavailable.");
                return;
            }

            ExecutePurchaseAsync(_product.ProductId).Forget();
        }

        private async UniTaskVoid ExecutePurchaseAsync(string productId)
        {
            View.UpdateStatus("Processing...");
            View.OnPurchaseClicked -= HandlePurchase;  // prevent double-tap

            IAPResult result;
            try
            {
                result = await _iap.BuyAsync(productId);
            }
            finally
            {
                View.OnPurchaseClicked += HandlePurchase;
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
                    View.UpdateStatus("Tap Purchase to buy.");
                    break;

                case IAPOutcome.PaymentFailed:
                    View.UpdateStatus("Purchase failed. Please try again.");
                    break;

                case IAPOutcome.ValidationFailed:
                    View.UpdateStatus("Purchase could not be verified. Please try again.");
                    break;
            }
        }

        private void HandleCancel()
        {
            Debug.Log("[IAPPurchasePresenter] Purchase cancelled.");
            _resultTcs?.TrySetResult(false);
        }
    }
}
