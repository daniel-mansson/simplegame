using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the Shop popup.
    /// Displays coin pack options from <see cref="IIAPService.Products"/> (runtime-merged
    /// data from PlayFab + local catalog fallback) and delegates purchase logic to
    /// <see cref="IIAPService"/>.
    ///
    /// Coins are granted inside IIAPService after PlayFab validates the receipt —
    /// this presenter never calls ICoinsService directly.
    /// </summary>
    public class ShopPresenter : Presenter<IShopView>
    {
        private readonly IIAPService _iap;
        private readonly ICoinsService _coins;
        private readonly IInputBlocker _inputBlocker;

        private UniTaskCompletionSource<bool> _resultTcs;
        private bool _purchaseInProgress;

        public ShopPresenter(IShopView view, IIAPService iap, ICoinsService coins,
                             IInputBlocker inputBlocker = null)
            : base(view)
        {
            _iap          = iap;
            _coins        = coins;
            _inputBlocker = inputBlocker;
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
            var products = _iap.Products;
            for (int i = 0; i < 3; i++)
            {
                if (i < products.Count)
                {
                    var info = products[i];
                    var label = string.IsNullOrEmpty(info.Description)
                        ? info.DisplayName
                        : $"{info.DisplayName}\n{info.Description}";
                    View.UpdatePackLabel(i, label);
                    View.SetPackVisible(i, true);
                }
                else
                {
                    View.SetPackVisible(i, false);
                }
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

            var products = _iap.Products;
            if (packIndex < 0 || packIndex >= products.Count)
            {
                Debug.LogWarning($"[ShopPresenter] Invalid pack index: {packIndex}");
                return;
            }

            var info = products[packIndex];
            if (string.IsNullOrEmpty(info.ProductId))
            {
                Debug.LogWarning($"[ShopPresenter] No product ID at index {packIndex}.");
                return;
            }

            ExecutePurchaseAsync(info.ProductId).Forget();
        }

        private async UniTaskVoid ExecutePurchaseAsync(string productId)
        {
            _purchaseInProgress = true;
            _inputBlocker?.Block();
            View.UpdateStatus("Processing...");

            IAPResult result;
            try
            {
                result = await _iap.BuyAsync(productId);

                // Wait one frame so Unity can fully destroy the FakeStore dialog window
                // (Object.Destroy is deferred). Without this, a rapid second tap finds
                // m_UIFakeStoreWindowObject non-null in UIFakeStore.GetOrCreateFakeStoreWindow,
                // reuses the pending-destroy object, and the new dialog is immediately destroyed.
                // Skipped outside the player loop (e.g. EditMode tests).
                if (UnityEngine.Application.isPlaying)
                    await UniTask.NextFrame();
            }
            finally
            {
                _inputBlocker?.Unblock();
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
