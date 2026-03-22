using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for cloud save merge logic and service contract.
    /// </summary>
    public class CloudSaveServiceTests
    {
        // ── MetaSaveMerge.TakeMax ────────────────────────────────────────────

        [Test]
        public void TakeMax_BothNull_ReturnsEmpty()
        {
            var result = MetaSaveMerge.TakeMax(null, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.coins);
            Assert.AreEqual(0, result.goldenPieces);
            Assert.IsNotNull(result.objectProgress);
            Assert.AreEqual(0, result.objectProgress.Count);
        }

        [Test]
        public void TakeMax_LocalHigherCoins_TakesLocal()
        {
            var local = new MetaSaveData { coins = 500, goldenPieces = 0 };
            var cloud = new MetaSaveData { coins = 100, goldenPieces = 0 };
            var result = MetaSaveMerge.TakeMax(local, cloud);
            Assert.AreEqual(500, result.coins);
        }

        [Test]
        public void TakeMax_CloudHigherCoins_TakesCloud()
        {
            var local = new MetaSaveData { coins = 100, goldenPieces = 0 };
            var cloud = new MetaSaveData { coins = 500, goldenPieces = 0 };
            var result = MetaSaveMerge.TakeMax(local, cloud);
            Assert.AreEqual(500, result.coins);
        }

        [Test]
        public void TakeMax_GoldenPieces_TakesMax()
        {
            var local = new MetaSaveData { goldenPieces = 3 };
            var cloud = new MetaSaveData { goldenPieces = 7 };
            var result = MetaSaveMerge.TakeMax(local, cloud);
            Assert.AreEqual(7, result.goldenPieces);
        }

        [Test]
        public void TakeMax_ObjectProgress_TakesMaxStepsPerObject()
        {
            var local = new MetaSaveData
            {
                objectProgress = new List<ObjectProgress>
                {
                    new ObjectProgress { objectId = "Fountain", currentSteps = 3 },
                    new ObjectProgress { objectId = "Bench",    currentSteps = 1 }
                }
            };
            var cloud = new MetaSaveData
            {
                objectProgress = new List<ObjectProgress>
                {
                    new ObjectProgress { objectId = "Fountain", currentSteps = 5 },
                    new ObjectProgress { objectId = "Harbor",   currentSteps = 2 }
                }
            };
            var result = MetaSaveMerge.TakeMax(local, cloud);

            // Fountain: max(3, 5) = 5
            Assert.AreEqual(5, GetSteps(result, "Fountain"), "Fountain should be 5");
            // Bench: local only, keeps 1
            Assert.AreEqual(1, GetSteps(result, "Bench"), "Bench should be 1");
            // Harbor: cloud only, keeps 2
            Assert.AreEqual(2, GetSteps(result, "Harbor"), "Harbor should be 2");
        }

        [Test]
        public void TakeMax_LocalNull_UsesCloud()
        {
            var cloud = new MetaSaveData { coins = 200, goldenPieces = 5 };
            var result = MetaSaveMerge.TakeMax(null, cloud);
            Assert.AreEqual(200, result.coins);
            Assert.AreEqual(5, result.goldenPieces);
        }

        [Test]
        public void TakeMax_CloudNull_UsesLocal()
        {
            var local = new MetaSaveData { coins = 300, goldenPieces = 2 };
            var result = MetaSaveMerge.TakeMax(local, null);
            Assert.AreEqual(300, result.coins);
            Assert.AreEqual(2, result.goldenPieces);
        }

        [Test]
        public void TakeMax_SavedAt_IsUpdated()
        {
            long before = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1;
            var result = MetaSaveMerge.TakeMax(new MetaSaveData(), new MetaSaveData());
            Assert.GreaterOrEqual(result.savedAt, before, "savedAt should be set to current time");
        }

        // ── MockCloudSaveService ────────────────────────────────────────────

        [Test]
        public void MockCloudSave_Push_WhenNotLoggedIn_IsSkipped()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = false };
            var mock = new MockCloudSaveService(auth);
            mock.PushAsync(new MetaSaveData()).Forget();
            Assert.AreEqual(0, mock.PushCallCount, "Push should be skipped when not logged in");
        }

        [Test]
        public void MockCloudSave_Push_WhenLoggedIn_IsExecuted()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockCloudSaveService(auth);
            mock.PushAsync(new MetaSaveData { coins = 100 }).Forget();
            Assert.AreEqual(1, mock.PushCallCount);
            Assert.AreEqual(100, mock.LastPushedData.coins);
        }

        [Test]
        public void MockCloudSave_Pull_WhenNotLoggedIn_ReturnsNull()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = false };
            var mock = new MockCloudSaveService(auth);
            MetaSaveData result = null;
            mock.PullAsync().ContinueWith(d => result = d).Forget();
            Assert.IsNull(result, "Pull should return null when not logged in");
        }

        [Test]
        public void MockCloudSave_Pull_WhenLoggedIn_ReturnsStoredData()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var stored = new MetaSaveData { coins = 500 };
            var mock = new MockCloudSaveService(auth) { StoredData = stored };
            MetaSaveData result = null;
            mock.PullAsync().ContinueWith(d => result = d).Forget();
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.coins);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static int GetSteps(MetaSaveData data, string objectId)
        {
            foreach (var entry in data.objectProgress)
                if (entry.objectId == objectId) return entry.currentSteps;
            return 0;
        }
    }

    /// <summary>
    /// Synchronous mock for <see cref="ICloudSaveService"/>. Guards on IsLoggedIn.
    /// </summary>
    public class MockCloudSaveService : ICloudSaveService
    {
        private readonly IPlayFabAuthService _auth;

        public int PushCallCount { get; private set; }
        public MetaSaveData LastPushedData { get; private set; }
        public MetaSaveData StoredData { get; set; }

        public MockCloudSaveService(IPlayFabAuthService auth)
        {
            _auth = auth;
        }

        public UniTask PushAsync(MetaSaveData data)
        {
            if (!_auth.IsLoggedIn) return UniTask.CompletedTask;
            PushCallCount++;
            LastPushedData = data;
            StoredData = data;
            return UniTask.CompletedTask;
        }

        public UniTask<MetaSaveData> PullAsync()
        {
            if (!_auth.IsLoggedIn) return UniTask.FromResult<MetaSaveData>(null);
            return UniTask.FromResult(StoredData);
        }
    }
}
