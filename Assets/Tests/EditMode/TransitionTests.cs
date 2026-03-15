using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.ScreenManagement;
using SimpleGame.Core.TransitionManagement;

namespace SimpleGame.Tests
{
    // ---------------------------------------------------------------------------
    // MockTransitionPlayer: records "fadeOut"/"fadeIn" in a shared CallLog
    // ---------------------------------------------------------------------------
    internal class MockTransitionPlayer : ITransitionPlayer
    {
        /// <summary>Ordered record of all FadeOutAsync / FadeInAsync calls made.</summary>
        public List<string> CallLog { get; } = new List<string>();

        public UniTask FadeOutAsync(CancellationToken ct = default)
        {
            CallLog.Add("fadeOut");
            return UniTask.CompletedTask;
        }

        public UniTask FadeInAsync(CancellationToken ct = default)
        {
            CallLog.Add("fadeIn");
            return UniTask.CompletedTask;
        }
    }

    // ---------------------------------------------------------------------------
    // ThrowingSceneLoader: throws on LoadSceneAdditiveAsync for exception tests
    // ---------------------------------------------------------------------------
    internal class ThrowingSceneLoader : ISceneLoader
    {
        public List<string> CallLog { get; } = new List<string>();

        public UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            CallLog.Add($"load:{sceneName}");
            throw new InvalidOperationException("Simulated load failure");
        }

        public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            CallLog.Add($"unload:{sceneName}");
            return UniTask.CompletedTask;
        }
    }

    // ---------------------------------------------------------------------------
    // Transition orchestration tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class TransitionTests
    {
        private MockSceneLoader _loader;
        private MockTransitionPlayer _transition;
        private MockInputBlocker _inputBlocker;
        private ScreenManager _manager;

        [SetUp]
        public void SetUp()
        {
            _loader = new MockSceneLoader();
            _transition = new MockTransitionPlayer();
            _inputBlocker = new MockInputBlocker();
            _manager = new ScreenManager(_loader, _transition, _inputBlocker);
        }

        // -----------------------------------------------------------------------
        // Test 1: Correct call ordering — fadeOut before unload, fadeIn after load
        // -----------------------------------------------------------------------
        [Test]
        public void ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad()
        {
            // Navigate to MainMenu first so there is a screen to unload on next nav
            _manager.ShowScreenAsync(ScreenId.MainMenu).Forget();

            // Reset logs to isolate the second navigation
            _transition.CallLog.Clear();
            _loader.CallLog.Clear();

            _manager.ShowScreenAsync(ScreenId.Settings).Forget();

            // Build a merged call log: transition events + scene loader events in order
            // Because all mocks are synchronous, the interleaved order is deterministic.
            // Expected: fadeOut → unload:MainMenu → load:Settings → fadeIn
            var merged = new List<string>(_transition.CallLog);
            // We need to verify ordering by inspecting the combined sequence.
            // Use a combined log that records all operations in order.
            // Re-run with a merged-log approach:
            var loader2 = new MockSceneLoader();
            var mergedLog = new List<string>();

            var transition2 = new MergedLogTransitionPlayer(mergedLog);
            var loaderWrapper = new MergedLogSceneLoader(mergedLog, loader2);

            var manager2 = new ScreenManager(loaderWrapper, transition2, new MockInputBlocker());
            manager2.ShowScreenAsync(ScreenId.MainMenu).Forget();
            mergedLog.Clear(); // clear first nav

            manager2.ShowScreenAsync(ScreenId.Settings).Forget();

            Assert.AreEqual(4, mergedLog.Count,
                $"Expected 4 operations (fadeOut, unload:MainMenu, load:Settings, fadeIn) but got {mergedLog.Count}: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("fadeOut",           mergedLog[0], $"[0] must be fadeOut. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("unload:MainMenu",   mergedLog[1], $"[1] must be unload:MainMenu. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("load:Settings",     mergedLog[2], $"[2] must be load:Settings. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("fadeIn",            mergedLog[3], $"[3] must be fadeIn. Log: [{string.Join(", ", mergedLog)}]");
        }

        // -----------------------------------------------------------------------
        // Test 2: Input is blocked for the duration and unblocked after completion
        // -----------------------------------------------------------------------
        [Test]
        public void ShowScreenAsync_WithTransition_BlocksAndUnblocksInput()
        {
            _manager.ShowScreenAsync(ScreenId.MainMenu).Forget();

            Assert.AreEqual(1, _inputBlocker.BlockCallCount,
                "Block() must be called exactly once during transition navigation");
            Assert.AreEqual(1, _inputBlocker.UnblockCallCount,
                "Unblock() must be called exactly once after transition navigation completes");
            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after navigation completes (balanced Block/Unblock)");
        }

        // -----------------------------------------------------------------------
        // Test 3: GoBackAsync plays the same orchestration sequence
        // -----------------------------------------------------------------------
        [Test]
        public void GoBackAsync_WithTransition_PlaysFullTransitionSequence()
        {
            var mergedLog = new List<string>();
            var transition = new MergedLogTransitionPlayer(mergedLog);
            var loaderBase = new MockSceneLoader();
            var loader = new MergedLogSceneLoader(mergedLog, loaderBase);
            var inputBlocker = new MockInputBlocker();

            var manager = new ScreenManager(loader, transition, inputBlocker);

            // Navigate to MainMenu, then Settings (so GoBack has somewhere to return to)
            manager.ShowScreenAsync(ScreenId.MainMenu).Forget();
            manager.ShowScreenAsync(ScreenId.Settings).Forget();
            mergedLog.Clear();
            int blockCountBeforeBack = inputBlocker.BlockCallCount;
            int unblockCountBeforeBack = inputBlocker.UnblockCallCount;

            manager.GoBackAsync().Forget();

            // Expected: fadeOut → unload:Settings → load:MainMenu → fadeIn
            Assert.AreEqual(4, mergedLog.Count,
                $"GoBackAsync must produce 4 operations (fadeOut, unload:Settings, load:MainMenu, fadeIn). Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("fadeOut",         mergedLog[0], $"[0] must be fadeOut. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("unload:Settings", mergedLog[1], $"[1] must be unload:Settings. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("load:MainMenu",   mergedLog[2], $"[2] must be load:MainMenu. Log: [{string.Join(", ", mergedLog)}]");
            Assert.AreEqual("fadeIn",          mergedLog[3], $"[3] must be fadeIn. Log: [{string.Join(", ", mergedLog)}]");

            Assert.AreEqual(blockCountBeforeBack + 1, inputBlocker.BlockCallCount,
                "GoBackAsync must call Block() once");
            Assert.AreEqual(unblockCountBeforeBack + 1, inputBlocker.UnblockCallCount,
                "GoBackAsync must call Unblock() once");
            Assert.IsFalse(inputBlocker.IsBlocked,
                "IsBlocked must be false after GoBackAsync completes");
        }

        // -----------------------------------------------------------------------
        // Test 4: Null transition player — identical behavior to original
        // -----------------------------------------------------------------------
        [Test]
        public void ShowScreenAsync_WithoutTransition_BehavesIdentically()
        {
            var loader = new MockSceneLoader();
            var manager = new ScreenManager(loader); // null transition player

            manager.ShowScreenAsync(ScreenId.MainMenu).Forget();
            manager.ShowScreenAsync(ScreenId.Settings).Forget();

            // Original behavior: load:MainMenu, unload:MainMenu, load:Settings
            Assert.AreEqual(3, loader.CallLog.Count,
                $"Without transition player, call log must match original behavior. Log: [{string.Join(", ", loader.CallLog)}]");
            Assert.AreEqual("load:MainMenu",   loader.CallLog[0]);
            Assert.AreEqual("unload:MainMenu", loader.CallLog[1]);
            Assert.AreEqual("load:Settings",   loader.CallLog[2]);

            // No transition or input-blocker side-effects
            Assert.AreEqual(0, _transition.CallLog.Count,
                "Transition player must not be called when ScreenManager has null transition");
            Assert.AreEqual(0, _inputBlocker.BlockCallCount,
                "Input blocker must not be called when ScreenManager has null transition");
        }

        // -----------------------------------------------------------------------
        // Test 5: Exception during load — Unblock() still called (finally block)
        // -----------------------------------------------------------------------
        [Test]
        public void ShowScreenAsync_WithTransition_UnblocksInputOnException()
        {
            var throwingLoader = new ThrowingSceneLoader();
            var transition = new MockTransitionPlayer();
            var inputBlocker = new MockInputBlocker();
            var manager = new ScreenManager(throwingLoader, transition, inputBlocker);

            // The load throws; UniTask exception will be swallowed by Forget() but the
            // synchronous execution up to the throw + finally still runs.
            Assert.Throws<InvalidOperationException>(() =>
                manager.ShowScreenAsync(ScreenId.MainMenu).GetAwaiter().GetResult(),
                "ShowScreenAsync must propagate the InvalidOperationException from the scene loader");

            Assert.AreEqual(1, inputBlocker.BlockCallCount,
                "Block() must have been called before the load attempt");
            Assert.AreEqual(1, inputBlocker.UnblockCallCount,
                $"Unblock() must be called even when the scene loader throws (finally block). BlockCallCount={inputBlocker.BlockCallCount}, UnblockCallCount={inputBlocker.UnblockCallCount}");
            Assert.IsFalse(inputBlocker.IsBlocked,
                "IsBlocked must be false after exception — input must not stay permanently blocked");
        }
    }

    // ---------------------------------------------------------------------------
    // Helpers: merged-log wrappers so a single ordered list captures all events
    // ---------------------------------------------------------------------------

    internal class MergedLogTransitionPlayer : ITransitionPlayer
    {
        private readonly List<string> _log;
        public MergedLogTransitionPlayer(List<string> log) => _log = log;

        public UniTask FadeOutAsync(CancellationToken ct = default)
        {
            _log.Add("fadeOut");
            return UniTask.CompletedTask;
        }

        public UniTask FadeInAsync(CancellationToken ct = default)
        {
            _log.Add("fadeIn");
            return UniTask.CompletedTask;
        }
    }

    internal class MergedLogSceneLoader : ISceneLoader
    {
        private readonly List<string> _log;
        private readonly MockSceneLoader _inner;

        public MergedLogSceneLoader(List<string> log, MockSceneLoader inner)
        {
            _log = log;
            _inner = inner;
        }

        public UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            _log.Add($"load:{sceneName}");
            return _inner.LoadSceneAdditiveAsync(sceneName, ct);
        }

        public UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            _log.Add($"unload:{sceneName}");
            return _inner.UnloadSceneAsync(sceneName, ct);
        }
    }
}
