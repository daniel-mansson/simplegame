using System;
using System.Reflection;
using NUnit.Framework;
using SimpleGame.Core.MVP;

namespace SimpleGame.Tests.Core
{
    // ---------------------------------------------------------------------------
    // Test fixtures: ISampleView / SamplePresenter (live here, not in runtime)
    // ---------------------------------------------------------------------------

    internal interface ISampleView : IView
    {
        event Action OnButtonClicked;
        void UpdateLabel(string text);
    }

    internal class SamplePresenter : Presenter<ISampleView>
    {
        private readonly string _welcomeMessage;

        public SamplePresenter(ISampleView view, string welcomeMessage) : base(view)
        {
            _welcomeMessage = welcomeMessage;
        }

        public override void Initialize()
        {
            View.OnButtonClicked += HandleButtonClicked;
            View.UpdateLabel(_welcomeMessage);
        }

        public override void Dispose()
        {
            View.OnButtonClicked -= HandleButtonClicked;
        }

        private void HandleButtonClicked()
        {
            View.UpdateLabel(_welcomeMessage);
        }
    }

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
        private const string WelcomeMessage = "Welcome to Simple Game";

        [Test]
        public void PresenterCanBeConstructedWithMockView()
        {
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, WelcomeMessage);

            Assert.IsNotNull(presenter);
            Assert.IsInstanceOf<SamplePresenter>(presenter);
        }

        [Test]
        public void PresenterInitializeSetsWelcomeLabel()
        {
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, WelcomeMessage);

            presenter.Initialize();

            Assert.AreEqual(WelcomeMessage, view.LastLabelText,
                "Initialize() must set the welcome label");
        }

        [Test]
        public void PresenterRespondsToViewEvents()
        {
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, WelcomeMessage);

            presenter.Initialize();
            int callsAfterInit = view.UpdateLabelCallCount;

            view.SimulateButtonClick();

            Assert.Greater(view.UpdateLabelCallCount, callsAfterInit,
                "Presenter must call UpdateLabel again when OnButtonClicked fires");
            Assert.AreEqual(WelcomeMessage, view.LastLabelText,
                "Label content after click must be the welcome message");
        }

        [Test]
        public void PresenterDisposeUnsubscribesFromViewEvents()
        {
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, WelcomeMessage);

            presenter.Initialize();
            presenter.Dispose();

            int callsAfterDispose = view.UpdateLabelCallCount;

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
            }

            var baseType = mockType.BaseType;
            Assert.IsFalse(
                baseType != null &&
                (baseType.Name.Contains("Presenter") || (baseType.FullName?.Contains("Presenter") ?? false)),
                "MockSampleView must not inherit from any Presenter type");
        }

        [Test]
        public void PresenterConstructedWithDifferentMessageRendersCorrectly()
        {
            const string customMessage = "Custom Welcome";
            var view = new MockSampleView();
            var presenter = new SamplePresenter(view, customMessage);

            presenter.Initialize();

            Assert.AreEqual(customMessage, view.LastLabelText,
                "Presenter must render the injected message, not a hardcoded string");
        }
    }
}
