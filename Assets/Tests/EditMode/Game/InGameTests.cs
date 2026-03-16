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
        public event Action OnScoreClicked;
        public event Action OnWinClicked;
        public event Action OnLoseClicked;

        public string LastScoreText { get; private set; }
        public int UpdateScoreCallCount { get; private set; }
        public string LastLevelLabelText { get; private set; }
        public int UpdateLevelLabelCallCount { get; private set; }

        public void UpdateScore(string text)
        {
            LastScoreText = text;
            UpdateScoreCallCount++;
        }

        public void UpdateLevelLabel(string text)
        {
            LastLevelLabelText = text;
            UpdateLevelLabelCallCount++;
        }

        public void SimulateScoreClicked() => OnScoreClicked?.Invoke();
        public void SimulateWinClicked() => OnWinClicked?.Invoke();
        public void SimulateLoseClicked() => OnLoseClicked?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // InGamePresenter tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class InGamePresenterTests
    {
        private GameSessionService _session;
        private MockInGameView _view;
        private InGamePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _session = new GameSessionService();
            _session.ResetForNewGame(3); // level 3
            _view = new MockInGameView();
            _presenter = new InGamePresenter(_view, _session);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void Initialize_SetsScoreToZero()
        {
            _presenter.Initialize();
            Assert.AreEqual("0", _view.LastScoreText);
        }

        [Test]
        public void Initialize_SetsLevelLabel()
        {
            _presenter.Initialize();
            Assert.AreEqual("Level 3", _view.LastLevelLabelText);
        }

        [Test]
        public void ScoreClicked_IncrementsScoreDisplay()
        {
            _presenter.Initialize();
            _view.SimulateScoreClicked();
            Assert.AreEqual("1", _view.LastScoreText);

            _view.SimulateScoreClicked();
            Assert.AreEqual("2", _view.LastScoreText);
        }

        [Test]
        public void ScoreClicked_WritesToSessionCurrentScore()
        {
            _presenter.Initialize();
            _view.SimulateScoreClicked();
            _view.SimulateScoreClicked();
            _view.SimulateScoreClicked();
            Assert.AreEqual(3, _session.CurrentScore);
        }

        [Test]
        public async Task WaitForAction_WinClicked_ResolvesWin()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();
            _view.SimulateWinClicked();
            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
        }

        [Test]
        public async Task WaitForAction_LoseClicked_ResolvesLose()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();
            _view.SimulateLoseClicked();
            var result = await task;
            Assert.AreEqual(InGameAction.Lose, result);
        }

        [Test]
        public async Task WaitForAction_ScoreThenWin_OnlyWinResolves()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            // Score clicks should NOT resolve the task
            _view.SimulateScoreClicked();
            _view.SimulateScoreClicked();
            Assert.IsFalse(task.IsCompleted, "Score clicks must not resolve WaitForAction");

            _view.SimulateWinClicked();
            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
        }

        [Test]
        public async Task WinClicked_SetsSessionScore()
        {
            _presenter.Initialize();
            _view.SimulateScoreClicked();
            _view.SimulateScoreClicked(); // score = 2

            var task = _presenter.WaitForAction().AsTask();
            _view.SimulateWinClicked();
            await task;

            Assert.AreEqual(2, _session.CurrentScore);
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
                _view.SimulateScoreClicked();
                _view.SimulateWinClicked();
                _view.SimulateLoseClicked();
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
        private UIFactory _factory;
        private MockPopupContainerForInGame _popupContainer;
        private MockInputBlockerForInGame _inputBlocker;
        private PopupManager<PopupId> _popupManager;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _factory = new UIFactory(_gameService, _progression, _session);
            _popupContainer = new MockPopupContainerForInGame();
            _inputBlocker = new MockInputBlockerForInGame();
            _popupManager = new PopupManager<PopupId>(_popupContainer, _inputBlocker);
        }

        [Test]
        public async Task RunAsync_WinClicked_ShowsWinPopupAndReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session, _popupManager);

            var view = new MockInGameView();
            var winView = new MockWinDialogView();
            ctrl.SetViewsForTesting(view, winDialogView: winView);

            var task = ctrl.RunAsync().AsTask();
            view.SimulateScoreClicked(); // score = 1
            view.SimulateWinClicked();

            // Win popup should now be shown — continue it
            winView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);
            Assert.AreEqual(2, _progression.CurrentLevel);
            Assert.AreEqual("Score: 1", winView.LastScoreText);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseClicked_BackReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session, _popupManager);

            var view = new MockInGameView();
            var loseView = new MockLoseDialogView();
            ctrl.SetViewsForTesting(view, loseDialogView: loseView);

            var task = ctrl.RunAsync().AsTask();
            view.SimulateLoseClicked();

            // Lose popup shown — choose Back
            loseView.SimulateBackClicked();

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
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session, _popupManager);

            var view = new MockInGameView();
            var winView = new MockWinDialogView();
            var loseView = new MockLoseDialogView();
            ctrl.SetViewsForTesting(view, winView, loseView);

            var task = ctrl.RunAsync().AsTask();

            // First attempt: score some, then lose
            view.SimulateScoreClicked(); // score = 1
            view.SimulateScoreClicked(); // score = 2
            view.SimulateLoseClicked();
            loseView.SimulateRetryClicked(); // retry

            // Second attempt: score should be reset, win
            Assert.AreEqual(0, _session.CurrentScore, "Score should reset on retry");
            view.SimulateScoreClicked(); // score = 1
            view.SimulateWinClicked();
            winView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(2, _progression.CurrentLevel, "Level should advance after retry + win");
            Assert.AreEqual(1, _session.CurrentScore, "Final score should be 1 from second attempt");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_PlayFromEditor_UsesDefaultLevel()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            ctrl.Initialize(_factory, _progression, _session, _popupManager);

            var view = new MockInGameView();
            var winView = new MockWinDialogView();
            ctrl.SetViewsForTesting(view, winDialogView: winView);

            var task = ctrl.RunAsync().AsTask();

            Assert.AreEqual("Level 1", view.LastLevelLabelText,
                "Play-from-editor should use default level when session has no level set");

            view.SimulateWinClicked();
            winView.SimulateContinueClicked();
            await task;

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
}
