using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode contract tests for <see cref="IAdService"/> via <see cref="NullAdService"/>.
    /// No Unity Ads SDK dependency — all tests use the test double.
    /// </summary>
    [TestFixture]
    public class AdServiceTests
    {
        // ── Rewarded — loaded ────────────────────────────────────────────────

        [Test]
        public void Rewarded_WhenSimulateLoaded_IsRewardedLoadedIsTrue()
        {
            var svc = new NullAdService { SimulateLoaded = true };
            Assert.IsTrue(svc.IsRewardedLoaded);
        }

        [Test]
        public void Rewarded_WhenSimulateLoaded_ShowReturnsCompleted()
        {
            var svc = new NullAdService { SimulateLoaded = true, SimulateResult = AdResult.Completed };
            var result = svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(AdResult.Completed, result);
        }

        [Test]
        public void Rewarded_WhenSimulateLoadedAndSkipped_ShowReturnsSkipped()
        {
            var svc = new NullAdService { SimulateLoaded = true, SimulateResult = AdResult.Skipped };
            var result = svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(AdResult.Skipped, result);
        }

        // ── Rewarded — not loaded ────────────────────────────────────────────

        [Test]
        public void Rewarded_WhenNotSimulateLoaded_IsRewardedLoadedIsFalse()
        {
            var svc = new NullAdService { SimulateLoaded = false };
            Assert.IsFalse(svc.IsRewardedLoaded);
        }

        [Test]
        public void Rewarded_WhenNotSimulateLoaded_ShowReturnsNotLoaded()
        {
            var svc = new NullAdService { SimulateLoaded = false };
            var result = svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(AdResult.NotLoaded, result);
        }

        // ── Interstitial — loaded ────────────────────────────────────────────

        [Test]
        public void Interstitial_WhenSimulateLoaded_IsInterstitialLoadedIsTrue()
        {
            var svc = new NullAdService { SimulateLoaded = true };
            Assert.IsTrue(svc.IsInterstitialLoaded);
        }

        [Test]
        public void Interstitial_WhenSimulateLoaded_ShowReturnsCompleted()
        {
            var svc = new NullAdService { SimulateLoaded = true, SimulateResult = AdResult.Completed };
            var result = svc.ShowInterstitialAsync().GetAwaiter().GetResult();
            Assert.AreEqual(AdResult.Completed, result);
        }

        // ── Interstitial — not loaded ────────────────────────────────────────

        [Test]
        public void Interstitial_WhenNotSimulateLoaded_IsInterstitialLoadedIsFalse()
        {
            var svc = new NullAdService { SimulateLoaded = false };
            Assert.IsFalse(svc.IsInterstitialLoaded);
        }

        [Test]
        public void Interstitial_WhenNotSimulateLoaded_ShowReturnsNotLoaded()
        {
            var svc = new NullAdService { SimulateLoaded = false };
            var result = svc.ShowInterstitialAsync().GetAwaiter().GetResult();
            Assert.AreEqual(AdResult.NotLoaded, result);
        }

        // ── Analytics integration ────────────────────────────────────────────

        [Test]
        public void Rewarded_WhenCompleted_FiresImpressionAndCompleted()
        {
            var analytics = new MockAnalyticsService();
            var svc = new NullAdService { SimulateLoaded = true, SimulateResult = AdResult.Completed, Analytics = analytics };
            svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, analytics.AdImpressionCount);
            Assert.AreEqual(1, analytics.AdCompletedCount);
            Assert.AreEqual(0, analytics.AdSkippedCount);
            Assert.AreEqual("rewarded", analytics.LastAdType);
        }

        [Test]
        public void Rewarded_WhenSkipped_FiresImpressionAndSkipped()
        {
            var analytics = new MockAnalyticsService();
            var svc = new NullAdService { SimulateLoaded = true, SimulateResult = AdResult.Skipped, Analytics = analytics };
            svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, analytics.AdImpressionCount);
            Assert.AreEqual(0, analytics.AdCompletedCount);
            Assert.AreEqual(1, analytics.AdSkippedCount);
        }

        [Test]
        public void Rewarded_WhenNotLoaded_FiresFailedToLoad()
        {
            var analytics = new MockAnalyticsService();
            var svc = new NullAdService { SimulateLoaded = false, Analytics = analytics };
            svc.ShowRewardedAsync().GetAwaiter().GetResult();
            Assert.AreEqual(0, analytics.AdImpressionCount);
            Assert.AreEqual(1, analytics.AdFailedToLoadCount);
            Assert.AreEqual("rewarded", analytics.LastAdType);
        }

        [Test]
        public void Interstitial_WhenNotLoaded_FiresFailedToLoad()
        {
            var analytics = new MockAnalyticsService();
            var svc = new NullAdService { SimulateLoaded = false, Analytics = analytics };
            svc.ShowInterstitialAsync().GetAwaiter().GetResult();
            Assert.AreEqual(0, analytics.AdImpressionCount);
            Assert.AreEqual(1, analytics.AdFailedToLoadCount);
            Assert.AreEqual("interstitial", analytics.LastAdType);
        }

        // ── Initialize and Load are no-ops in NullAdService ─────────────────

        [Test]
        public void Initialize_DoesNotThrow()
        {
            var svc = new NullAdService();
            Assert.DoesNotThrow(() => svc.Initialize("ios-id", "android-id", testMode: true));
        }

        [Test]
        public void Load_DoesNotThrow()
        {
            var svc = new NullAdService();
            Assert.DoesNotThrow(() => svc.LoadRewarded());
            Assert.DoesNotThrow(() => svc.LoadInterstitial());
        }
    }
}
