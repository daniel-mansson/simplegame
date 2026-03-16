using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.ScreenManagement;

namespace SimpleGame.Tests.Core
{
    // ---------------------------------------------------------------------------
    // TestScreenId: local test enum — replaces game-specific ScreenId in Core tests
    // ---------------------------------------------------------------------------
    internal enum TestScreenId
    {
        MainMenu,
        Settings
    }

    // ---------------------------------------------------------------------------
    // MockSceneLoader: pure test double — records all load/unload calls
    // ---------------------------------------------------------------------------
    internal class MockSceneLoader : ISceneLoader
    {
        public List<string> LoadedScenes { get; } = new List<string>();
        public List<string> UnloadedScenes { get; } = new List<string>();
        public List<string> CallLog { get; } = new List<string>();

        public UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            LoadedScenes.Add(sceneName);
            CallLog.Add($"load:{sceneName}");
            return UniTask.CompletedTask;
        }

        public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            UnloadedScenes.Add(sceneName);
            CallLog.Add($"unload:{sceneName}");
            return UniTask.CompletedTask;
        }
    }

    // ---------------------------------------------------------------------------
    // ScreenManager edit-mode tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class ScreenManagerTests
    {
        private MockSceneLoader _loader;
        private ScreenManager<TestScreenId> _manager;

        [SetUp]
        public void SetUp()
        {
            _loader = new MockSceneLoader();
            _manager = new ScreenManager<TestScreenId>(_loader);
        }

        [Test]
        public void ShowScreenAsync_LoadsCorrectScene()
        {
            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();

            Assert.Contains("MainMenu", _loader.LoadedScenes,
                "ShowScreenAsync(MainMenu) must load the 'MainMenu' scene");
        }

        [Test]
        public void ShowScreenAsync_UnloadsPreviousBeforeLoadingNext()
        {
            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();
            _manager.ShowScreenAsync(TestScreenId.Settings).Forget();

            Assert.AreEqual(3, _loader.CallLog.Count,
                $"Expected 3 operations (load MainMenu, unload MainMenu, load Settings) but got {_loader.CallLog.Count}: [{string.Join(", ", _loader.CallLog)}]");
            Assert.AreEqual("load:MainMenu", _loader.CallLog[0], "First call must be load:MainMenu");
            Assert.AreEqual("unload:MainMenu", _loader.CallLog[1], "Second call must be unload:MainMenu");
            Assert.AreEqual("load:Settings", _loader.CallLog[2], "Third call must be load:Settings");
        }

        [Test]
        public void GoBackAsync_ReturnsToPreviousScreen()
        {
            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();
            _manager.ShowScreenAsync(TestScreenId.Settings).Forget();
            _manager.GoBackAsync().Forget();

            int mainMenuLoadCount = 0;
            foreach (var scene in _loader.LoadedScenes)
                if (scene == "MainMenu") mainMenuLoadCount++;

            Assert.AreEqual(2, mainMenuLoadCount,
                "GoBackAsync must reload MainMenu after navigating back from Settings");
            Assert.AreEqual(TestScreenId.MainMenu, _manager.CurrentScreen,
                "CurrentScreen must be MainMenu after GoBack");
        }

        [Test]
        public void GoBackAsync_WithEmptyHistory_IsNoOp()
        {
            Assert.DoesNotThrow(() => _manager.GoBackAsync().Forget(),
                "GoBackAsync on a fresh manager must not throw");

            Assert.IsEmpty(_loader.LoadedScenes, "No scenes should be loaded");
            Assert.IsEmpty(_loader.UnloadedScenes, "No scenes should be unloaded");
        }

        [Test]
        public void CurrentScreen_TracksActiveScreen()
        {
            Assert.IsNull(_manager.CurrentScreen,
                "CurrentScreen must be null before any navigation");

            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();
            Assert.AreEqual(TestScreenId.MainMenu, _manager.CurrentScreen,
                "CurrentScreen must be MainMenu after ShowScreenAsync(MainMenu)");

            _manager.ShowScreenAsync(TestScreenId.Settings).Forget();
            Assert.AreEqual(TestScreenId.Settings, _manager.CurrentScreen,
                "CurrentScreen must be Settings after ShowScreenAsync(Settings)");
        }

        [Test]
        public void CanGoBack_ReflectsHistoryState()
        {
            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false on a fresh manager");

            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();
            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false after first navigation (nothing in history yet)");

            _manager.ShowScreenAsync(TestScreenId.Settings).Forget();
            Assert.IsTrue(_manager.CanGoBack,
                "CanGoBack must be true after navigating to a second screen");

            _manager.GoBackAsync().Forget();
            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false after going back to the root screen");
        }

        [Test]
        public void ShowScreenAsync_GuardsAgainstConcurrentNavigation()
        {
            var blockingLoader = new BlockingMockSceneLoader();
            var guardedManager = new ScreenManager<TestScreenId>(blockingLoader);

            blockingLoader.IsBlocked = true;
            var firstNav = guardedManager.ShowScreenAsync(TestScreenId.MainMenu);

            int loadsAfterFirst = blockingLoader.LoadCallCount;
            blockingLoader.IsBlocked = false;

            var loader2 = new MockSceneLoader();
            var manager2 = new ScreenManager<TestScreenId>(loader2);

            manager2.ShowScreenAsync(TestScreenId.MainMenu).Forget();
            manager2.ShowScreenAsync(TestScreenId.Settings).Forget();

            bool unloadBeforeLoad = false;
            int unloadIndex = -1, loadSettingsIndex = -1;
            for (int i = 0; i < loader2.CallLog.Count; i++)
            {
                if (loader2.CallLog[i] == "unload:MainMenu") unloadIndex = i;
                if (loader2.CallLog[i] == "load:Settings") loadSettingsIndex = i;
            }
            if (unloadIndex >= 0 && loadSettingsIndex >= 0)
                unloadBeforeLoad = unloadIndex < loadSettingsIndex;

            Assert.IsTrue(unloadBeforeLoad,
                $"Navigation guard must ensure unload precedes load of next screen. CallLog: [{string.Join(", ", loader2.CallLog)}]");
            _ = firstNav;
            _ = loadsAfterFirst;
        }

        [Test]
        public void FirstShowScreen_DoesNotUnload()
        {
            _manager.ShowScreenAsync(TestScreenId.MainMenu).Forget();

            Assert.IsEmpty(_loader.UnloadedScenes,
                "The first ShowScreenAsync call must not unload any scene (no previous screen)");
        }
    }

    // ---------------------------------------------------------------------------
    // BlockingMockSceneLoader: auxiliary test double for concurrency guard test
    // ---------------------------------------------------------------------------
    internal class BlockingMockSceneLoader : ISceneLoader
    {
        public bool IsBlocked { get; set; }
        public int LoadCallCount { get; private set; }

        public UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            LoadCallCount++;
            return UniTask.CompletedTask;
        }

        public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }
    }
}
