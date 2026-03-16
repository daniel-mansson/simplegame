using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game;
using SimpleGame.Game.Boot;
using SimpleGame.Game.InGame;
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

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _factory = new UIFactory(_gameService, _progression, _session);
        }

        [Test]
        public async Task RunAsync_WinClicked_ReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session);

            var view = new MockInGameView();
            ctrl.SetViewForTesting(view);

            var task = ctrl.RunAsync().AsTask();
            view.SimulateWinClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseClicked_ReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session);

            var view = new MockInGameView();
            ctrl.SetViewForTesting(view);

            var task = ctrl.RunAsync().AsTask();
            view.SimulateLoseClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Lose, _session.Outcome);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_WinClicked_RegistersWinAndAdvancesLevel()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1);
            ctrl.Initialize(_factory, _progression, _session);

            var view = new MockInGameView();
            ctrl.SetViewForTesting(view);

            var task = ctrl.RunAsync().AsTask();
            view.SimulateScoreClicked(); // score = 1
            view.SimulateScoreClicked(); // score = 2
            view.SimulateWinClicked();
            await task;

            Assert.AreEqual(2, _progression.CurrentLevel, "RegisterWin should advance level from 1 to 2");
            Assert.AreEqual(2, _session.CurrentScore, "Session score should be 2");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_PlayFromEditor_UsesDefaultLevel()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            // Don't call _session.ResetForNewGame — simulates playing from editor
            ctrl.Initialize(_factory, _progression, _session);

            var view = new MockInGameView();
            ctrl.SetViewForTesting(view);

            var task = ctrl.RunAsync().AsTask();

            // The default level should have been used (1)
            Assert.AreEqual("Level 1", view.LastLevelLabelText,
                "Play-from-editor should use default level when session has no level set");

            view.SimulateWinClicked();
            await task;

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
