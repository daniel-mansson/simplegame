using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Test double implementation of <see cref="IAdService"/>.
    /// No dependency on the Unity Ads SDK — safe for use in EditMode tests.
    ///
    /// Configure <see cref="SimulateLoaded"/> to control whether ads are available.
    /// Configure <see cref="SimulateResult"/> to control the outcome of a shown ad.
    /// Configure <see cref="Analytics"/> to verify ad events are fired correctly.
    /// </summary>
    public sealed class NullAdService : IAdService
    {
        /// <summary>
        /// When true, <see cref="IsRewardedLoaded"/> and <see cref="IsInterstitialLoaded"/>
        /// return true and Show calls return <see cref="SimulateResult"/>.
        /// When false, both return <see cref="AdResult.NotLoaded"/> immediately.
        /// Default: true.
        /// </summary>
        public bool SimulateLoaded { get; set; } = true;

        /// <summary>
        /// The result returned by Show calls when <see cref="SimulateLoaded"/> is true.
        /// Default: <see cref="AdResult.Completed"/>.
        /// </summary>
        public AdResult SimulateResult { get; set; } = AdResult.Completed;

        /// <summary>
        /// Optional analytics service. When set, ad events are forwarded to verify
        /// that the correct events fire in unit tests.
        /// </summary>
        public IAnalyticsService Analytics { get; set; }

        public bool IsRewardedLoaded     => SimulateLoaded;
        public bool IsInterstitialLoaded => SimulateLoaded;

        public void Initialize(string appKey) { }
        public void LoadRewarded()     { }
        public void LoadInterstitial() { }

        public UniTask<AdResult> ShowRewardedAsync(CancellationToken ct = default)
        {
            if (!SimulateLoaded)
            {
                Analytics?.TrackAdFailedToLoad("rewarded");
                return UniTask.FromResult(AdResult.NotLoaded);
            }

            Analytics?.TrackAdImpression("rewarded");

            var result = SimulateResult;
            if (result == AdResult.Completed)
                Analytics?.TrackAdCompleted("rewarded");
            else if (result == AdResult.Skipped)
                Analytics?.TrackAdSkipped("rewarded");

            return UniTask.FromResult(result);
        }

        public UniTask<AdResult> ShowInterstitialAsync(CancellationToken ct = default)
        {
            if (!SimulateLoaded)
            {
                Analytics?.TrackAdFailedToLoad("interstitial");
                return UniTask.FromResult(AdResult.NotLoaded);
            }

            Analytics?.TrackAdImpression("interstitial");

            var result = SimulateResult;
            if (result == AdResult.Completed)
                Analytics?.TrackAdCompleted("interstitial");
            else if (result == AdResult.Skipped)
                Analytics?.TrackAdSkipped("interstitial");

            return UniTask.FromResult(result);
        }
    }
}
