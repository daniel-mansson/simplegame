using System;
using System.Reflection;
using NUnit.Framework;
using SimpleGame.Core.MVP;
using SimpleGame.Core.Services;

namespace SimpleGame.Tests
{
    // ---------------------------------------------------------------------------
    // MockSampleView: pure test double — no presenter or service references
    // ---------------------------------------------------------------------------
    internal class MockSampleView : ISampleView
    {
        public event Action OnButtonClicked;

        public string LastLabelText { get; private set; }
        public int UpdateLabelCallCount { get; private set; }

        public void UpdateLabel(string text)
        {
            LastLabelText = text;
            UpdateLabelCallCount++;
        }

        public void SimulateButtonClick()
        {
            OnButtonClicked?.Invoke();
        }
    }

    // ---------------------------------------------------------------------------
    // MVP wiring tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class MVPWiringTests
    {
        private GameService _gameService;

        [SetUp]
        public void SetUp()
        {
            _gameService = new GameService();
        }

        [Test]
        public void PresenterCanBeConstructedWithMockView()
        {
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, _gameService);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<SamplePresenter>(presenter);
        }

        [Test]
        public void UIFactoryCreatesSamplePresenterWithService()
        {
            var factory = new UIFactory(_gameService);
            var view = new MockSampleView();

            var presenter = factory.CreateSamplePresenter(view);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<SamplePresenter>(presenter);
        }

        [Test]
        public void PresenterInitializeSetsWelcomeLabel()
        {
            var factory = new UIFactory(_gameService);
            var view = new MockSampleView();
            var presenter = factory.CreateSamplePresenter(view);

            presenter.Initialize();

            Assert.AreEqual(_gameService.GetWelcomeMessage(), view.LastLabelText,
                "Initialize() must set the welcome label via the injected GameService");
        }

        [Test]
        public void PresenterRespondsToViewEvents()
        {
            var factory = new UIFactory(_gameService);
            var view = new MockSampleView();
            var presenter = factory.CreateSamplePresenter(view);

            presenter.Initialize();
            int callsAfterInit = view.UpdateLabelCallCount;

            view.SimulateButtonClick();

            Assert.Greater(view.UpdateLabelCallCount, callsAfterInit,
                "Presenter must call UpdateLabel again when OnButtonClicked fires");
            Assert.AreEqual(_gameService.GetWelcomeMessage(), view.LastLabelText,
                "Label content after click must be the welcome message from GameService");
        }

        [Test]
        public void PresenterDisposeUnsubscribesFromViewEvents()
        {
            var factory = new UIFactory(_gameService);
            var view = new MockSampleView();
            var presenter = factory.CreateSamplePresenter(view);

            presenter.Initialize();
            presenter.Dispose();

            int callsAfterDispose = view.UpdateLabelCallCount;

            // Trigger the event — disposed presenter must NOT respond
            view.SimulateButtonClick();

            Assert.AreEqual(callsAfterDispose, view.UpdateLabelCallCount,
                "UpdateLabel must not be called after Dispose — event subscription should be removed");
        }

        [Test]
        public void MockViewHasNoPresenterReference()
        {
            var mockType = typeof(MockSampleView);
            var allFields = mockType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var field in allFields)
            {
                var fieldType = field.FieldType;
                var typeName = fieldType.FullName ?? fieldType.Name;

                Assert.IsFalse(
                    typeName.Contains("Presenter"),
                    $"MockSampleView field '{field.Name}' references a Presenter type: {typeName}");

                Assert.IsFalse(
                    typeName.Contains("GameService") || typeName.Contains("SimpleGame.Core.Services"),
                    $"MockSampleView field '{field.Name}' references a Services type: {typeName}");

                Assert.IsFalse(
                    typeName.Contains("UIFactory"),
                    $"MockSampleView field '{field.Name}' references UIFactory: {typeName}");
            }

            // Verify MockSampleView does not inherit from any Presenter type
            var baseType = mockType.BaseType;
            Assert.IsFalse(
                baseType != null &&
                (baseType.Name.Contains("Presenter") || (baseType.FullName?.Contains("Presenter") ?? false)),
                "MockSampleView must not inherit from any Presenter type");
        }
    }
}
