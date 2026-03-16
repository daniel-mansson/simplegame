using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game;
using SimpleGame.Game.Boot;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // Mock infrastructure for SceneController tests
    // ---------------------------------------------------------------------------

    internal class MockPopupContainerGame : IPopupContainer<PopupId>
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

    internal class MockInputBlockerGame : IInputBlocker
    {
        public int BlockCount { get; private set; }
        public int UnblockCount { get; private set; }
        public bool IsBlocked => BlockCount > UnblockCount;

        public void Block() => BlockCount++;
        public void Unblock() => UnblockCount++;
    }

    // ---------------------------------------------------------------------------
    // SceneController tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class SceneControllerTests
    {
        private GameService _gameService;
        private UIFactory _factory;
        private MockPopupContainerGame _popupContainer;
        private MockInputBlockerGame _inputBlocker;
        private PopupManager<PopupId> _popupManager;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _factory = new UIFactory(_gameService);
            _popupContainer = new MockPopupContainerGame();
            _inputBlocker = new MockInputBlockerGame();
            _popupManager = new PopupManager<PopupId>(_popupContainer, _inputBlocker);
        }

        // -----------------------------------------------------------------------
        // SettingsSceneController
        // -----------------------------------------------------------------------

        [Test]
        public async System.Threading.Tasks.Task SettingsSceneController_RunAsync_BackClicked_ReturnsMainMenu()
        {
            var go = new GameObject("SettingsCtrl");
            var ctrl = go.AddComponent<SettingsSceneController>();
            ctrl.Initialize(_factory);

            var mockView = new MockSettingsView();
            ctrl.SetViewForTesting(mockView);

            var task = ctrl.RunAsync().AsTask();
            mockView.SimulateBackClicked();

            var result = await task;

            Assert.AreEqual(ScreenId.MainMenu, result,
                "SettingsSceneController must return MainMenu after back is pressed");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async System.Threading.Tasks.Task SettingsSceneController_RunAsync_DisposesPresenterOnReturn()
        {
            // Verifies that the presenter is disposed after RunAsync returns,
            // meaning a second RunAsync call won't be confused by stale state.
            var go = new GameObject("SettingsCtrl");
            var ctrl = go.AddComponent<SettingsSceneController>();
            ctrl.Initialize(_factory);

            var mockView = new MockSettingsView();
            ctrl.SetViewForTesting(mockView);

            // First run
            var task1 = ctrl.RunAsync().AsTask();
            mockView.SimulateBackClicked();
            await task1;

            // Second run — same view, fresh presenter should be created
            var task2 = ctrl.RunAsync().AsTask();
            mockView.SimulateBackClicked();
            var result2 = await task2;

            Assert.AreEqual(ScreenId.MainMenu, result2,
                "A second RunAsync call after first completes must work correctly");

            UnityEngine.Object.DestroyImmediate(go);
        }

        // -----------------------------------------------------------------------
        // MainMenuSceneController
        // -----------------------------------------------------------------------

        [Test]
        public async System.Threading.Tasks.Task MainMenuSceneController_RunAsync_SettingsClicked_ReturnsSettings()
        {
            var go = new GameObject("MainMenuCtrl");
            var ctrl = go.AddComponent<MainMenuSceneController>();
            ctrl.Initialize(_factory, _popupManager);

            var mmView = new MockMainMenuView();
            var cdView = new MockConfirmDialogView();
            ctrl.SetViewsForTesting(mmView, cdView);

            var task = ctrl.RunAsync().AsTask();
            mmView.SimulateSettingsClicked();

            var result = await task;

            Assert.AreEqual(ScreenId.Settings, result,
                "Clicking Settings must cause RunAsync to return ScreenId.Settings");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async System.Threading.Tasks.Task MainMenuSceneController_RunAsync_PopupThenSettings_HandlesInline()
        {
            var go = new GameObject("MainMenuCtrl");
            var ctrl = go.AddComponent<MainMenuSceneController>();
            ctrl.Initialize(_factory, _popupManager);

            var mmView = new MockMainMenuView();
            var cdView = new MockConfirmDialogView();
            ctrl.SetViewsForTesting(mmView, cdView);

            var task = ctrl.RunAsync().AsTask();

            // Trigger popup — RunAsync awaits WaitForConfirmation internally
            mmView.SimulatePopupClicked();

            // Let the popup flow run up to WaitForConfirmation await
            // (synchronous mock — confirm immediately)
            cdView.SimulateConfirmClicked();

            // Now dismiss has run; loop continues — click Settings to exit
            mmView.SimulateSettingsClicked();

            var result = await task;

            Assert.AreEqual(ScreenId.Settings, result,
                "After handling popup inline, the loop must continue and return Settings on next action");
            Assert.Contains("show:ConfirmDialog", _popupContainer.CallLog,
                "ShowPopupAsync must have been called for ConfirmDialog");
            Assert.Contains("hide:ConfirmDialog", _popupContainer.CallLog,
                "DismissPopupAsync must have been called for ConfirmDialog");

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async System.Threading.Tasks.Task MainMenuSceneController_RunAsync_PopupCancelled_LoopContinues()
        {
            var go = new GameObject("MainMenuCtrl");
            var ctrl = go.AddComponent<MainMenuSceneController>();
            ctrl.Initialize(_factory, _popupManager);

            var mmView = new MockMainMenuView();
            var cdView = new MockConfirmDialogView();
            ctrl.SetViewsForTesting(mmView, cdView);

            var task = ctrl.RunAsync().AsTask();

            // Popup clicked, then user cancels
            mmView.SimulatePopupClicked();
            cdView.SimulateCancelClicked(); // WaitForConfirmation resolves false

            // Loop continues — navigate to Settings
            mmView.SimulateSettingsClicked();

            var result = await task;

            Assert.AreEqual(ScreenId.Settings, result,
                "Cancelling the popup must allow the loop to continue");

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
