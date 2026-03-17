using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game;
using SimpleGame.Game.Boot;
using SimpleGame.Game.InGame;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // MockInGameView: pure test double
    // ---------------------------------------------------------------------------
    internal class MockInGameView : IInGameView
    {
        public event Action OnPlaceCorrect;
        public event Action OnPlaceIncorrect;

        public string LastHeartsText { get; private set; }
        public int UpdateHeartsCallCount { get; private set; }
        public string LastPieceCounterText { get; private set; }
        public int UpdatePieceCounterCallCount { get; private set; }
        public string LastLevelLabelText { get; private set; }
        public int UpdateLevelLabelCallCount { get; private set; }

        public void UpdateHearts(string text)
        {
            LastHeartsText = text;
            UpdateHeartsCallCount++;
        }

        public void UpdatePieceCounter(string text)
        {
            LastPieceCounterText = text;
            UpdatePieceCounterCallCount++;
        }

        public void UpdateLevelLabel(string text)
        {
            LastLevelLabelText = text;
            UpdateLevelLabelCallCount++;
        }

        public void SimulatePlaceCorrect() => OnPlaceCorrect?.Invoke();
        public void SimulatePlaceIncorrect() => OnPlaceIncorrect?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // InGamePresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class InGamePresenterTests
    {
        private GameSessionService _session;
        private HeartService _hearts;
        private MockInGameView _view;
        private InGamePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _session = new GameSessionService();
            _session.ResetForNewGame(3, totalPieces: 5); // level 3, 5 pieces
            _hearts = new HeartService();
            _view = new MockInGameView();
            _presenter = new InGamePresenter(_view, _session, _hearts, totalPieces: 5);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void Initialize_SetsLevelLabel()
        {
            _presenter.Initialize();
            Assert.AreEqual("Level 3", _view.LastLevelLabelText);
        }

        [Test]
        public void Initialize_SetsPieceCounterToZero()
        {
            _presenter.Initialize();
            Assert.AreEqual("0/5", _view.LastPieceCounterText);
        }

        [Test]
        public void Initialize_SetsHeartsDisplay()
        {
            _presenter.Initialize();
            Assert.AreEqual("3", _view.LastHeartsText);
        }

        [Test]
        public void Initialize_ResetsHeartsToThree()
        {
            _presenter.Initialize();
            Assert.AreEqual(3, _hearts.RemainingHearts);
        }

        [Test]
        public void PlaceCorrect_IncrementsPieceCounter()
        {
            _presenter.Initialize();
            _view.SimulatePlaceCorrect();
            Assert.AreEqual("1/5", _view.LastPieceCounterText);
        }

        [Test]
        public void PlaceCorrect_UpdatesSessionScore()
        {
            _presenter.Initialize();
            _view.SimulatePlaceCorrect();
            _view.SimulatePlaceCorrect();
            Assert.AreEqual(2, _session.CurrentScore);
        }

        [Test]
        public async Task PlaceCorrect_AllPieces_ResolvesWin()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            for (int i = 0; i < 5; i++)
                _view.SimulatePlaceCorrect();

            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
        }

        [Test]
        public void PlaceIncorrect_CostsOneHeart()
        {
            _presenter.Initialize();
            _view.SimulatePlaceIncorrect();
            Assert.AreEqual(2, _hearts.RemainingHearts);
            Assert.AreEqual("2", _view.LastHeartsText);
        }

        [Test]
        public async Task PlaceIncorrect_AllHearts_ResolvesLose()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            _view.SimulatePlaceIncorrect(); // 2 hearts
            _view.SimulatePlaceIncorrect(); // 1 heart
            _view.SimulatePlaceIncorrect(); // 0 hearts

            var result = await task;
            Assert.AreEqual(InGameAction.Lose, result);
        }

        [Test]
        public void PlaceIncorrect_DoesNotResolve_WhenHeartsRemain()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            _view.SimulatePlaceIncorrect();
            Assert.IsFalse(task.IsCompleted, "Should not resolve with hearts remaining");
        }

        [Test]
        public async Task MixedActions_WinBeforeDeath()
        {
            // 3 pieces total, 3 hearts
            var presenter = new InGamePresenter(_view, _session, _hearts, totalPieces: 3);
            presenter.Initialize();
            var task = presenter.WaitForAction().AsTask();

            _view.SimulatePlaceCorrect();     // 1/3
            _view.SimulatePlaceIncorrect();   // 2 hearts
            _view.SimulatePlaceCorrect();     // 2/3
            _view.SimulatePlaceCorrect();     // 3/3 → win

            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
            Assert.AreEqual(2, _hearts.RemainingHearts);
            presenter.Dispose();
        }

        [Test]
        public void Dispose_CancelsPendingTask()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();
            _presenter.Dispose();
            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            _presenter.Initialize();
            _presenter.Dispose();
            Assert.DoesNotThrow(() =>
            {
                _view.SimulatePlaceCorrect();
                _view.SimulatePlaceIncorrect();
            });
        }
    }

    // ---------------------------------------------------------------------------
    // InGameSceneController tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class InGameSceneControllerTests
    {
        private GameService _gameService;
        private ProgressionService _progression;
        private GameSessionService _session;
        private HeartService _hearts;
        private UIFactory _factory;
        private MockPopupContainerForInGame _popupContainer;
        private MockInputBlockerForInGame _inputBlocker;
        private PopupManager<PopupId> _popupManager;
        private MockGoldenPieceService _goldenPieces;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _hearts = new HeartService();
            _goldenPieces = new MockGoldenPieceService();
            _factory = new UIFactory(_gameService, _progression, _session, _hearts);
            _popupContainer = new MockPopupContainerForInGame();
            _inputBlocker = new MockInputBlockerForInGame();
            _popupManager = new PopupManager<PopupId>(_popupContainer, _inputBlocker);
        }

        [Test]
        public async Task RunAsync_WinAllPieces_ShowsLevelCompleteAndReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 3);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            ctrl.SetViewsForTesting(view, levelCompleteView: completeView);

            var task = ctrl.RunAsync().AsTask();

            // Place all 3 pieces correctly → auto-win
            view.SimulatePlaceCorrect();
            view.SimulatePlaceCorrect();
            view.SimulatePlaceCorrect();

            // LevelComplete popup should now be shown — continue it
            completeView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);
            Assert.AreEqual(2, _progression.CurrentLevel);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseAllHearts_QuitReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 5);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, levelFailedView: failedView);

            var task = ctrl.RunAsync().AsTask();

            // Lose all 3 hearts → auto-lose
            view.SimulatePlaceIncorrect();
            view.SimulatePlaceIncorrect();
            view.SimulatePlaceIncorrect();

            // LevelFailed popup shown — choose Quit
            failedView.SimulateQuitClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Lose, _session.Outcome);
            Assert.AreEqual(1, _progression.CurrentLevel, "Level should not advance on lose");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseRetryThenWin_AdvancesLevel()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 2);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);

            var task = ctrl.RunAsync().AsTask();

            // First attempt: place one correct, then lose all hearts
            view.SimulatePlaceCorrect();      // 1/2
            view.SimulatePlaceIncorrect();    // 2 hearts
            view.SimulatePlaceIncorrect();    // 1 heart
            view.SimulatePlaceIncorrect();    // 0 hearts → lose
            failedView.SimulateRetryClicked(); // retry

            // Second attempt: win
            view.SimulatePlaceCorrect();
            view.SimulatePlaceCorrect(); // 2/2 → win
            completeView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(2, _progression.CurrentLevel, "Level should advance after retry + win");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_PlayFromEditor_UsesDefaultLevel()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            ctrl.SetViewsForTesting(view, levelCompleteView: completeView);

            var task = ctrl.RunAsync().AsTask();

            Assert.AreEqual("Level 1", view.LastLevelLabelText,
                "Play-from-editor should use default level when session has no level set");

            // Complete all default pieces (10) to finish
            for (int i = 0; i < 10; i++)
                view.SimulatePlaceCorrect();
            completeView.SimulateContinueClicked();
            await task;

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseWatchAdThenContinue_KeepsPieceProgress()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 5);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);

            var task = ctrl.RunAsync().AsTask();

            // Place 3 correct pieces, then lose all hearts
            view.SimulatePlaceCorrect();      // 1/5
            view.SimulatePlaceCorrect();      // 2/5
            view.SimulatePlaceCorrect();      // 3/5
            view.SimulatePlaceIncorrect();    // 2 hearts
            view.SimulatePlaceIncorrect();    // 1 heart
            view.SimulatePlaceIncorrect();    // 0 hearts → lose

            // Choose WatchAd → should continue with hearts restored, piece progress kept
            failedView.SimulateWatchAdClicked();

            // Hearts should be restored — verify via view update
            Assert.AreEqual("3", view.LastHeartsText,
                "Hearts should be fully restored after WatchAd");

            // Continue placing the remaining 2 pieces to win
            view.SimulatePlaceCorrect();      // 4/5
            view.SimulatePlaceCorrect();      // 5/5 → win
            completeView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);
            Assert.AreEqual(2, _progression.CurrentLevel, "Level should advance after WatchAd + win");
            Assert.AreEqual("5/5", view.LastPieceCounterText,
                "Piece counter should show all pieces placed");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseRetryThenWin_ResetsProgress()
        {
            // Verify that Retry (unlike WatchAd) does reset piece progress
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 3);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);

            var task = ctrl.RunAsync().AsTask();

            // Place 2 correct, then lose
            view.SimulatePlaceCorrect();      // 1/3
            view.SimulatePlaceCorrect();      // 2/3
            view.SimulatePlaceIncorrect();    // 2 hearts
            view.SimulatePlaceIncorrect();    // 1 heart
            view.SimulatePlaceIncorrect();    // 0 hearts → lose

            // Choose Retry → piece progress should reset
            failedView.SimulateRetryClicked();

            // After retry, piece counter should be back to 0/3
            Assert.AreEqual("0/3", view.LastPieceCounterText,
                "Piece counter should reset after Retry");

            // Complete all pieces
            view.SimulatePlaceCorrect();
            view.SimulatePlaceCorrect();
            view.SimulatePlaceCorrect();  // 3/3 → win
            completeView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);

            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // ---------------------------------------------------------------------------
    // Mock infrastructure for InGame scene controller tests
    // ---------------------------------------------------------------------------
    internal class MockPopupContainerForInGame : IPopupContainer<PopupId>
    {
        public UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
            => UniTask.CompletedTask;

        public UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default)
            => UniTask.CompletedTask;
    }

    internal class MockInputBlockerForInGame : IInputBlocker
    {
        private int _blockCount;
        public bool IsBlocked => _blockCount > 0;
        public void Block() => _blockCount++;
        public void Unblock() => _blockCount = System.Math.Max(0, _blockCount - 1);
    }

    internal class MockGoldenPieceService : IGoldenPieceService
    {
        public int Balance { get; private set; }
        public int EarnCallCount { get; private set; }
        public int SaveCallCount { get; private set; }

        public void Earn(int amount)
        {
            Balance += amount;
            EarnCallCount++;
        }

        public bool TrySpend(int amount)
        {
            if (Balance < amount) return false;
            Balance -= amount;
            return true;
        }

        public void Save() => SaveCallCount++;
        public void ResetAll() => Balance = 0;
    }
}
