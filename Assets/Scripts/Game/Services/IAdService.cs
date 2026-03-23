using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for showing Unity Ads rewarded and interstitial advertisements.
    ///
    /// Lifecycle per ad type:
    ///   1. <see cref="Initialize"/> is called once at boot.
    ///   2. The implementation pre-loads both ad types automatically after initialization.
    ///   3. Call <see cref="IsRewardedLoaded"/> / <see cref="IsInterstitialLoaded"/> before showing.
    ///   4. Call <see cref="ShowRewardedAsync"/> / <see cref="ShowInterstitialAsync"/> to show.
    ///   5. The implementation reloads automatically after each successful show.
    ///
    /// Implementations:
    ///   <see cref="UnityAdService"/> — wraps the Unity LevelPlay (Ads Mediation) SDK.
    ///   <see cref="NullAdService"/>  — deterministic test double, no SDK dependency.
    /// </summary>
    public interface IAdService
    {
        /// <summary>
        /// Initializes the ad SDK with the LevelPlay App Key.
        /// Must be called once before any Load or Show operations.
        /// </summary>
        /// <param name="appKey">LevelPlay App Key from the ironSource/LevelPlay dashboard.</param>
        /// <param name="testMode">When true, enables adapter debug logging and test ad serving.
        /// Required while app is in "Temp" status on the LevelPlay dashboard (before going live on store).
        /// Register test device GAID at: platform.ironsrc.com → Monetize → Setup → SDK Testing.</param>
        void Initialize(string appKey, bool testMode = false);

        /// <summary>
        /// Manually triggers a load of the rewarded ad unit.
        /// Normally called automatically after initialization and after each show.
        /// </summary>
        void LoadRewarded();

        /// <summary>
        /// Shows the pre-loaded rewarded ad and waits for the player to finish or skip.
        /// Returns <see cref="AdResult.NotLoaded"/> immediately if <see cref="IsRewardedLoaded"/> is false.
        /// </summary>
        UniTask<AdResult> ShowRewardedAsync(CancellationToken ct = default);

        /// <summary>Whether a rewarded ad is currently loaded and ready to show.</summary>
        bool IsRewardedLoaded { get; }

        /// <summary>
        /// Manually triggers a load of the interstitial ad unit.
        /// Normally called automatically after initialization and after each show.
        /// </summary>
        void LoadInterstitial();

        /// <summary>
        /// Shows the pre-loaded interstitial ad and waits for it to finish.
        /// Returns <see cref="AdResult.NotLoaded"/> immediately if <see cref="IsInterstitialLoaded"/> is false.
        /// </summary>
        UniTask<AdResult> ShowInterstitialAsync(CancellationToken ct = default);

        /// <summary>Whether an interstitial ad is currently loaded and ready to show.</summary>
        bool IsInterstitialLoaded { get; }
    }
}
