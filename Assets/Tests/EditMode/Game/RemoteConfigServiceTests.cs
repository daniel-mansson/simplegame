using NUnit.Framework;
using SimpleGame.Game.Services;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for remote config service contract and fallback behaviour.
    /// </summary>
    public class RemoteConfigServiceTests
    {
        // ── Defaults ─────────────────────────────────────────────────────────

        [Test]
        public void Default_InitialHearts_IsThree()
        {
            Assert.AreEqual(3, GameRemoteConfig.Default.InitialHearts);
        }

        [Test]
        public void Default_GoldenPiecesPerWin_IsFive()
        {
            Assert.AreEqual(5, GameRemoteConfig.Default.GoldenPiecesPerWin);
        }

        [Test]
        public void Default_ContinueCostCoins_IsOneHundred()
        {
            Assert.AreEqual(100, GameRemoteConfig.Default.ContinueCostCoins);
        }

        [Test]
        public void Default_InterstitialEveryNLevels_IsThree()
        {
            Assert.AreEqual(3, GameRemoteConfig.Default.InterstitialEveryNLevels);
        }

        // ── MockRemoteConfigService ──────────────────────────────────────────

        [Test]
        public void Mock_BeforeFetch_ReturnsDefaults()
        {
            var mock = new MockRemoteConfigService();
            Assert.AreEqual(GameRemoteConfig.Default.InitialHearts,      mock.Config.InitialHearts);
            Assert.AreEqual(GameRemoteConfig.Default.GoldenPiecesPerWin, mock.Config.GoldenPiecesPerWin);
            Assert.AreEqual(GameRemoteConfig.Default.ContinueCostCoins,  mock.Config.ContinueCostCoins);
        }

        [Test]
        public void Mock_AfterFetch_ReturnsOverriddenValues()
        {
            var mock = new MockRemoteConfigService
            {
                Override = new GameRemoteConfig
                {
                    InitialHearts      = 5,
                    GoldenPiecesPerWin = 10,
                    ContinueCostCoins  = 50,
                }
            };
            mock.FetchAsync().Forget();
            Assert.AreEqual(5,   mock.Config.InitialHearts);
            Assert.AreEqual(10,  mock.Config.GoldenPiecesPerWin);
            Assert.AreEqual(50,  mock.Config.ContinueCostCoins);
        }

        [Test]
        public void Mock_FetchCount_IncrementedEachCall()
        {
            var mock = new MockRemoteConfigService();
            mock.FetchAsync().Forget();
            mock.FetchAsync().Forget();
            Assert.AreEqual(2, mock.FetchCallCount);
        }

        // ── PlayFabRemoteConfigService offline guard ─────────────────────────

        [Test]
        public void PlayFabRemoteConfig_WhenNotLoggedIn_ReturnsDefaults()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = false };
            // Not calling LoginAsync — IsLoggedIn stays false
            var svc = new PlayFabRemoteConfigService(auth);
            svc.FetchAsync().Forget();
            // Should not throw and should return defaults
            Assert.AreEqual(GameRemoteConfig.Default.InitialHearts,      svc.Config.InitialHearts);
            Assert.AreEqual(GameRemoteConfig.Default.GoldenPiecesPerWin, svc.Config.GoldenPiecesPerWin);
            Assert.AreEqual(GameRemoteConfig.Default.ContinueCostCoins,  svc.Config.ContinueCostCoins);
        }
    }

    /// <summary>
    /// Synchronous mock for <see cref="IRemoteConfigService"/>.
    /// Set <see cref="Override"/> before calling FetchAsync to override config values.
    /// </summary>
    public class MockRemoteConfigService : IRemoteConfigService
    {
        public GameRemoteConfig Config { get; private set; } = GameRemoteConfig.Default;
        public GameRemoteConfig? Override { get; set; }
        public int FetchCallCount { get; private set; }

        public UniTask FetchAsync()
        {
            FetchCallCount++;
            if (Override.HasValue)
                Config = Override.Value;
            return UniTask.CompletedTask;
        }
    }
}
