using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Core.MVP;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;
using SimpleGame.Core.Services;

namespace SimpleGame.Tests
{
    // ---------------------------------------------------------------------------
    // MockMainMenuView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockMainMenuView : IMainMenuView
    {
        public event Action OnSettingsClicked;
        public event Action OnPopupClicked;

        public string LastTitleText { get; private set; }
        public int UpdateTitleCallCount { get; private set; }

        public void UpdateTitle(string text)
        {
            LastTitleText = text;
            UpdateTitleCallCount++;
        }

        public void SimulateSettingsClicked() => OnSettingsClicked?.Invoke();
        public void SimulatePopupClicked() => OnPopupClicked?.Invoke();
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
        private ScreenId _lastNavigatedScreen;
        private PopupId _lastShownPopup;
        private int _goBackCallCount;
        private int _dismissCallCount;
        private UIFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
            _lastNavigatedScreen = default;
            _lastShownPopup = default;
            _goBackCallCount = 0;
            _dismissCallCount = 0;

            _factory = new UIFactory(
                _gameService,
                screenId => _lastNavigatedScreen = screenId,
                popupId => _lastShownPopup = popupId,
                () => { _goBackCallCount++; return UniTask.CompletedTask; },
                () => { _dismissCallCount++; return UniTask.CompletedTask; }
            );
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

        // --- Event → callback wiring ---

        [Test]
        public void MainMenuPresenter_SettingsClicked_InvokesNavigateCallback()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            view.SimulateSettingsClicked();

            Assert.AreEqual(ScreenId.Settings, _lastNavigatedScreen,
                "OnSettingsClicked must invoke navigateCallback with ScreenId.Settings");
        }

        [Test]
        public void MainMenuPresenter_PopupClicked_InvokesShowPopupCallback()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();

            view.SimulatePopupClicked();

            Assert.AreEqual(PopupId.ConfirmDialog, _lastShownPopup,
                "OnPopupClicked must invoke showPopupCallback with PopupId.ConfirmDialog");
        }

        [Test]
        public void SettingsPresenter_BackClicked_InvokesGoBackCallback()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();

            view.SimulateBackClicked();

            Assert.AreEqual(1, _goBackCallCount,
                "OnBackClicked must invoke goBackCallback exactly once");
        }

        [Test]
        public void ConfirmDialogPresenter_ConfirmClicked_InvokesDismissCallback()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            view.SimulateConfirmClicked();

            Assert.AreEqual(1, _dismissCallCount,
                "OnConfirmClicked must invoke dismissCallback exactly once");
        }

        [Test]
        public void ConfirmDialogPresenter_CancelClicked_InvokesDismissCallback()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();

            view.SimulateCancelClicked();

            Assert.AreEqual(1, _dismissCallCount,
                "OnCancelClicked must invoke dismissCallback exactly once");
        }

        // --- Dispose unsubscribes ---

        [Test]
        public void MainMenuPresenter_Dispose_UnsubscribesEvents()
        {
            var view = new MockMainMenuView();
            var presenter = _factory.CreateMainMenuPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            _lastNavigatedScreen = default;
            _lastShownPopup = default;
            view.SimulateSettingsClicked();
            view.SimulatePopupClicked();

            Assert.AreEqual(default(ScreenId), _lastNavigatedScreen,
                "After Dispose, OnSettingsClicked must not trigger navigate callback");
            Assert.AreEqual(default(PopupId), _lastShownPopup,
                "After Dispose, OnPopupClicked must not trigger showPopup callback");
        }

        [Test]
        public void SettingsPresenter_Dispose_UnsubscribesEvents()
        {
            var view = new MockSettingsView();
            var presenter = _factory.CreateSettingsPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            int callsBefore = _goBackCallCount;
            view.SimulateBackClicked();

            Assert.AreEqual(callsBefore, _goBackCallCount,
                "After Dispose, OnBackClicked must not trigger goBack callback");
        }

        [Test]
        public void ConfirmDialogPresenter_Dispose_UnsubscribesEvents()
        {
            var view = new MockConfirmDialogView();
            var presenter = _factory.CreateConfirmDialogPresenter(view);
            presenter.Initialize();
            presenter.Dispose();

            int callsBefore = _dismissCallCount;
            view.SimulateConfirmClicked();
            view.SimulateCancelClicked();

            Assert.AreEqual(callsBefore, _dismissCallCount,
                "After Dispose, neither OnConfirmClicked nor OnCancelClicked must trigger dismiss callback");
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
                    typeName.Contains("GameService") || typeName.Contains("SimpleGame.Core.Services"),
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
