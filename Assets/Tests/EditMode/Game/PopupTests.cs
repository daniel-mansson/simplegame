using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Popup;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // Mock popup views
    // ---------------------------------------------------------------------------
    internal class MockWinDialogView : IWinDialogView
    {
        public event Action OnContinueClicked;
        public string LastScoreText { get; private set; }
        public string LastLevelText { get; private set; }

        public void UpdateScore(string text) => LastScoreText = text;
        public void UpdateLevel(string text) => LastLevelText = text;
        public void SimulateContinueClicked() => OnContinueClicked?.Invoke();
    }

    internal class MockLoseDialogView : ILoseDialogView
    {
        public event Action OnRetryClicked;
        public event Action OnBackClicked;
        public string LastScoreText { get; private set; }
        public string LastLevelText { get; private set; }

        public void UpdateScore(string text) => LastScoreText = text;
        public void UpdateLevel(string text) => LastLevelText = text;
        public void SimulateRetryClicked() => OnRetryClicked?.Invoke();
        public void SimulateBackClicked() => OnBackClicked?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // WinDialogPresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class WinDialogPresenterTests
    {
        [Test]
        public void Initialize_SetsScoreAndLevel()
        {
            var view = new MockWinDialogView();
            var presenter = new WinDialogPresenter(view);
            presenter.Initialize(42, 3);

            Assert.AreEqual("Score: 42", view.LastScoreText);
            Assert.AreEqual("Level 3 Complete!", view.LastLevelText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForContinue_ContinueClicked_Resolves()
        {
            var view = new MockWinDialogView();
            var presenter = new WinDialogPresenter(view);
            presenter.Initialize(10, 1);

            var task = presenter.WaitForContinue().AsTask();
            view.SimulateContinueClicked();
            await task;

            Assert.Pass("WaitForContinue resolved after continue click");
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockWinDialogView();
            var presenter = new WinDialogPresenter(view);
            presenter.Initialize(10, 1);

            var task = presenter.WaitForContinue().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockWinDialogView();
            var presenter = new WinDialogPresenter(view);
            presenter.Initialize(10, 1);
            presenter.Dispose();

            Assert.DoesNotThrow(() => view.SimulateContinueClicked());
        }
    }

    // ---------------------------------------------------------------------------
    // LoseDialogPresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class LoseDialogPresenterTests
    {
        [Test]
        public void Initialize_SetsScoreAndLevel()
        {
            var view = new MockLoseDialogView();
            var presenter = new LoseDialogPresenter(view);
            presenter.Initialize(15, 2);

            Assert.AreEqual("Score: 15", view.LastScoreText);
            Assert.AreEqual("Level 2", view.LastLevelText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForChoice_RetryClicked_ReturnsRetry()
        {
            var view = new MockLoseDialogView();
            var presenter = new LoseDialogPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            view.SimulateRetryClicked();
            var result = await task;

            Assert.AreEqual(LoseDialogChoice.Retry, result);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForChoice_BackClicked_ReturnsBack()
        {
            var view = new MockLoseDialogView();
            var presenter = new LoseDialogPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            view.SimulateBackClicked();
            var result = await task;

            Assert.AreEqual(LoseDialogChoice.Back, result);
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockLoseDialogView();
            var presenter = new LoseDialogPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockLoseDialogView();
            var presenter = new LoseDialogPresenter(view);
            presenter.Initialize(15, 2);
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulateRetryClicked();
                view.SimulateBackClicked();
            });
        }
    }
}
