using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for analytics service contract.
    /// </summary>
    public class AnalyticsServiceTests
    {
        // ── MockAnalyticsService ─────────────────────────────────────────────

        [Test]
        public void MockAnalytics_TrackSessionStart_RecordsEvent()
        {
            var mock = new MockAnalyticsService();
            mock.TrackSessionStart();
            Assert.AreEqual(1, mock.SessionStartCount);
        }

        [Test]
        public void MockAnalytics_TrackSessionEnd_RecordsEvent()
        {
            var mock = new MockAnalyticsService();
            mock.TrackSessionEnd();
            Assert.AreEqual(1, mock.SessionEndCount);
        }

        [Test]
        public void MockAnalytics_TrackLevelStarted_RecordsLevelId()
        {
            var mock = new MockAnalyticsService();
            mock.TrackLevelStarted("5");
            Assert.AreEqual(1, mock.LevelStartedCount);
            Assert.AreEqual("5", mock.LastLevelId);
        }

        [Test]
        public void MockAnalytics_TrackLevelCompleted_RecordsLevelId()
        {
            var mock = new MockAnalyticsService();
            mock.TrackLevelCompleted("3");
            Assert.AreEqual(1, mock.LevelCompletedCount);
            Assert.AreEqual("3", mock.LastLevelId);
        }

        [Test]
        public void MockAnalytics_TrackLevelFailed_RecordsLevelId()
        {
            var mock = new MockAnalyticsService();
            mock.TrackLevelFailed("7");
            Assert.AreEqual(1, mock.LevelFailedCount);
        }

        [Test]
        public void MockAnalytics_TrackCurrencyEarned_RecordsCurrencyAndAmount()
        {
            var mock = new MockAnalyticsService();
            mock.TrackCurrencyEarned("coins", 100);
            Assert.AreEqual(1, mock.CurrencyEarnedCount);
            Assert.AreEqual("coins", mock.LastCurrency);
            Assert.AreEqual(100, mock.LastCurrencyAmount);
        }

        [Test]
        public void MockAnalytics_TrackCurrencySpent_RecordsCurrencyAndAmount()
        {
            var mock = new MockAnalyticsService();
            mock.TrackCurrencySpent("golden_pieces", 5);
            Assert.AreEqual(1, mock.CurrencySpentCount);
            Assert.AreEqual("golden_pieces", mock.LastCurrency);
            Assert.AreEqual(5, mock.LastCurrencyAmount);
        }

        [Test]
        public void MockAnalytics_TrackPlatformLinked_RecordsPlatform()
        {
            var mock = new MockAnalyticsService();
            mock.TrackPlatformLinked("GameCenter");
            Assert.AreEqual(1, mock.PlatformLinkedCount);
            Assert.AreEqual("GameCenter", mock.LastPlatform);
        }

        // ── PlayFabAnalyticsService offline guard ────────────────────────────

        [Test]
        public void PlayFabAnalytics_WhenNotLoggedIn_DoesNotThrow()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = false };
            // Not calling LoginAsync — IsLoggedIn stays false
            var analytics = new PlayFabAnalyticsService(auth);
            // Should not throw; should silently no-op
            Assert.DoesNotThrow(() => analytics.TrackSessionStart());
            Assert.DoesNotThrow(() => analytics.TrackLevelStarted("1"));
            Assert.DoesNotThrow(() => analytics.TrackCurrencyEarned("coins", 100));
        }
    }

    /// <summary>
    /// Synchronous mock for <see cref="IAnalyticsService"/>. Tracks call counts and last values.
    /// </summary>
    public class MockAnalyticsService : IAnalyticsService
    {
        public int SessionStartCount   { get; private set; }
        public int SessionEndCount     { get; private set; }
        public int LevelStartedCount   { get; private set; }
        public int LevelCompletedCount { get; private set; }
        public int LevelFailedCount    { get; private set; }
        public int CurrencyEarnedCount { get; private set; }
        public int CurrencySpentCount  { get; private set; }
        public int PlatformLinkedCount { get; private set; }

        public string LastLevelId        { get; private set; }
        public string LastCurrency       { get; private set; }
        public int    LastCurrencyAmount { get; private set; }
        public string LastPlatform       { get; private set; }

        public void TrackSessionStart()                          => SessionStartCount++;
        public void TrackSessionEnd()                            => SessionEndCount++;
        public void TrackLevelStarted(string levelId)            { LevelStartedCount++;   LastLevelId = levelId; }
        public void TrackLevelCompleted(string levelId)          { LevelCompletedCount++; LastLevelId = levelId; }
        public void TrackLevelFailed(string levelId)             { LevelFailedCount++;    LastLevelId = levelId; }
        public void TrackPlatformLinked(string platform)         { PlatformLinkedCount++; LastPlatform = platform; }

        public void TrackCurrencyEarned(string currency, int amount)
        {
            CurrencyEarnedCount++;
            LastCurrency = currency;
            LastCurrencyAmount = amount;
        }

        public void TrackCurrencySpent(string currency, int amount)
        {
            CurrencySpentCount++;
            LastCurrency = currency;
            LastCurrencyAmount = amount;
        }
    }
}
