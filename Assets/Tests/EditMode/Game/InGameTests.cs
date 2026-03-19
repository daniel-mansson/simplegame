using System;
using System.Collections.Generic;
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
using SimpleGame.Puzzle;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // MockInGameView: pure test double
    // ---------------------------------------------------------------------------
    internal class MockInGameView : IInGameView
    {
        public event Action<int> OnTapPiece;

        public string LastHeartsText { get; private set; }
        public int UpdateHeartsCallCount { get; private set; }
        public string LastPieceCounterText { get; private set; }
        public int UpdatePieceCounterCallCount { get; private set; }
        public string LastLevelLabelText { get; private set; }
        public int UpdateLevelLabelCallCount { get; private set; }
        public int LastDeckPieceId { get; private set; } = -1;
        public bool DeckHidden { get; private set; }
        public int LastRevealedPieceId { get; private set; } = -1;

        public void UpdateHearts(string text)         { LastHeartsText = text; UpdateHeartsCallCount++; }
        public void UpdatePieceCounter(string text)   { LastPieceCounterText = text; UpdatePieceCounterCallCount++; }
        public void UpdateLevelLabel(string text)     { LastLevelLabelText = text; UpdateLevelLabelCallCount++; }
        public void ShowDeckPiece(int pieceId)        { LastDeckPieceId = pieceId; DeckHidden = false; }
        public void HideDeckPanel()                   { DeckHidden = true; }
        public void RevealPiece(int pieceId)          { LastRevealedPieceId = pieceId; }

        /// <summary>Fires OnTapPiece with the given piece ID — simulates a tap on that piece.</summary>
        public void SimulateTapPiece(int pieceId) => OnTapPiece?.Invoke(pieceId);
    }

    // ---------------------------------------------------------------------------
    // Test level helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Builds a linear-chain test level matching the stub used by InGameSceneController:
    /// Piece 0 is the seed. Piece i neighbors i-1 and i+1.
    /// Correct placement order: 1, 2, 3, ...
    /// Wrong tap: any piece whose predecessor isn't yet placed (e.g. piece 3 before placing 2).
    /// </summary>
    internal static class TestLevelBuilder
    {
        public static IPuzzleLevel LinearChain(int totalPieces)
        {
            var pieces = new List<IPuzzlePiece>(totalPieces);
            for (int i = 0; i < totalPieces; i++)
            {
                var neighbors = new List<int>();
                if (i > 0) neighbors.Add(i - 1);
                if (i < totalPieces - 1) neighbors.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, neighbors));
            }
            var seeds = new[] { 0 };
            var deckOrder = new int[totalPieces - 1];
            for (int i = 0; i < deckOrder.Length; i++) deckOrder[i] = i + 1;
            return new PuzzleLevel(pieces, seeds, new IDeck[] { new Deck(deckOrder) });
        }
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
        private IPuzzleLevel _level;

        [SetUp]
        public void SetUp()
        {
            _session = new GameSessionService();
            _session.ResetForNewGame(3, totalPieces: 5); // level 3, 5 pieces (1 seed + 4 non-seed)
            _hearts = new HeartService();
            _view = new MockInGameView();
            _level = TestLevelBuilder.LinearChain(5); // pieces 0..4, seed=0
            _presenter = new InGamePresenter(_view, _session, _hearts, _level);
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
            // 5 total - 1 seed = 4 non-seed pieces
            Assert.AreEqual("0/4", _view.LastPieceCounterText);
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
        public void TapCorrectPiece_IncrementsPieceCounter()
        {
            _presenter.Initialize();
            _view.SimulateTapPiece(1); // piece 1 neighbors seed 0 — correct
            Assert.AreEqual("1/4", _view.LastPieceCounterText);
        }

        [Test]
        public void TapCorrectPiece_UpdatesSessionScore()
        {
            _presenter.Initialize();
            _view.SimulateTapPiece(1); // correct
            _view.SimulateTapPiece(2); // correct
            Assert.AreEqual(2, _session.CurrentScore);
        }

        [Test]
        public async Task TapAllCorrect_ResolvesWin()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            // Tap pieces 1..4 in order (linear chain)
            for (int i = 1; i <= 4; i++)
                _view.SimulateTapPiece(i);

            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
        }

        [Test]
        public void TapIncorrectPiece_CostsOneHeart()
        {
            _presenter.Initialize();
            _view.SimulateTapPiece(3); // piece 3's neighbor 2 is not placed — wrong
            Assert.AreEqual(2, _hearts.RemainingHearts);
            Assert.AreEqual("2", _view.LastHeartsText);
        }

        [Test]
        public async Task TapIncorrect_AllHearts_ResolvesLose()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(3); // wrong (2 hearts)
            _view.SimulateTapPiece(3); // wrong (1 heart)
            _view.SimulateTapPiece(3); // wrong (0 hearts → lose)

            var result = await task;
            Assert.AreEqual(InGameAction.Lose, result);
        }

        [Test]
        public void TapIncorrect_DoesNotResolve_WhenHeartsRemain()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(3); // wrong
            Assert.IsFalse(task.IsCompleted, "Should not resolve with hearts remaining");
        }

        [Test]
        public async Task MixedActions_WinBeforeDeath()
        {
            // 3-piece chain: 0(seed)→1→2, 3 hearts
            var level = TestLevelBuilder.LinearChain(3);
            var presenter = new InGamePresenter(_view, _session, _hearts, level);
            presenter.Initialize();
            var task = presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(1);  // correct — 1/2
            _view.SimulateTapPiece(4);  // wrong — 2 hearts (piece 4 doesn't exist in 3-piece level, so no neighbor placed)
            _view.SimulateTapPiece(2);  // correct — 2/2 → win

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
                _view.SimulateTapPiece(1);
                _view.SimulateTapPiece(3);
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

        // Helper: inject a known linear-chain level into the controller
        private static void SetStubLevel(InGameSceneController ctrl, int totalPieces)
            => ctrl.SetLevelFactory(() => TestLevelBuilder.LinearChain(totalPieces));

        // Helper: tap pieces 1..count in order (all correct on a linear chain)
        private static void TapAllCorrect(MockInGameView view, int nonSeedCount)
        {
            for (int i = 1; i <= nonSeedCount; i++)
                view.SimulateTapPiece(i);
        }

        // Helper: tap a wrong piece repeatedly
        private static void TapWrong(MockInGameView view, int times, int wrongPieceId = 99)
        {
            for (int i = 0; i < times; i++)
                view.SimulateTapPiece(wrongPieceId);
        }

        [Test]
        public async Task RunAsync_WinAllPieces_ShowsLevelCompleteAndReturnsMainMenu()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 3); // 3 total = 1 seed + 2 non-seed
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            ctrl.SetViewsForTesting(view, levelCompleteView: completeView);
            SetStubLevel(ctrl, 3);

            var task = ctrl.RunAsync().AsTask();

            TapAllCorrect(view, 2); // pieces 1 and 2
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
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, levelFailedView: failedView);
            SetStubLevel(ctrl, 5);

            var task = ctrl.RunAsync().AsTask();

            TapWrong(view, 3); // lose 3 hearts
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
            _session.ResetForNewGame(1, totalPieces: 2); // 2 total = 1 seed + 1 non-seed
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 2);

            var task = ctrl.RunAsync().AsTask();

            // First attempt: lose
            TapWrong(view, 3);
            failedView.SimulateRetryClicked();

            // Second attempt: win
            TapAllCorrect(view, 1);
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
            // No session.ResetForNewGame → play-from-editor path
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            ctrl.SetViewsForTesting(view, levelCompleteView: completeView);

            var task = ctrl.RunAsync().AsTask();

            Assert.AreEqual("Level 1", view.LastLevelLabelText,
                "Play-from-editor should use default level when session has no level set");

            // Complete default 10-piece linear chain (9 non-seed pieces 1..9)
            TapAllCorrect(view, 9);
            completeView.SimulateContinueClicked();
            await task;

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseWatchAdThenContinue_KeepsPieceProgress()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 5); // 5 total = 1 seed + 4 non-seed
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 5);

            var task = ctrl.RunAsync().AsTask();

            // Place 3 correct pieces, then lose all hearts
            view.SimulateTapPiece(1);  // 1/4
            view.SimulateTapPiece(2);  // 2/4
            view.SimulateTapPiece(3);  // 3/4
            TapWrong(view, 3);         // 0 hearts → lose

            // WatchAd → continue with hearts restored
            failedView.SimulateWatchAdClicked();

            Assert.AreEqual("3", view.LastHeartsText, "Hearts should be fully restored after WatchAd");

            // Continue placing remaining 1 piece to win
            view.SimulateTapPiece(4);  // 4/4 → win
            completeView.SimulateContinueClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.MainMenu, result);
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);
            Assert.AreEqual(2, _progression.CurrentLevel);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async Task RunAsync_LoseRetryThenWin_ResetsProgress()
        {
            var go = new UnityEngine.GameObject("InGameCtrl");
            var ctrl = go.AddComponent<InGameSceneController>();
            _session.ResetForNewGame(1, totalPieces: 3); // 3 total = 1 seed + 2 non-seed
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 3);

            var task = ctrl.RunAsync().AsTask();

            // Place 1 correct, then lose
            view.SimulateTapPiece(1);  // 1/2
            TapWrong(view, 3);         // 0 hearts → lose

            failedView.SimulateRetryClicked();

            // After retry, counter should reset to 0/2
            Assert.AreEqual("0/2", view.LastPieceCounterText, "Piece counter should reset after Retry");

            // Win on retry
            TapAllCorrect(view, 2);
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
        public UniTask FadeInAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask FadeOutAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public void SetSortOrder(int sortOrder) { }
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
