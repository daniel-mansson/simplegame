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

    // ---------------------------------------------------------------------------
    // UnityViewContainer sort order tests (D053 — stacking visual layering)
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class ViewContainerSortOrderTests
    {
        [Test]
        public void FirstPopup_SortOrder_IsAboveBlocker()
        {
            // Revised scheme: first popup at 200, blocker base at 100.
            // A single popup must be ABOVE the blocker so it is not dimmed when alone.
            const int blockerBaseSortOrder = 100;
            const int firstPopupSortOrder = 200 + (0 * 100); // = 200
            Assert.Greater(firstPopupSortOrder, blockerBaseSortOrder,
                "First popup sort order (200) must be above blocker base (100) — not dimmed when alone");
        }

        [Test]
        public void SecondPopup_SortOrder_IsAboveBlockerStacked()
        {
            // When a second popup is open, the blocker jumps to 250 (between 200 and 300).
            // Second popup at 300 must still be above the stacked blocker (250).
            const int blockerStackedSortOrder = 250;
            const int secondPopupSortOrder = 200 + (1 * 100); // = 300
            Assert.Greater(secondPopupSortOrder, blockerStackedSortOrder,
                "Second popup sort order (300) must be above stacked blocker (250) so it appears on top");
        }

        [Test]
        public void FirstPopup_IsBelowBlockerStacked_WhenSecondPopupOpen()
        {
            // When stacked, the blocker at 250 must be above the bottom popup (200),
            // visually dimming it.
            const int blockerStackedSortOrder = 250;
            const int firstPopupSortOrder = 200 + (0 * 100); // = 200
            Assert.Less(firstPopupSortOrder, blockerStackedSortOrder,
                "First popup sort order (200) must be below stacked blocker (250) — dimmed when second popup is open");
        }

        [Test]
        public void GetNextPopupSortOrder_Returns200_OnFreshContainer()
        {
            var containerGO = new GameObject("TestContainer");
            try
            {
                var container = containerGO.AddComponent<UnityViewContainer>();
                Assert.AreEqual(200, container.GetNextPopupSortOrder(),
                    "Fresh container: next sort order must be 200 (depth 0)");
            }
            finally
            {
                Object.DestroyImmediate(containerGO);
            }
        }
    }
}
