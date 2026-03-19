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

        // Slot state — slotIndex → current piece ID
        private readonly System.Collections.Generic.Dictionary<int, int?> _slots
            = new System.Collections.Generic.Dictionary<int, int?>();

        public int? GetSlot(int index) => _slots.TryGetValue(index, out var v) ? v : null;
        public int    LastRevealedPieceId { get; private set; } = -1;

        public void UpdateHearts(string text)       { LastHeartsText = text; UpdateHeartsCallCount++; }
        public void UpdatePieceCounter(string text) { LastPieceCounterText = text; UpdatePieceCounterCallCount++; }
        public void UpdateLevelLabel(string text)   { LastLevelLabelText = text; UpdateLevelLabelCallCount++; }
        public void RefreshSlot(int slotIndex, int? pieceId) { _slots[slotIndex] = pieceId; }
        public void RevealPiece(int pieceId)        { LastRevealedPieceId = pieceId; }

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
        private PuzzleModel _model;

        /// <summary>
        /// Builds a linear-chain PuzzleModel:
        /// Piece 0 = seed; pieces 1..N-1 = non-seed in a chain.
        /// Deck = [1, 2, ..., N-1]. SlotCount = 1 for presenter tests
        /// (single slot keeps tests deterministic — tap always hits slot 0).
        /// </summary>
        private static PuzzleModel LinearChainModel(int totalPieces, int slotCount = 1)
        {
            var pieces = new List<IPuzzlePiece>(totalPieces);
            for (int i = 0; i < totalPieces; i++)
            {
                var neighbors = new List<int>();
                if (i > 0) neighbors.Add(i - 1);
                if (i < totalPieces - 1) neighbors.Add(i + 1);
                pieces.Add(new PuzzlePiece(i, neighbors));
            }
            var deckOrder = new int[totalPieces - 1];
            for (int i = 0; i < deckOrder.Length; i++) deckOrder[i] = i + 1;
            return new PuzzleModel(pieces, new[] { 0 }, deckOrder, slotCount);
        }

        [SetUp]
        public void SetUp()
        {
            _session = new GameSessionService();
            _session.ResetForNewGame(3, totalPieces: 5); // level 3, 5 pieces (1 seed + 4 non-seed)
            _hearts = new HeartService();
            _view = new MockInGameView();
            _model = LinearChainModel(5); // pieces 0..4, seed=0, 1 slot
            _presenter = new InGamePresenter(_view, _session, _hearts, _model);
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
            // slot 0 holds piece 1 (neighbours seed 0 — correct)
            _view.SimulateTapPiece(1);
            Assert.AreEqual("1/4", _view.LastPieceCounterText);
        }

        [Test]
        public void TapCorrectPiece_UpdatesSessionScore()
        {
            _presenter.Initialize();
            _view.SimulateTapPiece(1); // slot 0 = piece 1 → correct
            _view.SimulateTapPiece(2); // slot 0 = piece 2 → correct
            Assert.AreEqual(2, _session.CurrentScore);
        }

        [Test]
        public async Task TapAllCorrect_ResolvesWin()
        {
            _presenter.Initialize();
            var task = _presenter.WaitForAction().AsTask();

            // Tap pieces 1..4 in order through slot 0
            for (int i = 1; i <= 4; i++)
                _view.SimulateTapPiece(i);

            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
        }

        [Test]
        public void TapIncorrectPiece_CostsOneHeart()
        {
            _presenter.Initialize();
            // slot 0 holds piece 1 (correct). Tapping piece 3 which is not in any slot → ignored.
            // To trigger Rejected, tap piece 1 but make it unplaceable: use a 5-piece chain
            // and tap piece 3 (slot 0 = piece 1; piece 3 needs piece 2 placed first).
            // Build a 2-slot model so piece 3 is in slot 1 (unplaceable).
            var twoSlotModel = LinearChainModel(5, slotCount: 2);
            // slots: 0=piece1 (correct), 1=piece2 (needs piece1 first → but piece2 neighbours piece1 AND piece3)
            // Actually piece2 neighbours piece1 which is NOT placed → rejected? No: piece2's neighbors are [1,3].
            // piece1 is not placed yet → piece2 rejected.
            var twoSlotPresenter = new InGamePresenter(_view, _session, _hearts, twoSlotModel);
            twoSlotPresenter.Initialize();
            _view.SimulateTapPiece(2); // slot 1 = piece 2, piece 2 neighbours piece 1 (not placed) → rejected
            Assert.AreEqual(2, _hearts.RemainingHearts);
            Assert.AreEqual("2", _view.LastHeartsText);
            twoSlotPresenter.Dispose();
        }

        [Test]
        public async Task TapIncorrect_AllHearts_ResolvesLose()
        {
            // Use 2-slot model; piece 2 in slot 1 is unplaceable until piece 1 placed
            var twoSlotModel = LinearChainModel(5, slotCount: 2);
            var presenter = new InGamePresenter(_view, _session, _hearts, twoSlotModel);
            presenter.Initialize();
            var task = presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(2); // slot 1 — rejected (2 hearts)
            _view.SimulateTapPiece(2); // rejected (1 heart)
            _view.SimulateTapPiece(2); // rejected (0 hearts → lose)

            var result = await task;
            Assert.AreEqual(InGameAction.Lose, result);
            presenter.Dispose();
        }

        [Test]
        public void TapIncorrect_DoesNotResolve_WhenHeartsRemain()
        {
            var twoSlotModel = LinearChainModel(5, slotCount: 2);
            var presenter = new InGamePresenter(_view, _session, _hearts, twoSlotModel);
            presenter.Initialize();
            var task = presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(2); // rejected — 2 hearts remain
            Assert.IsFalse(task.IsCompleted, "Should not resolve with hearts remaining");
            presenter.Dispose();
        }

        [Test]
        public async Task MixedActions_WinBeforeDeath()
        {
            // 3-piece chain: 0(seed)→1→2, 3 hearts, 1 slot
            var model = LinearChainModel(3, slotCount: 1);
            var presenter = new InGamePresenter(_view, _session, _hearts, model);
            presenter.Initialize();
            var task = presenter.WaitForAction().AsTask();

            _view.SimulateTapPiece(1);  // slot 0 = piece1 → correct, 1/2
            // Tap a piece not in any slot — ignored (no heart cost)
            _view.SimulateTapPiece(99); // not in any slot → ignored

            // Now slot 0 = piece 2 (correct)
            _view.SimulateTapPiece(2);  // correct — 2/2 → win

            var result = await task;
            Assert.AreEqual(InGameAction.Win, result);
            Assert.AreEqual(3, _hearts.RemainingHearts, "Hearts unchanged when tap ignored");
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
        // Uses 6 pieces so slots 0=1(placeable), 1=2(NOT placeable until 1 placed), 2=3(not placeable)
        private static void SetStubLevel(InGameSceneController ctrl, int totalPieces)
            => ctrl.SetLevelFactory(() => TestLevelBuilder.LinearChain(totalPieces));

        // Helper: tap pieces 1..count in order (all correct on a linear chain)
        private static void TapAllCorrect(MockInGameView view, int nonSeedCount)
        {
            for (int i = 1; i <= nonSeedCount; i++)
                view.SimulateTapPiece(i);
        }

        // Helper: tap a piece that is in a slot but unplaceable on a linear chain.
        // Piece 2 is always in slot 1 on a ≥4-piece linear chain (slotCount=3),
        // and it requires piece 1 to be placed first — so it's rejected until piece 1 is down.
        private static void TapWrong(MockInGameView view, int times, int wrongPieceId = 2)
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

            // Initial slots: [1, 2, 3]. Piece 2 needs piece 1 placed first → rejected 3 times.
            TapWrong(view, 3); // tap piece 2 (slot 1) — rejected x3 → 0 hearts
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
            // 5 pieces so slots = [1, 2, 3]; piece 2 (slot 1) needs piece 1 → rejected
            _session.ResetForNewGame(1, totalPieces: 5);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 5);

            var task = ctrl.RunAsync().AsTask();

            // First attempt: tap piece 2 (slot 1) 3 times — rejected → lose
            TapWrong(view, 3);
            failedView.SimulateRetryClicked();

            // Second attempt: win (place pieces 1..4 in order)
            TapAllCorrect(view, 4);
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
            // 8 pieces (seed=0, deck=[1..7]). Initial slots: [1,2,3].
            // After placing 1,2,3 → slots become [4,5,6]. Piece 5 (slot 1) needs piece 4 → rejected.
            _session.ResetForNewGame(1, totalPieces: 8);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 8);

            var task = ctrl.RunAsync().AsTask();

            // Place 3 correct pieces
            view.SimulateTapPiece(1);  // 1/7
            view.SimulateTapPiece(2);  // 2/7
            view.SimulateTapPiece(3);  // 3/7
            // Slots now: [4, 5, 6]. Piece 5 needs piece 4 → rejected → costs heart
            view.SimulateTapPiece(5);  // rejected → 2 hearts
            view.SimulateTapPiece(5);  // rejected → 1 heart
            view.SimulateTapPiece(5);  // rejected → 0 hearts → lose

            // WatchAd → continue with hearts restored
            failedView.SimulateWatchAdClicked();

            Assert.AreEqual("3", view.LastHeartsText, "Hearts should be fully restored after WatchAd");

            // Continue: place pieces 4..7 to win (placed so far: 1,2,3 = 3/7; need 4 more)
            view.SimulateTapPiece(4);  // 4/7
            view.SimulateTapPiece(5);  // 5/7 (piece 5 now has piece 4 placed)
            view.SimulateTapPiece(6);  // 6/7
            view.SimulateTapPiece(7);  // 7/7 → win
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
            // 5 pieces (seed=0, deck=[1..4]). Slots: [1,2,3].
            // After placing piece 1, slots: [4,2,3]. Piece 2 needs 1(placed)→correct. Piece 3 needs 2(not placed)→rejected.
            _session.ResetForNewGame(1, totalPieces: 5);
            ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null, null);

            var view = new MockInGameView();
            var completeView = new MockLevelCompleteView();
            var failedView = new MockLevelFailedView();
            ctrl.SetViewsForTesting(view, completeView, failedView);
            SetStubLevel(ctrl, 5);

            var task = ctrl.RunAsync().AsTask();

            // Place piece 1 (correct), then tap piece 3 (slot 2 — needs piece 2, not placed) 3 times → lose
            view.SimulateTapPiece(1);  // 1/4 — correct
            view.SimulateTapPiece(3);  // rejected (piece 3 needs piece 2) → 2 hearts
            view.SimulateTapPiece(3);  // rejected → 1 heart
            view.SimulateTapPiece(3);  // rejected → 0 hearts → lose

            failedView.SimulateRetryClicked();

            // After retry, counter should reset to 0/4
            Assert.AreEqual("0/4", view.LastPieceCounterText, "Piece counter should reset after Retry");

            // Win on retry
            TapAllCorrect(view, 4);
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
