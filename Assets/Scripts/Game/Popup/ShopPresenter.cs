using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the Shop popup.
    /// Displays three coin pack tiers backed by fake IAP (stub — no real store SDK).
    /// Purchasing a pack grants coins immediately. Cancel closes the shop.
    ///
    /// Pack tiers (hardcoded for stub implementation):
    ///   Pack 0: 500 coins — €1.99
    ///   Pack 1: 1200 coins — €3.99
    ///   Pack 2: 2500 coins — €7.99
    /// </summary>
    public class ShopPresenter : Presenter<IShopView>
    {
        private static readonly (int coins, string label)[] Packs =
        {
            (500,  "500 Coins\n€1.99"),
            (1200, "1200 Coins\n€3.99"),
            (2500, "2500 Coins\n€7.99"),
        };

        private readonly ICoinsService _coins;
        private UniTaskCompletionSource<bool> _resultTcs;

        public ShopPresenter(IShopView view, ICoinsService coins) : base(view)
        {
            _coins = coins;
        }

        public override void Initialize()
        {
            View.OnPackClicked += HandlePackClicked;
            View.OnCancelClicked += HandleCancelClicked;

            for (int i = 0; i < Packs.Length; i++)
                View.UpdatePackLabel(i, Packs[i].label);

            View.UpdateStatus("Choose a coin pack.");
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

        private void HandlePackClicked(int packIndex)
        {
            if (packIndex < 0 || packIndex >= Packs.Length)
            {
                Debug.LogWarning($"[ShopPresenter] Invalid pack index: {packIndex}");
                return;
            }

            var (coinsGranted, _) = Packs[packIndex];
            Debug.Log($"[ShopPresenter] Stub purchase: pack {packIndex} — granting {coinsGranted} coins.");

            _coins?.Earn(coinsGranted);
            _coins?.Save();

            View.UpdateStatus($"+{coinsGranted} coins added!");
            _resultTcs?.TrySetResult(true);
        }

        private void HandleCancelClicked()
        {
            Debug.Log("[ShopPresenter] Shop cancelled.");
            _resultTcs?.TrySetResult(false);
        }
    }
}
