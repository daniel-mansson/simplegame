using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.MVP;
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
    // MockMainMenuView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockMainMenuView : IMainMenuView
    {
        public event Action OnSettingsClicked;
        public event Action OnPlayClicked;
        public event Action OnResetProgressClicked;
        public event Action OnNextEnvironmentClicked;
        public event Action OnShopClicked;
        public event Action OnShopBackClicked;
        public event Action OnDebugRewardedClicked;
        public event Action OnDebugInterstitialClicked;
        public event Action OnDebugBannerClicked;
        public event Action<int> OnObjectTapped;

        public string LastEnvironmentNameText { get; private set; }
        public string LastBalanceText { get; private set; }
        public string LastLevelDisplayText { get; private set; }
        public ObjectDisplayData[] LastObjects { get; private set; }
        public int UpdateObjectsCallCount { get; private set; }
        public bool NextEnvironmentVisible { get; private set; }
        public bool DebugAdsVisible { get; private set; }
        public string LastDebugStatus { get; private set; }

        public void UpdateEnvironmentName(string text) => LastEnvironmentNameText = text;
        public void UpdateBalance(string text) => LastBalanceText = text;
        public void UpdateLevelDisplay(string text) => LastLevelDisplayText = text;
        public void SetNextEnvironmentVisible(bool visible) => NextEnvironmentVisible = visible;
        public void SetDebugAdsVisible(bool visible) => DebugAdsVisible = visible;
        public void UpdateDebugStatus(string text) => LastDebugStatus = text;

        public void UpdateObjects(ObjectDisplayData[] objects)
        {
            LastObjects = objects;
            UpdateObjectsCallCount++;
        }

        public void SimulateSettingsClicked() => OnSettingsClicked?.Invoke();
        public void SimulatePlayClicked() => OnPlayClicked?.Invoke();
        public void SimulateObjectTapped(int index) => OnObjectTapped?.Invoke(index);
        public void SimulateResetProgressClicked() => OnResetProgressClicked?.Invoke();
        public void SimulateNextEnvironmentClicked() => OnNextEnvironmentClicked?.Invoke();
        public void SimulateShopClicked() => OnShopClicked?.Invoke();
        public void SimulateShopBackClicked() => OnShopBackClicked?.Invoke();
        public void SimulateDebugRewardedClicked() => OnDebugRewardedClicked?.Invoke();
        public void SimulateDebugInterstitialClicked() => OnDebugInterstitialClicked?.Invoke();
        public void SimulateDebugBannerClicked() => OnDebugBannerClicked?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // MockSettingsView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockSettingsView : ISettingsView
    {
        public event Action OnBackClicked;
        public event Action OnLinkGameCenterClicked;
        public event Action OnLinkGooglePlayClicked;
        public event Action OnUnlinkGameCenterClicked;
        public event Action OnUnlinkGooglePlayClicked;

        public string LastTitleText { get; private set; }
        public int UpdateTitleCallCount { get; private set; }
        public bool LastGameCenterLinked { get; private set; }
        public bool LastGooglePlayLinked { get; private set; }

        public void UpdateTitle(string text)
        {
            LastTitleText = text;
            UpdateTitleCallCount++;
        }

        public void UpdateLinkStatus(bool gameCenterLinked, bool googlePlayLinked)
        {
            LastGameCenterLinked = gameCenterLinked;
            LastGooglePlayLinked = googlePlayLinked;
        }

        public void SimulateBackClicked() => OnBackClicked?.Invoke();
        public void SimulateLinkGameCenter() => OnLinkGameCenterClicked?.Invoke();
        public void SimulateLinkGooglePlay() => OnLinkGooglePlayClicked?.Invoke();
        public void SimulateUnlinkGameCenter() => OnUnlinkGameCenterClicked?.Invoke();
        public void SimulateUnlinkGooglePlay() => OnUnlinkGooglePlayClicked?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // MockConfirmDialogView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockConfirmDialogView : IConfirmDialogView
    {
        public event Action OnConfirmClicked;
        public event Action OnCancelClicked;

        public string LastMessageText { get; private set; }
        public int UpdateMessageCallCount { get; private set; }

        public void UpdateMessage(string text)
        {
            LastMessageText = text;
            UpdateMessageCallCount++;
        }

        public void SimulateConfirmClicked() => OnConfirmClicked?.Invoke();
        public void SimulateCancelClicked() => OnCancelClicked?.Invoke();
        public UniTask AnimateInAsync(CancellationToken ct = default) => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }

    // ---------------------------------------------------------------------------
    // Demo wiring tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class DemoWiringTests
    {
        private GameService _gameService;
        private ProgressionService _progression;
        private GameSessionService _session;
        private HeartService _hearts;
        private MockMetaSaveServiceForDemo _saveService;
        private MetaProgressionService _metaProgression;
        private GoldenPieceService _goldenPieces;
        private UIFactory _factory;
        private EnvironmentData _testEnv;
        private WorldData _testWorld;
        private RestorableObjectData _fountain;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _hearts = new HeartService();

            // Create minimal test data
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

            _saveService = new MockMetaSaveServiceForDemo();
            _metaProgression = new MetaProgressionService(_testWorld, _saveService);
            _goldenPieces = new GoldenPieceService(_saveService);

            _factory = new UIFactory(_gameService, _progression, _session, _hearts,
                                     _metaProgression, _goldenPieces);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fountain);
            UnityEngine.Object.DestroyImmediate(_testEnv);
            UnityEngine.Object.DestroyImmediate(_testWorld);
        }

        // --- Construction tests ---

        [Test]
        public void UIFactory_CreatesMainMenuPresenter()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<MainMenuPresenter>(presenter);
        }

        [Test]
        public void UIFactory_CreatesSettingsPresenter()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<SettingsPresenter>(presenter);
        }

        [Test]
        public void UIFactory_CreatesConfirmDialogPresenter()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<ConfirmDialogPresenter>(presenter);
        }

        // --- Initialize sets initial state ---

        [Test]
        public void MainMenuPresenter_Initialize_SetsEnvironmentName()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            Assert.AreEqual("Garden", view.LastEnvironmentNameText);
        }

        [Test]
        public void MainMenuPresenter_Initialize_SetsLevelDisplay()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            Assert.AreEqual("Level 1", view.LastLevelDisplayText);
        }

        [Test]
        public void MainMenuPresenter_Initialize_SetsBalance()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            Assert.AreEqual("0 Golden Pieces", view.LastBalanceText);
        }

        [Test]
        public void MainMenuPresenter_Initialize_SetsObjects()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            Assert.IsNotNull(view.LastObjects);
            Assert.AreEqual(1, view.LastObjects.Length);
            Assert.AreEqual("Fountain", view.LastObjects[0].Name);
            Assert.AreEqual("0/3", view.LastObjects[0].Progress);
            Assert.IsFalse(view.LastObjects[0].IsBlocked);
            Assert.IsFalse(view.LastObjects[0].IsComplete);
        }

        [Test]
        public void MainMenuPresenter_Initialize_ReflectsAdvancedLevel()
        {
            _progression.RegisterWin(0); // advance to level 2
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            Assert.AreEqual("Level 2", view.LastLevelDisplayText);
        }

        [Test]
        public void SettingsPresenter_Initialize_SetsTitle()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            Assert.IsFalse(string.IsNullOrEmpty(view.LastTitleText));
        }

        [Test]
        public void ConfirmDialogPresenter_Initialize_SetsMessage()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            Assert.IsFalse(string.IsNullOrEmpty(view.LastMessageText));
        }

        // --- Async result task tests ---

        [Test]
        public async Task MainMenuPresenter_WaitForAction_SettingsClicked_ResolvesSettings()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulateSettingsClicked();

            var result = await task;
            Assert.AreEqual(MainMenuAction.Settings, result);
        }

        [Test]
        public async Task MainMenuPresenter_WaitForAction_PlayClicked_ResolvesPlay()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulatePlayClicked();

            var result = await task;
            Assert.AreEqual(MainMenuAction.Play, result);
        }

        [Test]
        public async Task MainMenuPresenter_PlayClicked_SetsSessionContext()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulatePlayClicked();
            await task;

            Assert.AreEqual(1, _session.CurrentLevelId);
            Assert.AreEqual(0, _session.CurrentScore);
            Assert.AreEqual(GameOutcome.None, _session.Outcome);
        }

        // --- Object tapping ---

        [Test]
        public void MainMenuPresenter_TapObject_SpendsGoldenPieces()
        {
            _goldenPieces.Earn(10);
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            view.SimulateObjectTapped(0);

            Assert.AreEqual(9, _goldenPieces.Balance);
            Assert.AreEqual(1, _metaProgression.GetCurrentSteps(_fountain));
        }

        [Test]
        public void MainMenuPresenter_TapObject_InsufficientBalance_NoChange()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            view.SimulateObjectTapped(0); // 0 balance

            Assert.AreEqual(0, _metaProgression.GetCurrentSteps(_fountain));
        }

        [Test]
        public async Task MainMenuPresenter_TapObject_CompletesObject_ResolvesObjectRestored()
        {
            _goldenPieces.Earn(10);
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();

            view.SimulateObjectTapped(0); // 1/3
            view.SimulateObjectTapped(0); // 2/3
            view.SimulateObjectTapped(0); // 3/3 → complete

            var result = await task;
            Assert.AreEqual(MainMenuAction.ObjectRestored, result);
            Assert.AreEqual("Fountain", presenter.LastRestoredObjectName);
        }

        // --- Async tests for other presenters ---

        [Test]
        public async Task SettingsPresenter_WaitForBack_BackClicked_Resolves()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForBack().AsTask();
            view.SimulateBackClicked();
            await task;
            Assert.Pass("WaitForBack resolved");
        }

        [Test]
        public async Task ConfirmDialogPresenter_WaitForConfirmation_ConfirmClicked_ReturnsTrue()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForConfirmation().AsTask();
            view.SimulateConfirmClicked();
            var result = await task;
            Assert.IsTrue(result);
        }

        [Test]
        public async Task ConfirmDialogPresenter_WaitForConfirmation_CancelClicked_ReturnsFalse()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForConfirmation().AsTask();
            view.SimulateCancelClicked();
            var result = await task;
            Assert.IsFalse(result);
        }

        // --- Dispose ---

        [Test]
        public void MainMenuPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void SettingsPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForBack().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void ConfirmDialogPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForConfirmation().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public void MainMenuPresenter_Dispose_UnsubscribesViewEvents()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view, _testEnv);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulateSettingsClicked();
                view.SimulatePlayClicked();
                view.SimulateObjectTapped(0);
            });
        }

        [Test]
        public void SettingsPresenter_Dispose_UnsubscribesViewEvents()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() => view.SimulateBackClicked());
        }

        [Test]
        public void ConfirmDialogPresenter_Dispose_UnsubscribesViewEvents()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() =>
            {
                view.SimulateConfirmClicked();
                view.SimulateCancelClicked();
            });
        }

        // --- Mock views have no backward references ---

        [Test]
        public void MockMainMenuView_HasNoPresenterOrServiceReference()
        {
            AssertNoBackwardReferences(typeof(MockMainMenuView));
        }

        [Test]
        public void MockSettingsView_HasNoPresenterOrServiceReference()
        {
            AssertNoBackwardReferences(typeof(MockSettingsView));
        }

        [Test]
        public void MockConfirmDialogView_HasNoPresenterOrServiceReference()
        {
            AssertNoBackwardReferences(typeof(MockConfirmDialogView));
        }

        // ---------------------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------------------
        private static void AssertNoBackwardReferences(Type mockType)
        {
            var allFields = mockType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in allFields)
            {
                var fieldType = field.FieldType;
                var typeName = fieldType.FullName ?? fieldType.Name;

                Assert.IsFalse(
                    typeName.Contains("Presenter"),
                    $"{mockType.Name} field '{field.Name}' references a Presenter type: {typeName}");

                Assert.IsFalse(
                    typeName.Contains("GameService") || typeName.Contains("SimpleGame.Game.Services"),
                    $"{mockType.Name} field '{field.Name}' references a Services type: {typeName}");

                Assert.IsFalse(
                    typeName.Contains("UIFactory"),
                    $"{mockType.Name} field '{field.Name}' references UIFactory: {typeName}");
            }

            var baseType = mockType.BaseType;
            Assert.IsFalse(
                baseType != null &&
                (baseType.Name.Contains("Presenter") || (baseType.FullName?.Contains("Presenter") ?? false)),
                $"{mockType.Name} must not inherit from any Presenter type");
        }

        /// <summary>In-memory mock save service for demo wiring tests.</summary>
        private class MockMetaSaveServiceForDemo : IMetaSaveService
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
