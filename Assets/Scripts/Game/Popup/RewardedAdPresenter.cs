using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the RewardedAd popup.
    ///
    /// Watch flow:
    ///   - If <see cref="IAdService.IsRewardedLoaded"/> is false: grays the Watch button,
    ///     updates status text — player must use Skip.
    ///   - If loaded: shows the ad via <see cref="IAdService.ShowRewardedAsync"/>.
    ///     <see cref="AdResult.Completed"/> → <see cref="WaitForResult"/> resolves true (grant reward).
    ///     <see cref="AdResult.Skipped"/> or <see cref="AdResult.Failed"/> → resolves false (no reward).
    ///
    /// Skip flow: resolves <see cref="WaitForResult"/> with false immediately.
    /// </summary>
    public class RewardedAdPresenter : Presenter<IRewardedAdView>
    {
        private readonly IAdService _adService;
        private UniTaskCompletionSource<bool> _completeTcs;

        public RewardedAdPresenter(IRewardedAdView view, IAdService adService = null) : base(view)
        {
            _adService = adService ?? new NullAdService();
        }

        public override void Initialize()
        {
            View.OnWatchClicked += HandleWatch;
            View.OnSkipClicked  += HandleSkip;
            View.UpdateStatus("Watch a short ad for a reward?");
            View.SetWatchInteractable(_adService.IsRewardedLoaded);
        }

        public override void Dispose()
        {
            View.OnWatchClicked -= HandleWatch;
            View.OnSkipClicked  -= HandleSkip;
            _completeTcs?.TrySetCanceled();
            _completeTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with:
        /// <c>true</c>  — ad completed, reward should be granted.
        /// <c>false</c> — ad skipped, failed, unavailable, or player tapped Skip.
        /// </summary>
        public UniTask<bool> WaitForResult()
        {
            _completeTcs?.TrySetCanceled();
            _completeTcs = new UniTaskCompletionSource<bool>();
            return _completeTcs.Task;
        }

        private void HandleWatch()
        {
            if (!_adService.IsRewardedLoaded)
            {
                View.SetWatchInteractable(false);
                View.UpdateStatus("Ad not available right now.");
                Debug.Log("[RewardedAdPresenter] Ad not loaded — Watch button disabled.");
                return;
            }

            ShowAdAsync().Forget();
        }

        private async UniTaskVoid ShowAdAsync()
        {
            View.SetWatchInteractable(false);
            var result = await _adService.ShowRewardedAsync();

            Debug.Log($"[RewardedAdPresenter] Ad result: {result}");

            if (result == AdResult.Completed)
            {
                View.UpdateStatus("Ad complete! Reward granted.");
                _completeTcs?.TrySetResult(true);
            }
            else
            {
                View.UpdateStatus(result == AdResult.NotLoaded
                    ? "Ad not available right now."
                    : "Ad ended early — no reward.");
                _completeTcs?.TrySetResult(false);
            }
        }

        private void HandleSkip()
        {
            Debug.Log("[RewardedAdPresenter] Ad skipped.");
            _completeTcs?.TrySetResult(false);
        }
    }
}
