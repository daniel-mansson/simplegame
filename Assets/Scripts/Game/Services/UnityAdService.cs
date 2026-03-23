using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if LEVELPLAY_ENABLED
using Unity.Services.LevelPlay;
#endif

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Unity LevelPlay (Ads Mediation) implementation of <see cref="IAdService"/>.
    ///
    /// ─── HOW TO ACTIVATE ────────────────────────────────────────────────────────
    /// This class is a compile-safe stub. To wire real ads:
    ///
    ///   1. In Unity Editor: Window → Package Manager → Unity Registry
    ///      Install "Ads Mediation" (com.unity.services.levelplay).
    ///
    ///   2. In Unity Editor: Ads Mediation → Integration Manager
    ///      Install the "Unity Ads" adapter. Run the Android/iOS resolver.
    ///
    ///   3. Sign up at https://platform.ironsource.io and create an app.
    ///      Copy your App Key into GameBootstrapper where UnityAdService is constructed.
    ///
    ///   4. Add LEVELPLAY_ENABLED to Project Settings → Player → Scripting Define Symbols.
    ///      This activates the real implementation below.
    ///
    ///   5. Add "Unity.Services.LevelPlay" to SimpleGame.Game.asmdef references.
    ///
    /// Lifecycle (when LEVELPLAY_ENABLED):
    ///   Initialize(appKey) → OnInitSuccess → Load rewarded + interstitial
    ///   After each show → auto-reload for next time
    ///
    /// Ad Unit IDs (LevelPlay defaults — confirm in your LevelPlay dashboard):
    ///   Rewarded:      "DefaultRewardedVideoStoreId"  (or your custom placement)
    ///   Interstitial:  "DefaultInterstitialStoreId"   (or your custom placement)
    /// ────────────────────────────────────────────────────────────────────────────
    /// </summary>
    public sealed class UnityAdService : IAdService
    {
        private IAnalyticsService _analytics;
        private ISingularService _singular;

        // ── Loaded state ──────────────────────────────────────────────────────

        public bool IsRewardedLoaded     { get; private set; }
        public bool IsInterstitialLoaded { get; private set; }

        // ── Pending result tasks ──────────────────────────────────────────────

        private UniTaskCompletionSource<AdResult> _rewardedTcs;
        private UniTaskCompletionSource<AdResult> _interstitialTcs;

        /// <summary>Inject analytics before calling Initialize.</summary>
        public void SetAnalytics(IAnalyticsService analytics) => _analytics = analytics;

        /// <summary>Inject Singular MMP before calling Initialize.</summary>
        public void SetSingular(ISingularService singular) => _singular = singular;

        // ── IAdService ────────────────────────────────────────────────────────

        public void Initialize(string appKey, bool testMode = false)
        {
#if LEVELPLAY_ENABLED
            if (testMode)
            {
                // Enables test ads for registered test devices (required while app is in "Temp" status on LevelPlay dashboard).
                // Register device GAID at: platform.ironsrc.com → Monetize → Setup → SDK Testing
                IronSource.Agent.setAdaptersDebug(true);
            }
            LevelPlay.OnInitSuccess  += OnInitSuccess;
            LevelPlay.OnInitFailed   += OnInitFailed;
            LevelPlay.Init(appKey);
            Debug.Log($"[UnityAdService] LevelPlay.Init called — appKey={appKey} testMode={testMode}");
#else
            Debug.LogWarning("[UnityAdService] LevelPlay SDK not installed. Install com.unity.services.levelplay and add LEVELPLAY_ENABLED scripting symbol. Ads will be unavailable this session.");
#endif
        }

        public void LoadRewarded()
        {
#if LEVELPLAY_ENABLED
            var ad = new LevelPlayRewardedAd("DefaultRewardedVideoStoreId");
            ad.OnAdLoaded        += (info) => { IsRewardedLoaded = true; _currentRewarded = ad; Debug.Log("[UnityAdService] Rewarded loaded."); };
            ad.OnAdLoadFailed    += (error) => { Debug.LogWarning($"[UnityAdService] Rewarded failed to load: {error}"); _analytics?.TrackAdFailedToLoad("rewarded"); };
            ad.OnAdDisplayed     += (info)  => { _analytics?.TrackAdImpression("rewarded"); ReportAdRevenue(info); };
            ad.OnAdClosed        += (info)  => { IsRewardedLoaded = false; _rewardedTcs?.TrySetResult(AdResult.Completed); _analytics?.TrackAdCompleted("rewarded"); LoadRewarded(); };
            ad.OnAdDisplayFailed += (info, error) => { _rewardedTcs?.TrySetResult(AdResult.Failed); LoadRewarded(); };
            ad.LoadAd();
#else
            IsRewardedLoaded = false;
#endif
        }

        public void LoadInterstitial()
        {
#if LEVELPLAY_ENABLED
            var ad = new LevelPlayInterstitialAd("DefaultInterstitialStoreId");
            ad.OnAdLoaded        += (info) => { IsInterstitialLoaded = true; _currentInterstitial = ad; Debug.Log("[UnityAdService] Interstitial loaded."); };
            ad.OnAdLoadFailed    += (error) => { Debug.LogWarning($"[UnityAdService] Interstitial failed to load: {error}"); _analytics?.TrackAdFailedToLoad("interstitial"); };
            ad.OnAdDisplayed     += (info)  => { _analytics?.TrackAdImpression("interstitial"); ReportAdRevenue(info); };
            ad.OnAdClosed        += (info)  => { IsInterstitialLoaded = false; _interstitialTcs?.TrySetResult(AdResult.Completed); _analytics?.TrackAdCompleted("interstitial"); LoadInterstitial(); };
            ad.OnAdDisplayFailed += (info, error) => { _interstitialTcs?.TrySetResult(AdResult.Failed); LoadInterstitial(); };
            ad.LoadAd();
#else
            IsInterstitialLoaded = false;
#endif
        }

        public UniTask<AdResult> ShowRewardedAsync(CancellationToken ct = default)
        {
#if LEVELPLAY_ENABLED
            if (!IsRewardedLoaded || _currentRewarded == null)
            {
                Debug.LogWarning("[UnityAdService] ShowRewardedAsync — rewarded not loaded.");
                return UniTask.FromResult(AdResult.NotLoaded);
            }
            _rewardedTcs?.TrySetCanceled();
            _rewardedTcs = new UniTaskCompletionSource<AdResult>();
            _currentRewarded.ShowAd();
            return _rewardedTcs.Task;
#else
            Debug.LogWarning("[UnityAdService] ShowRewardedAsync — LevelPlay not installed.");
            return UniTask.FromResult(AdResult.NotLoaded);
#endif
        }

        public UniTask<AdResult> ShowInterstitialAsync(CancellationToken ct = default)
        {
#if LEVELPLAY_ENABLED
            if (!IsInterstitialLoaded || _currentInterstitial == null)
            {
                Debug.LogWarning("[UnityAdService] ShowInterstitialAsync — interstitial not loaded.");
                return UniTask.FromResult(AdResult.NotLoaded);
            }
            _interstitialTcs?.TrySetCanceled();
            _interstitialTcs = new UniTaskCompletionSource<AdResult>();
            _currentInterstitial.ShowAd();
            return _interstitialTcs.Task;
#else
            Debug.LogWarning("[UnityAdService] ShowInterstitialAsync — LevelPlay not installed.");
            return UniTask.FromResult(AdResult.NotLoaded);
#endif
        }

        // ── LevelPlay init callbacks (compiled only with LEVELPLAY_ENABLED) ───

#if LEVELPLAY_ENABLED
        private LevelPlayRewardedAd     _currentRewarded;
        private LevelPlayInterstitialAd _currentInterstitial;

        private void OnInitSuccess(LevelPlayConfiguration config)
        {
            Debug.Log("[UnityAdService] LevelPlay initialized — loading ads.");
            // TODO(ads): Remove ValidateIntegration() call before shipping.
            // Run once to get device GAID (at bottom of log) for registering in
            // platform.ironsrc.com → Monetize → Setup → SDK Testing.
            LevelPlay.ValidateIntegration();
            LoadRewarded();
            LoadInterstitial();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning($"[UnityAdService] LevelPlay init failed: {error}. Ads unavailable this session.");
        }

        private void ReportAdRevenue(LevelPlayAdInfo info)
        {
            if (info == null) return;
            var revenue  = info.Revenue ?? 0;
            var network  = string.IsNullOrEmpty(info.AdNetwork) ? "LevelPlay" : info.AdNetwork;
            (_singular ?? new NullSingularService()).ReportAdRevenue(network, "USD", revenue);
        }
#endif
    }
}
