using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Popup;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // Mock popup views
    // ---------------------------------------------------------------------------
    internal class MockLevelCompleteView : ILevelCompleteView
    {
        public event Action OnContinueClicked;
        public string LastScoreText { get; private set; }
        public string LastLevelText { get; private set; }
        public string LastGoldenPiecesText { get; private set; }

        public void UpdateScore(string text) => LastScoreText = text;
        public void UpdateLevel(string text) => LastLevelText = text;
        public void UpdateGoldenPieces(string text) => LastGoldenPiecesText = text;
        public void SimulateContinueClicked() => OnContinueClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    internal class MockLevelFailedView : ILevelFailedView
    {
        public event Action OnRetryClicked;
        public event Action OnWatchAdClicked;
        public event Action OnQuitClicked;
        public string LastScoreText { get; private set; }
        public string LastLevelText { get; private set; }

        public void UpdateScore(string text) => LastScoreText = text;
        public void UpdateLevel(string text) => LastLevelText = text;
        public void SimulateRetryClicked() => OnRetryClicked?.Invoke();
        public void SimulateWatchAdClicked() => OnWatchAdClicked?.Invoke();
        public void SimulateQuitClicked() => OnQuitClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    internal class MockRewardedAdView : IRewardedAdView
    {
        public event Action OnWatchClicked;
        public event Action OnSkipClicked;
        public string LastStatusText { get; private set; }

        public void UpdateStatus(string text) => LastStatusText = text;
        public void SimulateWatchClicked() => OnWatchClicked?.Invoke();
        public void SimulateSkipClicked() => OnSkipClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    internal class MockIAPPurchaseView : IIAPPurchaseView
    {
        public event Action OnPurchaseClicked;
        public event Action OnCancelClicked;
        public string LastItemNameText { get; private set; }
        public string LastPriceText { get; private set; }
        public string LastStatusText { get; private set; }

        public void UpdateItemName(string text) => LastItemNameText = text;
        public void UpdatePrice(string text) => LastPriceText = text;
        public void UpdateStatus(string text) => LastStatusText = text;
        public void SimulatePurchaseClicked() => OnPurchaseClicked?.Invoke();
        public void SimulateCancelClicked() => OnCancelClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    internal class MockObjectRestoredView : IObjectRestoredView
    {
        public event Action OnContinueClicked;
        public string LastObjectNameText { get; private set; }

        public void UpdateObjectName(string text) => LastObjectNameText = text;
        public void SimulateContinueClicked() => OnContinueClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    // ---------------------------------------------------------------------------
    // LevelCompletePresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class LevelCompletePresenterTests
    {
        [Test]
        public void Initialize_SetsScoreLevelAndGoldenPieces()
        {
            var view = new MockLevelCompleteView();
            var presenter = new LevelCompletePresenter(view);
            presenter.Initialize(42, 3, 5);

            Assert.AreEqual("Score: 42", view.LastScoreText);
            Assert.AreEqual("Level 3 Complete!", view.LastLevelText);
            Assert.AreEqual("+5 Golden Pieces", view.LastGoldenPiecesText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForContinue_ContinueClicked_Resolves()
        {
            var view = new MockLevelCompleteView();
            var presenter = new LevelCompletePresenter(view);
            presenter.Initialize(10, 1, 5);

            var task = presenter.WaitForContinue().AsTask();
            view.SimulateContinueClicked();
            await task;

            Assert.Pass("WaitForContinue resolved after continue click");
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockLevelCompleteView();
            var presenter = new LevelCompletePresenter(view);
            presenter.Initialize(10, 1, 5);

            var task = presenter.WaitForContinue().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockLevelCompleteView();
            var presenter = new LevelCompletePresenter(view);
            presenter.Initialize(10, 1, 5);
            presenter.Dispose();

            Assert.DoesNotThrow(() => view.SimulateContinueClicked());
        }
    }

    // ---------------------------------------------------------------------------
    // LevelFailedPresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class LevelFailedPresenterTests
    {
        [Test]
        public void Initialize_SetsScoreAndLevel()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);

            Assert.AreEqual("Score: 15", view.LastScoreText);
            Assert.AreEqual("Level 2", view.LastLevelText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForChoice_RetryClicked_ReturnsRetry()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            view.SimulateRetryClicked();
            var result = await task;

            Assert.AreEqual(LevelFailedChoice.Retry, result);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForChoice_WatchAdClicked_ReturnsWatchAd()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            view.SimulateWatchAdClicked();
            var result = await task;

            Assert.AreEqual(LevelFailedChoice.WatchAd, result);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForChoice_QuitClicked_ReturnsQuit()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            view.SimulateQuitClicked();
            var result = await task;

            Assert.AreEqual(LevelFailedChoice.Quit, result);
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);

            var task = presenter.WaitForChoice().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockLevelFailedView();
            var presenter = new LevelFailedPresenter(view);
            presenter.Initialize(15, 2);
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulateRetryClicked();
                view.SimulateWatchAdClicked();
                view.SimulateQuitClicked();
            });
        }
    }

    // ---------------------------------------------------------------------------
    // RewardedAdPresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class RewardedAdPresenterTests
    {
        [Test]
        public void Initialize_SetsStatusText()
        {
            var view = new MockRewardedAdView();
            var presenter = new RewardedAdPresenter(view);
            presenter.Initialize();

            Assert.AreEqual("Watch a short ad for a reward?", view.LastStatusText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForResult_WatchClicked_ReturnsTrue()
        {
            var view = new MockRewardedAdView();
            var presenter = new RewardedAdPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            view.SimulateWatchClicked();
            var result = await task;

            Assert.IsTrue(result);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForResult_SkipClicked_ReturnsFalse()
        {
            var view = new MockRewardedAdView();
            var presenter = new RewardedAdPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            view.SimulateSkipClicked();
            var result = await task;

            Assert.IsFalse(result);
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockRewardedAdView();
            var presenter = new RewardedAdPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockRewardedAdView();
            var presenter = new RewardedAdPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulateWatchClicked();
                view.SimulateSkipClicked();
            });
        }
    }

    // ---------------------------------------------------------------------------
    // IAPPurchasePresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class IAPPurchasePresenterTests
    {
        [Test]
        public void Initialize_SetsItemAndPrice()
        {
            var view = new MockIAPPurchaseView();
            var presenter = new IAPPurchasePresenter(view, "50 Golden Pieces", "$0.99", 50);
            presenter.Initialize();

            Assert.AreEqual("50 Golden Pieces", view.LastItemNameText);
            Assert.AreEqual("$0.99", view.LastPriceText);
            Assert.AreEqual("Tap Purchase to buy.", view.LastStatusText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForResult_PurchaseClicked_ReturnsTrue()
        {
            var view = new MockIAPPurchaseView();
            var presenter = new IAPPurchasePresenter(view, "50 Golden Pieces", "$0.99", 50);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            view.SimulatePurchaseClicked();
            var result = await task;

            Assert.IsTrue(result);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForResult_CancelClicked_ReturnsFalse()
        {
            var view = new MockIAPPurchaseView();
            var presenter = new IAPPurchasePresenter(view, "50 Golden Pieces", "$0.99", 50);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            view.SimulateCancelClicked();
            var result = await task;

            Assert.IsFalse(result);
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockIAPPurchaseView();
            var presenter = new IAPPurchasePresenter(view, "50 Golden Pieces", "$0.99", 50);
            presenter.Initialize();

            var task = presenter.WaitForResult().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockIAPPurchaseView();
            var presenter = new IAPPurchasePresenter(view, "50 Golden Pieces", "$0.99", 50);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulatePurchaseClicked();
                view.SimulateCancelClicked();
            });
        }
    }

    // ---------------------------------------------------------------------------
    // ObjectRestoredPresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class ObjectRestoredPresenterTests
    {
        [Test]
        public void Initialize_SetsObjectName()
        {
            var view = new MockObjectRestoredView();
            var presenter = new ObjectRestoredPresenter(view);
            presenter.Initialize("Fountain");

            Assert.AreEqual("Fountain Restored!", view.LastObjectNameText);
            presenter.Dispose();
        }

        [Test]
        public async Task WaitForContinue_ContinueClicked_Resolves()
        {
            var view = new MockObjectRestoredView();
            var presenter = new ObjectRestoredPresenter(view);
            presenter.Initialize("Fountain");

            var task = presenter.WaitForContinue().AsTask();
            view.SimulateContinueClicked();
            await task;

            Assert.Pass("WaitForContinue resolved");
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            var view = new MockObjectRestoredView();
            var presenter = new ObjectRestoredPresenter(view);
            presenter.Initialize("Fountain");

            var task = presenter.WaitForContinue().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var view = new MockObjectRestoredView();
            var presenter = new ObjectRestoredPresenter(view);
            presenter.Initialize("Fountain");
            presenter.Dispose();

            Assert.DoesNotThrow(() => view.SimulateContinueClicked());
        }
    }
}
