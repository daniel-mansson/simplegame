using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Unity Ads Advertisement Legacy SDK implementation of <see cref="IAdService"/>.
    ///
    /// Lifecycle:
    ///   1. <see cref="Initialize"/> → SDK init → <see cref="OnInitializationComplete"/> fires
    ///   2. <see cref="OnInitializationComplete"/> → pre-loads rewarded and interstitial
    ///   3. On each successful show → automatically reloads for next time
    ///
    /// Callbacks bridge to UniTask via <see cref="UniTaskCompletionSource{T}"/>,
    /// following the same pattern as PlayFab callbacks in M016.
    ///
    /// Test IDs (Unity documented test mode):
    ///   iOS game ID:      "5314539"
    ///   Android game ID:  "5314538"
    ///   Rewarded unit:    "Rewarded_iOS" / "Rewarded_Android"
    ///   Interstitial unit:"Interstitial_iOS" / "Interstitial_Android"
    /// </summary>
    public sealed class UnityAdService : IAdService,
        IUnityAdsInitializationListener,
        IUnityAdsLoadListener,
        IUnityAdsShowListener
    {
        private string _rewardedAdUnitId;
        private string _interstitialAdUnitId;
        private IAnalyticsService _analytics;

        private bool _rewardedLoaded;
        private bool _interstitialLoaded;

        private UniTaskCompletionSource<AdResult> _rewardedTcs;
        private UniTaskCompletionSource<AdResult> _interstitialTcs;

        public bool IsRewardedLoaded     => _rewardedLoaded;
        public bool IsInterstitialLoaded => _interstitialLoaded;

        /// <summary>
        /// Injects an optional analytics service for ad event tracking.
        /// Call before <see cref="Initialize"/> or at any time before ads are shown.
        /// </summary>
        public void SetAnalytics(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        // ── IAdService ────────────────────────────────────────────────────────

        public void Initialize(string gameIdIos, string gameIdAndroid, bool testMode)
        {
#if UNITY_IOS
            var gameId = gameIdIos;
            _rewardedAdUnitId     = "Rewarded_iOS";
            _interstitialAdUnitId = "Interstitial_iOS";
#elif UNITY_ANDROID
            var gameId = gameIdAndroid;
            _rewardedAdUnitId     = "Rewarded_Android";
            _interstitialAdUnitId = "Interstitial_Android";
#else
            // Editor / unsupported platform: use iOS IDs as fallback for test mode
            var gameId = gameIdIos;
            _rewardedAdUnitId     = "Rewarded_iOS";
            _interstitialAdUnitId = "Interstitial_iOS";
#endif
            Debug.Log($"[UnityAdService] Initializing — gameId={gameId} testMode={testMode}");
            Advertisement.Initialize(gameId, testMode, this);
        }

        public void LoadRewarded()
        {
            _rewardedLoaded = false;
            Debug.Log($"[UnityAdService] Loading rewarded: {_rewardedAdUnitId}");
            Advertisement.Load(_rewardedAdUnitId, this);
        }

        public void LoadInterstitial()
        {
            _interstitialLoaded = false;
            Debug.Log($"[UnityAdService] Loading interstitial: {_interstitialAdUnitId}");
            Advertisement.Load(_interstitialAdUnitId, this);
        }

        public UniTask<AdResult> ShowRewardedAsync(CancellationToken ct = default)
        {
            if (!_rewardedLoaded)
            {
                Debug.LogWarning("[UnityAdService] ShowRewardedAsync called but rewarded ad not loaded.");
                return UniTask.FromResult(AdResult.NotLoaded);
            }

            _rewardedTcs?.TrySetCanceled();
            _rewardedTcs = new UniTaskCompletionSource<AdResult>();

            Debug.Log($"[UnityAdService] Showing rewarded: {_rewardedAdUnitId}");
            Advertisement.Show(_rewardedAdUnitId, this);

            return _rewardedTcs.Task;
        }

        public UniTask<AdResult> ShowInterstitialAsync(CancellationToken ct = default)
        {
            if (!_interstitialLoaded)
            {
                Debug.LogWarning("[UnityAdService] ShowInterstitialAsync called but interstitial not loaded.");
                return UniTask.FromResult(AdResult.NotLoaded);
            }

            _interstitialTcs?.TrySetCanceled();
            _interstitialTcs = new UniTaskCompletionSource<AdResult>();

            Debug.Log($"[UnityAdService] Showing interstitial: {_interstitialAdUnitId}");
            Advertisement.Show(_interstitialAdUnitId, this);

            return _interstitialTcs.Task;
        }

        // ── IUnityAdsInitializationListener ───────────────────────────────────

        public void OnInitializationComplete()
        {
            Debug.Log("[UnityAdService] SDK initialized — pre-loading ads.");
            LoadRewarded();
            LoadInterstitial();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogWarning($"[UnityAdService] Initialization failed: {error} — {message}. Ads unavailable this session.");
        }

        // ── IUnityAdsLoadListener ─────────────────────────────────────────────

        public void OnUnityAdsAdLoaded(string adUnitId)
        {
            if (adUnitId == _rewardedAdUnitId)
            {
                _rewardedLoaded = true;
                Debug.Log($"[UnityAdService] Rewarded loaded: {adUnitId}");
            }
            else if (adUnitId == _interstitialAdUnitId)
            {
                _interstitialLoaded = true;
                Debug.Log($"[UnityAdService] Interstitial loaded: {adUnitId}");
            }
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"[UnityAdService] Failed to load {adUnitId}: {error} — {message}");
            _analytics?.TrackAdFailedToLoad(AdTypeFrom(adUnitId));

            // Leave loaded flag false — IsXxxLoaded will return false, callers handle gracefully
        }

        // ── IUnityAdsShowListener ─────────────────────────────────────────────

        public void OnUnityAdsShowStart(string adUnitId)
        {
            Debug.Log($"[UnityAdService] Ad started: {adUnitId}");
            _analytics?.TrackAdImpression(AdTypeFrom(adUnitId));
        }

        public void OnUnityAdsShowClick(string adUnitId)
        {
            // No action needed for click tracking at this scope
        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState completionState)
        {
            var adType = AdTypeFrom(adUnitId);
            AdResult result;

            if (completionState == UnityAdsShowCompletionState.COMPLETED)
            {
                result = AdResult.Completed;
                _analytics?.TrackAdCompleted(adType);
            }
            else if (completionState == UnityAdsShowCompletionState.SKIPPED)
            {
                result = AdResult.Skipped;
                _analytics?.TrackAdSkipped(adType);
            }
            else
            {
                result = AdResult.Failed;
                Debug.LogWarning($"[UnityAdService] Ad show unknown completion state: {completionState}");
            }

            Debug.Log($"[UnityAdService] Ad complete: {adUnitId} → {result}");

            // Resolve the waiting task
            if (adUnitId == _rewardedAdUnitId)
                _rewardedTcs?.TrySetResult(result);
            else if (adUnitId == _interstitialAdUnitId)
                _interstitialTcs?.TrySetResult(result);

            // Reload for next time
            if (adUnitId == _rewardedAdUnitId)
                LoadRewarded();
            else if (adUnitId == _interstitialAdUnitId)
                LoadInterstitial();
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"[UnityAdService] Show failure: {adUnitId} — {error}: {message}");

            if (adUnitId == _rewardedAdUnitId)
                _rewardedTcs?.TrySetResult(AdResult.Failed);
            else if (adUnitId == _interstitialAdUnitId)
                _interstitialTcs?.TrySetResult(AdResult.Failed);

            // Attempt reload after failure
            if (adUnitId == _rewardedAdUnitId)
                LoadRewarded();
            else if (adUnitId == _interstitialAdUnitId)
                LoadInterstitial();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string AdTypeFrom(string adUnitId)
            => adUnitId == _rewardedAdUnitId ? "rewarded" : "interstitial";
    }
}
