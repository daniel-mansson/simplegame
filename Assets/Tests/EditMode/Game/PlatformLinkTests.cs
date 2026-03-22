using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for platform linking service contract and first-launch prompt logic.
    /// </summary>
    public class PlatformLinkTests
    {
        private const string HasSeenKey = PlatformLinkPresenter.HasSeenLinkPromptKey;

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(HasSeenKey);
        }

        // ── MockPlatformLinkService ─────────────────────────────────────────

        [Test]
        public void MockLink_InitialState_NothingLinked()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            Assert.IsFalse(mock.IsGameCenterLinked);
            Assert.IsFalse(mock.IsGooglePlayLinked);
        }

        [Test]
        public void MockLink_LinkGameCenter_WhenLoggedIn_Succeeds()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            bool result = false;
            mock.LinkGameCenterAsync().ContinueWith(r => result = r).Forget();
            Assert.IsTrue(result);
            Assert.IsTrue(mock.IsGameCenterLinked);
        }

        [Test]
        public void MockLink_LinkGameCenter_WhenNotLoggedIn_ReturnsFalse()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = false };
            var mock = new MockPlatformLinkService(auth);
            bool result = true;
            mock.LinkGameCenterAsync().ContinueWith(r => result = r).Forget();
            Assert.IsFalse(result);
            Assert.IsFalse(mock.IsGameCenterLinked);
        }

        [Test]
        public void MockLink_LinkGooglePlay_WhenLoggedIn_Succeeds()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            bool result = false;
            mock.LinkGooglePlayAsync().ContinueWith(r => result = r).Forget();
            Assert.IsTrue(result);
            Assert.IsTrue(mock.IsGooglePlayLinked);
        }

        [Test]
        public void MockLink_UnlinkGameCenter_ClearsLinkedState()
        {
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            mock.LinkGameCenterAsync().Forget();
            Assert.IsTrue(mock.IsGameCenterLinked);
            mock.UnlinkGameCenterAsync().Forget();
            Assert.IsFalse(mock.IsGameCenterLinked);
        }

        // ── PlatformLinkPresenter.ShouldShow ────────────────────────────────

        [Test]
        public void ShouldShow_NeverSeenAndNothingLinked_ReturnsTrue()
        {
            PlayerPrefs.DeleteKey(HasSeenKey);
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            Assert.IsTrue(PlatformLinkPresenter.ShouldShow(mock));
        }

        [Test]
        public void ShouldShow_AlreadySeen_ReturnsFalse()
        {
            PlatformLinkPresenter.MarkSeen();
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            Assert.IsFalse(PlatformLinkPresenter.ShouldShow(mock));
        }

        [Test]
        public void ShouldShow_GameCenterAlreadyLinked_ReturnsFalse()
        {
            PlayerPrefs.DeleteKey(HasSeenKey);
            var auth = new MockPlayFabAuthService { ShouldSucceed = true };
            auth.LoginAsync().Forget();
            var mock = new MockPlatformLinkService(auth);
            mock.LinkGameCenterAsync().Forget();
            Assert.IsFalse(PlatformLinkPresenter.ShouldShow(mock),
                "Should not show if player already has a platform account linked");
        }

        [Test]
        public void MarkSeen_PersistsAcrossCalls()
        {
            PlayerPrefs.DeleteKey(HasSeenKey);
            Assert.AreEqual(0, PlayerPrefs.GetInt(HasSeenKey, 0));
            PlatformLinkPresenter.MarkSeen();
            Assert.AreEqual(1, PlayerPrefs.GetInt(HasSeenKey, 0));
        }
    }

    /// <summary>
    /// Synchronous mock for <see cref="IPlatformLinkService"/>.
    /// Guards on IsLoggedIn, tracks link state in memory.
    /// </summary>
    public class MockPlatformLinkService : IPlatformLinkService
    {
        private readonly IPlayFabAuthService _auth;

        public bool IsGameCenterLinked { get; private set; }
        public bool IsGooglePlayLinked { get; private set; }
        public int RefreshCallCount { get; private set; }

        public MockPlatformLinkService(IPlayFabAuthService auth)
        {
            _auth = auth;
        }

        public UniTask<bool> LinkGameCenterAsync()
        {
            if (!_auth.IsLoggedIn) return UniTask.FromResult(false);
            IsGameCenterLinked = true;
            return UniTask.FromResult(true);
        }

        public UniTask<bool> LinkGooglePlayAsync()
        {
            if (!_auth.IsLoggedIn) return UniTask.FromResult(false);
            IsGooglePlayLinked = true;
            return UniTask.FromResult(true);
        }

        public UniTask<bool> UnlinkGameCenterAsync()
        {
            if (!_auth.IsLoggedIn) return UniTask.FromResult(false);
            IsGameCenterLinked = false;
            return UniTask.FromResult(true);
        }

        public UniTask<bool> UnlinkGooglePlayAsync()
        {
            if (!_auth.IsLoggedIn) return UniTask.FromResult(false);
            IsGooglePlayLinked = false;
            return UniTask.FromResult(true);
        }

        public UniTask RefreshLinkStatusAsync()
        {
            RefreshCallCount++;
            return UniTask.CompletedTask;
        }
    }
}
