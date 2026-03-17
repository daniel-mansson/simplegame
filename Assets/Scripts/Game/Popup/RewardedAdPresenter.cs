using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the RewardedAd stub popup. Simulates a rewarded ad flow.
    /// Watch completes the ad (grants reward). Skip declines.
    /// </summary>
    public class RewardedAdPresenter : Presenter<IRewardedAdView>
    {
        private UniTaskCompletionSource<bool> _completeTcs;

        public RewardedAdPresenter(IRewardedAdView view) : base(view) { }

        public override void Initialize()
        {
            View.OnWatchClicked += HandleWatch;
            View.OnSkipClicked += HandleSkip;
            View.UpdateStatus("Watch a short ad for a reward?");
        }

        public override void Dispose()
        {
            View.OnWatchClicked -= HandleWatch;
            View.OnSkipClicked -= HandleSkip;
            _completeTcs?.TrySetCanceled();
            _completeTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with true if the ad was watched (reward granted),
        /// or false if the user skipped.
        /// </summary>
        public UniTask<bool> WaitForResult()
        {
            _completeTcs?.TrySetCanceled();
            _completeTcs = new UniTaskCompletionSource<bool>();
            return _completeTcs.Task;
        }

        private void HandleWatch()
        {
            Debug.Log("[RewardedAd] Stub ad watched — reward granted.");
            View.UpdateStatus("Ad complete! Reward granted.");
            _completeTcs?.TrySetResult(true);
        }

        private void HandleSkip()
        {
            Debug.Log("[RewardedAd] Ad skipped.");
            _completeTcs?.TrySetResult(false);
        }
    }
}
