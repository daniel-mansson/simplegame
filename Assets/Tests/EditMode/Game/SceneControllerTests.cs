using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game;
using SimpleGame.Game.Boot;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Meta;
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
        public UniTask FadeInAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask FadeOutAsync(System.Threading.CancellationToken ct = default) => UniTask.CompletedTask;
        public void SetSortOrder(int sortOrder) { }
    }

    // ---------------------------------------------------------------------------
    // SceneController tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class SceneControllerTests
    {
        private GameService _gameService;
        private ProgressionService _progression;
        private GameSessionService _session;
        private HeartService _hearts;
        private MockMetaSaveServiceForCtrl _saveService;
        private MetaProgressionService _metaProgression;
        private GoldenPieceService _goldenPieces;
        private UIFactory _factory;
        private MockPopupContainerGame _popupContainer;
        private MockInputBlockerGame _inputBlocker;
        private PopupManager<PopupId> _popupManager;

        private RestorableObjectData _fountain;
        private EnvironmentData _testEnv;
        private WorldData _testWorld;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _hearts = new HeartService();

            _fountain = ScriptableObject.CreateInstance<RestorableObjectData>();
            _fountain.name = "Fountain";
            _fountain.displayName = "Fountain";
            _fountain.totalSteps = 3;
            _fountain.costPerStep = 1;
            _fountain.blockedBy = new RestorableObjectData[0];

            _testEnv = ScriptableObject.CreateInstance<EnvironmentData>();
            _testEnv.environmentName = "Garden";
            _testEnv.objects = new[] { _fountain };

            _testWorld = ScriptableObject.CreateInstance<WorldData>();
            _testWorld.environments = new[] { _testEnv };

            _saveService = new MockMetaSaveServiceForCtrl();
            _metaProgression = new MetaProgressionService(_testWorld, _saveService);
            _goldenPieces = new GoldenPieceService(_saveService);

            _factory = new UIFactory(_gameService, _progression, _session, _hearts,
                                     _metaProgression, _goldenPieces);
            _popupContainer = new MockPopupContainerGame();
            _inputBlocker = new MockInputBlockerGame();
            _popupManager = new PopupManager<PopupId>(_popupContainer, _inputBlocker);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fountain);
            UnityEngine.Object.DestroyImmediate(_testEnv);
            UnityEngine.Object.DestroyImmediate(_testWorld);
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
            Assert.AreEqual(ScreenId.MainMenu, result);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async System.Threading.Tasks.Task SettingsSceneController_RunAsync_DisposesPresenterOnReturn()
        {
            var go = new GameObject("SettingsCtrl");
            var ctrl = go.AddComponent<SettingsSceneController>();
            ctrl.Initialize(_factory);

            var mockView = new MockSettingsView();
            ctrl.SetViewForTesting(mockView);

            var task1 = ctrl.RunAsync().AsTask();
            mockView.SimulateBackClicked();
            await task1;

            var task2 = ctrl.RunAsync().AsTask();
            mockView.SimulateBackClicked();
            var result2 = await task2;

            Assert.AreEqual(ScreenId.MainMenu, result2);
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
            ctrl.Initialize(_factory, _popupManager, _metaProgression, _progression, _goldenPieces, null);

            var mmView = new MockMainMenuView();
            ctrl.SetViewsForTesting(mmView);

            var task = ctrl.RunAsync().AsTask();
            mmView.SimulateSettingsClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.Settings, result);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public async System.Threading.Tasks.Task MainMenuSceneController_RunAsync_PlayClicked_ReturnsInGame()
        {
            var go = new GameObject("MainMenuCtrl");
            var ctrl = go.AddComponent<MainMenuSceneController>();
            ctrl.Initialize(_factory, _popupManager, _metaProgression, _progression, _goldenPieces, null);

            var mmView = new MockMainMenuView();
            ctrl.SetViewsForTesting(mmView);

            var task = ctrl.RunAsync().AsTask();
            mmView.SimulatePlayClicked();

            var result = await task;
            Assert.AreEqual(ScreenId.InGame, result);

            UnityEngine.Object.DestroyImmediate(go);
        }

        /// <summary>In-memory mock save service for controller tests.</summary>
        private class MockMetaSaveServiceForCtrl : IMetaSaveService
        {
            private string _json;

            public void Save(MetaSaveData data)
            {
                _json = JsonUtility.ToJson(data);
            }

            public MetaSaveData Load()
            {
                if (string.IsNullOrEmpty(_json))
                    return new MetaSaveData();
                return JsonUtility.FromJson<MetaSaveData>(_json);
            }

            public void Delete()
            {
                _json = null;
            }
        }
    }
}
