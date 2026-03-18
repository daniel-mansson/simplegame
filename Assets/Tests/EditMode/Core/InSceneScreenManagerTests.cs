using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Core.ScreenManagement;
using UnityEngine;

namespace SimpleGame.Tests.Core
{
    // ---------------------------------------------------------------------------
    // TestInSceneScreenId — local enum for InSceneScreenManager tests
    // ---------------------------------------------------------------------------
    internal enum TestInSceneScreenId
    {
        Home,
        Shop,
        Settings
    }

    // ---------------------------------------------------------------------------
    // InSceneScreenManager EditMode tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class InSceneScreenManagerTests
    {
        private GameObject _homePanel;
        private GameObject _shopPanel;
        private GameObject _settingsPanel;
        private InSceneScreenManager<TestInSceneScreenId> _manager;

        [SetUp]
        public void SetUp()
        {
            _homePanel     = new GameObject("HomePanel");
            _shopPanel     = new GameObject("ShopPanel");
            _settingsPanel = new GameObject("SettingsPanel");

            // All panels start inactive
            _homePanel.SetActive(false);
            _shopPanel.SetActive(false);
            _settingsPanel.SetActive(false);

            var panels = new Dictionary<TestInSceneScreenId, GameObject>
            {
                { TestInSceneScreenId.Home,     _homePanel     },
                { TestInSceneScreenId.Shop,     _shopPanel     },
                { TestInSceneScreenId.Settings, _settingsPanel },
            };

            _manager = new InSceneScreenManager<TestInSceneScreenId>(panels);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_homePanel);
            Object.DestroyImmediate(_shopPanel);
            Object.DestroyImmediate(_settingsPanel);
        }

        [Test]
        public void CurrentScreen_IsNull_BeforeAnyShowScreen()
        {
            Assert.IsNull(_manager.CurrentScreen,
                "CurrentScreen must be null on a fresh manager");
        }

        [Test]
        public void CanGoBack_IsFalse_BeforeAnyShowScreen()
        {
            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false on a fresh manager");
        }

        [Test]
        public void ShowScreen_ActivatesTargetPanel()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);

            Assert.IsTrue(_homePanel.activeSelf,
                "Home panel must be active after ShowScreen(Home)");
            Assert.IsFalse(_shopPanel.activeSelf,
                "Shop panel must remain inactive when Home is shown");
        }

        [Test]
        public void ShowScreen_UpdatesCurrentScreen()
        {
            _manager.ShowScreen(TestInSceneScreenId.Shop);

            Assert.AreEqual(TestInSceneScreenId.Shop, _manager.CurrentScreen,
                "CurrentScreen must be Shop after ShowScreen(Shop)");
        }

        [Test]
        public void ShowScreen_DeactivatesPreviousPanel()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Shop);

            Assert.IsFalse(_homePanel.activeSelf,
                "Home panel must be inactive after navigating to Shop");
            Assert.IsTrue(_shopPanel.activeSelf,
                "Shop panel must be active after ShowScreen(Shop)");
        }

        [Test]
        public void ShowScreen_PushesPreviousOntoHistory()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Shop);

            Assert.IsTrue(_manager.CanGoBack,
                "CanGoBack must be true after navigating from Home to Shop");
        }

        [Test]
        public void GoBack_RestoresPreviousScreen()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Shop);
            _manager.GoBack();

            Assert.AreEqual(TestInSceneScreenId.Home, _manager.CurrentScreen,
                "CurrentScreen must be Home after GoBack from Shop");
            Assert.IsTrue(_homePanel.activeSelf,
                "Home panel must be active after GoBack");
            Assert.IsFalse(_shopPanel.activeSelf,
                "Shop panel must be inactive after GoBack");
        }

        [Test]
        public void GoBack_ClearsHistoryWhenOnlyOneEntryRemains()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Shop);
            _manager.GoBack();

            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false after exhausting the back stack");
        }

        [Test]
        public void GoBack_IsNoOp_WhenHistoryEmpty()
        {
            Assert.DoesNotThrow(() => _manager.GoBack(),
                "GoBack on a fresh manager must not throw");

            Assert.IsNull(_manager.CurrentScreen,
                "CurrentScreen must remain null after GoBack on empty stack");
        }

        [Test]
        public void ShowScreen_IsNoOp_WhenAlreadyOnSameScreen()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Home); // second call same screen

            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false — showing the same screen should not push history");
        }

        [Test]
        public void MultiLevel_BackStackNavigationWorks()
        {
            _manager.ShowScreen(TestInSceneScreenId.Home);
            _manager.ShowScreen(TestInSceneScreenId.Shop);
            _manager.ShowScreen(TestInSceneScreenId.Settings);

            Assert.AreEqual(TestInSceneScreenId.Settings, _manager.CurrentScreen);

            _manager.GoBack();
            Assert.AreEqual(TestInSceneScreenId.Shop, _manager.CurrentScreen);

            _manager.GoBack();
            Assert.AreEqual(TestInSceneScreenId.Home, _manager.CurrentScreen);

            Assert.IsFalse(_manager.CanGoBack,
                "CanGoBack must be false after exhausting all history");
        }
    }
}
