using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Popup;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // Minimal view interface used only in this test file
    // ---------------------------------------------------------------------------
    internal interface ITestView { }

    // ---------------------------------------------------------------------------
    // TestViewComponent: MonoBehaviour that implements ITestView (for GO attachment)
    // ---------------------------------------------------------------------------
    internal class TestViewComponent : MonoBehaviour, ITestView { }

    // ---------------------------------------------------------------------------
    // MockViewResolver: dictionary-based test double implementing IViewResolver.
    // Intended for use by S02 executor agents that need a lightweight resolver
    // without requiring a Unity scene hierarchy.
    // ---------------------------------------------------------------------------
    internal class MockViewResolver : IViewResolver
    {
        private readonly Dictionary<System.Type, object> _views = new();

        public void Register<T>(T view) where T : class => _views[typeof(T)] = view;

        public T Get<T>() where T : class =>
            _views.TryGetValue(typeof(T), out var v) ? (T)v : null;
    }

    // ---------------------------------------------------------------------------
    // UnityViewContainer.Get<T>() tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class ViewContainerGetTests
    {
        private GameObject _containerGO;
        private UnityViewContainer _container;

        [SetUp]
        public void SetUp()
        {
            _containerGO = new GameObject("TestContainer");
            _container = _containerGO.AddComponent<UnityViewContainer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_containerGO);
        }

        [Test]
        public void Get_ReturnsCorrectInterface()
        {
            var childGO = new GameObject("TestChild");
            childGO.transform.SetParent(_containerGO.transform);
            childGO.AddComponent<TestViewComponent>();

            var result = _container.Get<ITestView>();

            Assert.IsNotNull(result, "Expected Get<ITestView>() to return the child component.");
            Assert.IsInstanceOf<TestViewComponent>(result);
        }

        [Test]
        public void Get_ReturnsNull_WhenInterfaceNotFound()
        {
            // No child has ITestView — resolver must return null gracefully
            var result = _container.Get<ITestView>();

            Assert.IsNull(result, "Expected Get<ITestView>() to return null when no child implements the interface.");
        }

        [Test]
        public void Get_FindsInactiveChild()
        {
            var childGO = new GameObject("InactiveChild");
            childGO.transform.SetParent(_containerGO.transform);
            childGO.AddComponent<TestViewComponent>();
            childGO.SetActive(false);

            // GetComponentInChildren<T>(true) must find inactive objects
            var result = _container.Get<ITestView>();

            Assert.IsNotNull(result, "Expected Get<ITestView>() to find component on inactive child (includeInactive=true).");
        }
    }

    // ---------------------------------------------------------------------------
    // MockViewResolver tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class MockViewResolverTests
    {
        [Test]
        public void MockViewResolver_ReturnsRegistered()
        {
            var resolver = new MockViewResolver();
            var view = new MockLevelCompleteView(); // reuse existing mock from PopupTests.cs
            resolver.Register<ILevelCompleteView>(view);

            var result = resolver.Get<ILevelCompleteView>();

            Assert.AreEqual(view, result, "Expected registered view to be returned by Get<T>().");
        }

        [Test]
        public void MockViewResolver_ReturnsNull_WhenNotRegistered()
        {
            var resolver = new MockViewResolver();

            var result = resolver.Get<ILevelCompleteView>();

            Assert.IsNull(result, "Expected Get<T>() to return null for an unregistered type.");
        }
    }
}
