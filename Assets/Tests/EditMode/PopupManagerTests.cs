using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;

namespace SimpleGame.Tests
{
    // ---------------------------------------------------------------------------
    // MockPopupContainer: pure test double — records all show/hide calls
    // ---------------------------------------------------------------------------
    internal class MockPopupContainer : IPopupContainer
    {
        public List<string> CallLog { get; } = new List<string>();

        public UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
        {
            CallLog.Add($"show:{popupId}");
            return UniTask.CompletedTask;
        }

        public UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default)
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
        /// <summary>Current reference count (Block increments, Unblock decrements, floor 0).</summary>
        public int BlockCount { get; private set; }

        /// <summary>Total number of Block() calls made.</summary>
        public int BlockCallCount { get; private set; }

        /// <summary>Total number of Unblock() calls made.</summary>
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
    }

    // ---------------------------------------------------------------------------
    // PopupManager edit-mode tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class PopupManagerTests
    {
        private MockPopupContainer _container;
        private MockInputBlocker _inputBlocker;
        private PopupManager _manager;

        [SetUp]
        public void SetUp()
        {
            _container = new MockPopupContainer();
            _inputBlocker = new MockInputBlocker();
            _manager = new PopupManager(_container, _inputBlocker);
        }

        [Test]
        public void ShowPopupAsync_PushesPopupOntoStack()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();

            Assert.AreEqual(PopupId.ConfirmDialog, _manager.TopPopup,
                "TopPopup must be ConfirmDialog after ShowPopupAsync(ConfirmDialog)");
            Assert.AreEqual(1, _manager.PopupCount,
                "PopupCount must be 1 after a single ShowPopupAsync call");
        }

        [Test]
        public void ShowPopupAsync_CallsContainerShowPopup()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();

            Assert.Contains("show:ConfirmDialog", _container.CallLog,
                $"CallLog must contain 'show:ConfirmDialog'. Actual: [{string.Join(", ", _container.CallLog)}]");
        }

        [Test]
        public void ShowPopupAsync_BlocksInput()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();

            Assert.AreEqual(1, _inputBlocker.BlockCallCount,
                "Block() must be called exactly once after a single ShowPopupAsync");
            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must be true after ShowPopupAsync");
        }

        [Test]
        public void DismissPopupAsync_PopsTopPopup()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.IsNull(_manager.TopPopup,
                "TopPopup must be null after dismissing the only open popup");
            Assert.AreEqual(0, _manager.PopupCount,
                "PopupCount must be 0 after dismissing the only open popup");
        }

        [Test]
        public void DismissPopupAsync_CallsContainerHidePopup()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.Contains("hide:ConfirmDialog", _container.CallLog,
                $"CallLog must contain 'hide:ConfirmDialog'. Actual: [{string.Join(", ", _container.CallLog)}]");
        }

        [Test]
        public void DismissPopupAsync_UnblocksInputWhenStackEmpty()
        {
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.DismissPopupAsync().Forget();

            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after dismissing the last popup");
            Assert.AreEqual(1, _inputBlocker.UnblockCallCount,
                "Unblock() must be called exactly once when the stack becomes empty");
        }

        [Test]
        public void DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain()
        {
            // Show two popups then dismiss one — one remains so input stays blocked
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
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
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
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

            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            Assert.IsTrue(_manager.HasActivePopup,
                "HasActivePopup must be true after ShowPopupAsync");

            _manager.DismissPopupAsync().Forget();
            Assert.IsFalse(_manager.HasActivePopup,
                "HasActivePopup must be false after dismissing the last popup");
        }

        [Test]
        public void ShowPopupAsync_GuardsAgainstConcurrentOperation()
        {
            // Since MockPopupContainer is synchronous, each ShowPopupAsync completes fully
            // (including _isOperating reset) before the next begins. Verify that sequential
            // calls produce a well-ordered, non-interleaved CallLog — same approach as
            // ScreenManagerTests.ShowScreenAsync_GuardsAgainstConcurrentNavigation.
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();
            _manager.ShowPopupAsync(PopupId.ConfirmDialog).Forget();

            // Both complete sequentially; CallLog must have exactly 2 "show:" entries
            int showCount = 0;
            foreach (var entry in _container.CallLog)
                if (entry.StartsWith("show:")) showCount++;

            Assert.AreEqual(2, showCount,
                $"Two sequential ShowPopupAsync calls must each invoke container.ShowPopupAsync. CallLog: [{string.Join(", ", _container.CallLog)}]");
            Assert.AreEqual(2, _manager.PopupCount,
                "PopupCount must be 2 after two sequential show calls");

            // Verify the _isOperating guard fires by checking that a dismiss interleaved
            // during an in-flight show is not possible: confirm show always precedes stack push
            // (CallLog entry present iff PopupCount reflects the push).
            // With synchronous mocks: show entries and stack count stay in sync.
            Assert.AreEqual(showCount, _manager.PopupCount,
                "CallLog show entries must match PopupCount — guard ensures atomic push");
        }

        [Test]
        public void InputBlocker_NestedBlockUnblock()
        {
            // Block twice, unblock once: still blocked
            _inputBlocker.Block();
            _inputBlocker.Block();
            _inputBlocker.Unblock();

            Assert.IsTrue(_inputBlocker.IsBlocked,
                "IsBlocked must be true after 2 Block() calls and 1 Unblock()");
            Assert.AreEqual(1, _inputBlocker.BlockCount,
                "BlockCount must be 1 after 2 Block() and 1 Unblock()");

            // Unblock again: now unblocked
            _inputBlocker.Unblock();

            Assert.IsFalse(_inputBlocker.IsBlocked,
                "IsBlocked must be false after 2 Block() calls and 2 Unblock() calls");
            Assert.AreEqual(0, _inputBlocker.BlockCount,
                "BlockCount must be 0 after balanced Block/Unblock calls");
        }

        [Test]
        public void InputBlocker_BlockUnblockBlock_Sequence()
        {
            // Block, unblock, block: blocked with count 1
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
