using System;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.MVP;
using SimpleGame.Game;
using SimpleGame.Game.Boot;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // MockMainMenuView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockMainMenuView : IMainMenuView
    {
        public event Action OnSettingsClicked;
        public event Action OnPopupClicked;
        public event Action OnPlayClicked;

        public string LastTitleText { get; private set; }
        public int UpdateTitleCallCount { get; private set; }
        public string LastLevelDisplayText { get; private set; }
        public int UpdateLevelDisplayCallCount { get; private set; }

        public void UpdateTitle(string text)
        {
            LastTitleText = text;
            UpdateTitleCallCount++;
        }

        public void UpdateLevelDisplay(string text)
        {
            LastLevelDisplayText = text;
            UpdateLevelDisplayCallCount++;
        }

        public void SimulateSettingsClicked() => OnSettingsClicked?.Invoke();
        public void SimulatePopupClicked() => OnPopupClicked?.Invoke();
        public void SimulatePlayClicked() => OnPlayClicked?.Invoke();
    }

    // ---------------------------------------------------------------------------
    // MockSettingsView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockSettingsView : ISettingsView
    {
        public event Action OnBackClicked;

        public string LastTitleText { get; private set; }
        public int UpdateTitleCallCount { get; private set; }

        public void UpdateTitle(string text)
        {
            LastTitleText = text;
            UpdateTitleCallCount++;
        }

        public void SimulateBackClicked() => OnBackClicked?.Invoke();
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
        private UIFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _progression = new ProgressionService();
            _session = new GameSessionService();
            _factory = new UIFactory(_gameService, _progression, _session);
        }

        // --- Construction tests ---

        [Test]
        public void UIFactory_CreatesMainMenuPresenter()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);

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

        // --- Initialize sets initial title/message ---

        [Test]
        public void MainMenuPresenter_Initialize_SetsTitle()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);

            presenter.Initialize();

            Assert.IsFalse(string.IsNullOrEmpty(view.LastTitleText),
                "Initialize() must set a non-empty title on the MainMenu view");
            Assert.Greater(view.UpdateTitleCallCount, 0,
                "Initialize() must call UpdateTitle at least once");
        }

        [Test]
        public void MainMenuPresenter_Initialize_SetsLevelDisplay()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);

            presenter.Initialize();

            Assert.AreEqual("Level 1", view.LastLevelDisplayText,
                "Initialize() must set level display to 'Level 1' (progression starts at 1)");
            Assert.Greater(view.UpdateLevelDisplayCallCount, 0,
                "Initialize() must call UpdateLevelDisplay at least once");
        }

        [Test]
        public void MainMenuPresenter_Initialize_ReflectsAdvancedLevel()
        {
            _progression.RegisterWin(0); // advance to level 2
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);

            presenter.Initialize();

            Assert.AreEqual("Level 2", view.LastLevelDisplayText);
        }

        [Test]
        public void SettingsPresenter_Initialize_SetsTitle()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);

            presenter.Initialize();

            Assert.IsFalse(string.IsNullOrEmpty(view.LastTitleText),
                "Initialize() must set a non-empty title on the Settings view");
            Assert.Greater(view.UpdateTitleCallCount, 0,
                "Initialize() must call UpdateTitle at least once");
        }

        [Test]
        public void ConfirmDialogPresenter_Initialize_SetsMessage()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);

            presenter.Initialize();

            Assert.IsFalse(string.IsNullOrEmpty(view.LastMessageText),
                "Initialize() must set a non-empty message on the ConfirmDialog view");
            Assert.Greater(view.UpdateMessageCallCount, 0,
                "Initialize() must call UpdateMessage at least once");
        }

        // --- Async result task tests ---

        [Test]
        public async Task MainMenuPresenter_WaitForAction_SettingsClicked_ResolvesSettings()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulateSettingsClicked();

            var result = await task;
            Assert.AreEqual(MainMenuAction.Settings, result);
        }

        [Test]
        public async Task MainMenuPresenter_WaitForAction_PopupClicked_ResolvesPopup()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulatePopupClicked();

            var result = await task;
            Assert.AreEqual(MainMenuAction.Popup, result);
        }

        [Test]
        public async Task MainMenuPresenter_WaitForAction_PlayClicked_ResolvesPlay()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
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
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            view.SimulatePlayClicked();
            await task;

            Assert.AreEqual(1, _session.CurrentLevelId,
                "Play must set session level to progression's current level");
            Assert.AreEqual(0, _session.CurrentScore,
                "Play must reset session score to 0");
            Assert.AreEqual(GameOutcome.None, _session.Outcome,
                "Play must reset session outcome to None");
        }

        [Test]
        public async Task SettingsPresenter_WaitForBack_BackClicked_Resolves()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForBack().AsTask();
            view.SimulateBackClicked();

            await task; // resolves without exception == pass
            Assert.Pass("WaitForBack() resolved after back click");
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
            Assert.IsTrue(result, "Confirm click must resolve WaitForConfirmation() as true");
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
            Assert.IsFalse(result, "Cancel click must resolve WaitForConfirmation() as false");
        }

        [Test]
        public async Task MainMenuPresenter_WaitForAction_SecondCall_CancelsPrevious()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            var firstTask = presenter.WaitForAction().AsTask();
            var secondTask = presenter.WaitForAction().AsTask(); // cancels first

            view.SimulateSettingsClicked();

            // First task should throw OperationCanceledException when awaited
            Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(async () => await firstTask,
                "Calling WaitForAction() a second time must cancel the previous pending task");

            var result = await secondTask;
            Assert.AreEqual(MainMenuAction.Settings, result,
                "Second task resolves normally");
        }

        // --- Dispose cancels pending tasks ---

        [Test]
        public void MainMenuPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForAction().AsTask();
            presenter.Dispose();

            // After dispose, awaiting the task throws OperationCanceledException
            Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(async () => await task,
                "Dispose() must cancel any pending WaitForAction() task");
        }

        [Test]
        public void SettingsPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForBack().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(async () => await task,
                "Dispose() must cancel any pending WaitForBack() task");
        }

        [Test]
        public void ConfirmDialogPresenter_Dispose_CancelsPendingTask()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            var task = presenter.WaitForConfirmation().AsTask();
            presenter.Dispose();

            Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(async () => await task,
                "Dispose() must cancel any pending WaitForConfirmation() task");
        }

        // --- After Dispose, view events are unsubscribed ---

        [Test]
        public void MainMenuPresenter_Dispose_UnsubscribesViewEvents()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            // Fire events after dispose — no exception, no side-effects
            Assert.DoesNotThrow(() =>
            {
                view.SimulateSettingsClicked();
                view.SimulatePopupClicked();
                view.SimulatePlayClicked();
            }, "Firing view events after Dispose() must not throw");
        }

        [Test]
        public void SettingsPresenter_Dispose_UnsubscribesViewEvents()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            Assert.DoesNotThrow(() => view.SimulateBackClicked(),
                "Firing back event after Dispose() must not throw");
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
            }, "Firing view events after Dispose() must not throw");
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
    }
}
