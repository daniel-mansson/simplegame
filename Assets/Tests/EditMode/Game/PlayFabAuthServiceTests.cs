using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for <see cref="IPlayFabAuthService"/> contract and
    /// <see cref="MockPlayFabAuthService"/> mock fidelity.
    ///
    /// <see cref="PlayFabAuthService"/> makes real network calls and cannot be
    /// tested in edit-mode without a configured Title ID and network access.
    /// These tests verify the interface contract via a mock and ensure that
    /// downstream code written against <see cref="IPlayFabAuthService"/> behaves
    /// correctly in both success and failure scenarios.
    /// </summary>
    public class PlayFabAuthServiceTests
    {
        private const string TestPlayFabId = "TESTPLAYFABID001";
        private const string PlayerIdPrefsKey = "PlayFab_PlayerId";

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(PlayerIdPrefsKey);
        }

        // ── Mock fidelity ────────────────────────────────────────────────────

        [Test]
        public void MockAuth_InitialState_NotLoggedIn()
        {
            var mock = new MockPlayFabAuthService();
            Assert.IsFalse(mock.IsLoggedIn);
            Assert.AreEqual(string.Empty, mock.PlayFabId);
        }

        [Test]
        public void MockAuth_LoginSuccess_SetsIsLoggedIn()
        {
            var mock = new MockPlayFabAuthService { ShouldSucceed = true, FakePlayFabId = TestPlayFabId };
            mock.LoginAsync().Forget();
            Assert.IsTrue(mock.IsLoggedIn);
            Assert.AreEqual(TestPlayFabId, mock.PlayFabId);
        }

        [Test]
        public void MockAuth_LoginFailure_ThrowsPlayFabLoginException()
        {
            var mock = new MockPlayFabAuthService { ShouldSucceed = false };
            Assert.Throws<PlayFabLoginException>(() => mock.LoginAsync());
            Assert.IsFalse(mock.IsLoggedIn);
        }

        [Test]
        public void MockAuth_LoginCalledMultipleTimes_ReturnsSameId()
        {
            var mock = new MockPlayFabAuthService { ShouldSucceed = true, FakePlayFabId = TestPlayFabId };
            mock.LoginAsync().Forget();
            mock.LoginAsync().Forget();
            Assert.AreEqual(TestPlayFabId, mock.PlayFabId, "PlayFabId should remain stable across multiple login calls");
            Assert.AreEqual(2, mock.LoginCallCount, "Both login calls should be counted");
        }

        // ── Contract: offline-mode guard pattern ─────────────────────────────

        [Test]
        public void OfflineGuard_WhenNotLoggedIn_ServiceReportsNotLoggedIn()
        {
            IPlayFabAuthService auth = new MockPlayFabAuthService { ShouldSucceed = false };
            // Simulate a service that checks IsLoggedIn before doing cloud work
            bool wouldAttemptCloudOp = auth.IsLoggedIn;
            Assert.IsFalse(wouldAttemptCloudOp, "Cloud operations should be skipped when not logged in");
        }

        [Test]
        public void OfflineGuard_WhenLoggedIn_ServiceReportsLoggedIn()
        {
            IPlayFabAuthService auth = new MockPlayFabAuthService { ShouldSucceed = true, FakePlayFabId = TestPlayFabId };
            auth.LoginAsync().Forget();
            Assert.IsTrue(auth.IsLoggedIn);
            Assert.IsFalse(string.IsNullOrEmpty(auth.PlayFabId));
        }
    }

    /// <summary>
    /// Synchronous mock for <see cref="IPlayFabAuthService"/>. Used in edit-mode tests
    /// and any downstream presenter or service that receives auth via DI.
    /// </summary>
    public class MockPlayFabAuthService : IPlayFabAuthService
    {
        public bool ShouldSucceed { get; set; } = true;
        public string FakePlayFabId { get; set; } = "MOCK_PLAYFAB_ID";
        public int LoginCallCount { get; private set; }

        public bool IsLoggedIn { get; private set; }
        public string PlayFabId { get; private set; } = string.Empty;

        public UniTask LoginAsync()
        {
            LoginCallCount++;
            if (!ShouldSucceed)
                throw new PlayFabLoginException("Mock login failure", PlayFab.PlayFabErrorCode.NotAuthenticated);

            PlayFabId = FakePlayFabId;
            IsLoggedIn = true;
            return UniTask.CompletedTask;
        }
    }
}
