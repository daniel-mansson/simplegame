using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the IAPPurchase stub popup. Simulates an in-app purchase.
    /// Purchase grants golden pieces, Cancel declines.
    /// </summary>
    public class IAPPurchasePresenter : Presenter<IIAPPurchaseView>
    {
        private readonly string _itemName;
        private readonly string _price;
        private readonly int _goldenPiecesGranted;
        private UniTaskCompletionSource<bool> _resultTcs;

        public IAPPurchasePresenter(IIAPPurchaseView view, string itemName, string price, int goldenPiecesGranted)
            : base(view)
        {
            _itemName = itemName;
            _price = price;
            _goldenPiecesGranted = goldenPiecesGranted;
        }

        public override void Initialize()
        {
            View.OnPurchaseClicked += HandlePurchase;
            View.OnCancelClicked += HandleCancel;
            View.UpdateItemName(_itemName);
            View.UpdatePrice(_price);
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
            Debug.Log($"[IAPPurchase] Stub purchase: {_itemName} for {_price} — granting {_goldenPiecesGranted} golden pieces.");
            View.UpdateStatus("Purchase complete!");
            _resultTcs?.TrySetResult(true);
        }

        private void HandleCancel()
        {
            Debug.Log("[IAPPurchase] Purchase cancelled.");
            _resultTcs?.TrySetResult(false);
        }
    }
}
