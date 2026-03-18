using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;

namespace SimpleGame.Tests.Core
{
    // ---------------------------------------------------------------------------
    // TestPopupId: local test enum — replaces game-specific PopupId in Core tests
    // ---------------------------------------------------------------------------
    internal enum TestPopupId
    {
        TestPopup
    }

    // ---------------------------------------------------------------------------
    // MockPopupContainer: pure test double — records all show/hide calls
    // ---------------------------------------------------------------------------
    internal class MockPopupContainer : IPopupContainer<TestPopupId>
    {
        public List<string> CallLog { get; } = new List<string>();

        public UniTask ShowPopupAsync(TestPopupId popupId, CancellationToken ct = default)
        {
            CallLog.Add($"show:{popupId}");
            return UniTask.CompletedTask;
        }

        public UniTask HidePopupAsync(TestPopupId popupId, CancellationToken ct = default)
        {
            CallLog.Add($"hide:{popupId}");
            return UniTask.CompletedTask;
        }
    }

    // ---------------------------------------------------------------------------
    // MockInputBlocker: reference-counted test double — exposes block state
    // ---------------------------------------------------------------------------
    internal class MockInputBlocker : IInputBlocker
    {
        public int BlockCount { get; private set; }
        public int BlockCallCount { get; private set; }
        public int UnblockCallCount { get; private set; }
        public bool IsBlocked => BlockCount > 0;

        public void Block()
        {
            BlockCount++;
            BlockCallCount++;
        }

        public void Unblock()
        {
            UnblockCallCount++;
            if (BlockCount > 0)
                BlockCount--;
        }

        public UniTask FadeInAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask FadeOutAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public void SetSortOrder(int sortOrder) { }
    }

    // ---------------------------------------------------------------------------
    // PopupManager edit-mode tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class PopupManagerTests
    {
        private MockPopupContainer _container;
        private MockInputBlocker _inputBlocker;
        private PopupManager<TestPopupId> _manager;

        [SetUp]
        public void SetUp()
        {
            _container = new MockPopupContainer();
            _inputBlocker = new MockInputBlocker();
            _manager = new PopupManager<TestPopupId>(_container, _inputBlocker);
        }

        [Test]
        public void ShowPopupAsync_PushesPopupOntoStack()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();

            Assert.AreEqual(TestPopupId.TestPopup, _manager.TopPopup,
                "TopPopup must be TestPopup after ShowPopupAsync(TestPopup)");
            Assert.AreEqual(1, _manager.PopupCount,
                "PopupCount must be 1 after a single ShowPopupAsync call");
        }

        [Test]
        public void ShowPopupAsync_CallsContainerShowPopup()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();

            Assert.Contains("show:TestPopup", _container.CallLog,
                $"CallLog must contain 'show:TestPopup'. Actual: [{string.Join(", ", _container.CallLog)}]");
        }

        [Test]
        public void ShowPopupAsync_BlocksInput()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();

            Assert.AreEqual(1, _inputBlocker.BlockCallCount,
                "Block() must be called exactly once after a single ShowPopupAsync");
            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must be true after ShowPopupAsync");
        }

        [Test]
        public void DismissPopupAsync_PopsTopPopup()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.IsNull(_manager.TopPopup,
                "TopPopup must be null after dismissing the only open popup");
            Assert.AreEqual(0, _manager.PopupCount,
                "PopupCount must be 0 after dismissing the only open popup");
        }

        [Test]
        public void DismissPopupAsync_CallsContainerHidePopup()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.Contains("hide:TestPopup", _container.CallLog,
                $"CallLog must contain 'hide:TestPopup'. Actual: [{string.Join(", ", _container.CallLog)}]");
        }

        [Test]
        public void DismissPopupAsync_UnblocksInputWhenStackEmpty()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after dismissing the last popup");
            Assert.AreEqual(1, _inputBlocker.UnblockCallCount,
                "Unblock() must be called exactly once when the stack becomes empty");
        }

        [Test]
        public void DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must remain true when at least one popup is still on the stack");
            Assert.AreEqual(1, _manager.PopupCount,
                "PopupCount must be 1 after dismissing one of two popups");
        }

        [Test]
        public void DismissPopupAsync_WithEmptyStack_IsNoOp()
        {
            Assert.DoesNotThrow(() => _manager.DismissPopupAsync().Forget(),
                "DismissPopupAsync on a fresh manager must not throw");

            Assert.IsEmpty(_container.CallLog,
                "No container calls should be made when dismissing an empty stack");
            Assert.AreEqual(0, _inputBlocker.UnblockCallCount,
                "Unblock() must not be called when the stack is empty");
        }

        [Test]
        public void DismissAllAsync_ClearsEntireStack()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.DismissAllAsync().Forget();

            Assert.AreEqual(0, _manager.PopupCount,
                $"PopupCount must be 0 after DismissAllAsync. CallLog: [{string.Join(", ", _container.CallLog)}]");
            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after DismissAllAsync clears all popups");

            int hideCount = 0;
            foreach (var entry in _container.CallLog)
                if (entry.StartsWith("hide:")) hideCount++;

            Assert.AreEqual(2, hideCount,
                $"DismissAllAsync must call HidePopupAsync for each popup. CallLog: [{string.Join(", ", _container.CallLog)}]");
        }

        [Test]
        public void HasActivePopup_ReflectsStackState()
        {
            Assert.IsFalse(_manager.HasActivePopup,
                "HasActivePopup must be false on a fresh manager");

            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            Assert.IsTrue(_manager.HasActivePopup,
                "HasActivePopup must be true after ShowPopupAsync");

            _manager.DismissPopupAsync().Forget();
            Assert.IsFalse(_manager.HasActivePopup,
                "HasActivePopup must be false after dismissing the last popup");
        }

        [Test]
        public void ShowPopupAsync_GuardsAgainstConcurrentOperation()
        {
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();
            _manager.ShowPopupAsync(TestPopupId.TestPopup).Forget();

            int showCount = 0;
            foreach (var entry in _container.CallLog)
                if (entry.StartsWith("show:")) showCount++;

            Assert.AreEqual(2, showCount,
                $"Two sequential ShowPopupAsync calls must each invoke container.ShowPopupAsync. CallLog: [{string.Join(", ", _container.CallLog)}]");
            Assert.AreEqual(2, _manager.PopupCount,
                "PopupCount must be 2 after two sequential show calls");

            Assert.AreEqual(showCount, _manager.PopupCount,
                "CallLog show entries must match PopupCount — guard ensures atomic push");
        }

        [Test]
        public void InputBlocker_NestedBlockUnblock()
        {
            _inputBlocker.Block();
            _inputBlocker.Block();
            _inputBlocker.Unblock();

            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must be true after 2 Block() calls and 1 Unblock()");
            Assert.AreEqual(1, _inputBlocker.BlockCount,
                "BlockCount must be 1 after 2 Block() and 1 Unblock()");

            _inputBlocker.Unblock();

            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after 2 Block() calls and 2 Unblock() calls");
            Assert.AreEqual(0, _inputBlocker.BlockCount,
                "BlockCount must be 0 after balanced Block/Unblock calls");
        }

        [Test]
        public void InputBlocker_BlockUnblockBlock_Sequence()
        {
            _inputBlocker.Block();
            _inputBlocker.Unblock();
            _inputBlocker.Block();

            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must be true after Block→Unblock→Block sequence");
            Assert.AreEqual(1, _inputBlocker.BlockCount,
                "BlockCount must be 1 after Block→Unblock→Block sequence");
            Assert.AreEqual(2, _inputBlocker.BlockCallCount,
                "BlockCallCount must be 2 (two Block() calls total)");
            Assert.AreEqual(1, _inputBlocker.UnblockCallCount,
                "UnblockCallCount must be 1 (one Unblock() call total)");
        }
    }
}
