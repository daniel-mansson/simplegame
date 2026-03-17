using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for <see cref="GoldenPieceService"/>.
    /// Uses in-memory mock save service to avoid PlayerPrefs side effects.
    /// </summary>
    [TestFixture]
    public class GoldenPieceServiceTests
    {
        private MockMetaSaveService _saveService;

        [SetUp]
        public void SetUp()
        {
            _saveService = new MockMetaSaveService();
        }

        // --- Initial state ---

        [Test]
        public void NewService_BalanceIsZero()
        {
            var svc = new GoldenPieceService(_saveService);
            Assert.AreEqual(0, svc.Balance);
        }

        [Test]
        public void NewService_LoadsExistingBalance()
        {
            // Pre-seed balance
            var data = new MetaSaveData { goldenPieces = 42 };
            _saveService.Save(data);

            var svc = new GoldenPieceService(_saveService);
            Assert.AreEqual(42, svc.Balance);
        }

        // --- Earn ---

        [Test]
        public void Earn_IncreasesBalance()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            Assert.AreEqual(10, svc.Balance);
        }

        [Test]
        public void Earn_MultipleTimes_Accumulates()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(5);
            svc.Earn(3);
            Assert.AreEqual(8, svc.Balance);
        }

        [Test]
        public void Earn_ZeroAmount_NoChange()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            svc.Earn(0);
            Assert.AreEqual(10, svc.Balance);
        }

        [Test]
        public void Earn_NegativeAmount_NoChange()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            svc.Earn(-5);
            Assert.AreEqual(10, svc.Balance);
        }

        // --- TrySpend ---

        [Test]
        public void TrySpend_SufficientBalance_ReturnsTrue()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            bool result = svc.TrySpend(5);
            Assert.IsTrue(result);
            Assert.AreEqual(5, svc.Balance);
        }

        [Test]
        public void TrySpend_ExactBalance_ReturnsTrue()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            bool result = svc.TrySpend(10);
            Assert.IsTrue(result);
            Assert.AreEqual(0, svc.Balance);
        }

        [Test]
        public void TrySpend_InsufficientBalance_ReturnsFalse()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(3);
            bool result = svc.TrySpend(5);
            Assert.IsFalse(result);
            Assert.AreEqual(3, svc.Balance); // Balance unchanged
        }

        [Test]
        public void TrySpend_ZeroBalance_ReturnsFalse()
        {
            var svc = new GoldenPieceService(_saveService);
            bool result = svc.TrySpend(1);
            Assert.IsFalse(result);
            Assert.AreEqual(0, svc.Balance);
        }

        [Test]
        public void TrySpend_ZeroAmount_ReturnsFalse()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            bool result = svc.TrySpend(0);
            Assert.IsFalse(result);
            Assert.AreEqual(10, svc.Balance);
        }

        // --- Persistence ---

        [Test]
        public void Save_PersistsBalance()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(25);
            svc.Save();

            // Create new service — should load persisted balance
            var svc2 = new GoldenPieceService(_saveService);
            Assert.AreEqual(25, svc2.Balance);
        }

        [Test]
        public void UnsavedChanges_NotPersistedToNewInstance()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(25);
            // Don't call Save()

            var svc2 = new GoldenPieceService(_saveService);
            Assert.AreEqual(0, svc2.Balance);
        }

        [Test]
        public void Save_AfterSpend_PersistsReducedBalance()
        {
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(20);
            svc.TrySpend(8);
            svc.Save();

            var svc2 = new GoldenPieceService(_saveService);
            Assert.AreEqual(12, svc2.Balance);
        }

        [Test]
        public void Save_PreservesObjectProgressFromOtherService()
        {
            // Simulate MetaProgressionService having written objectProgress
            var data = new MetaSaveData();
            data.SetSteps("Fountain", 2);
            _saveService.Save(data);

            // GoldenPieceService earns and saves — should NOT wipe objectProgress
            var svc = new GoldenPieceService(_saveService);
            svc.Earn(10);
            svc.Save();

            // Verify objectProgress survived
            var loaded = _saveService.Load();
            Assert.AreEqual(10, loaded.goldenPieces);
            Assert.AreEqual(2, loaded.GetSteps("Fountain"));
        }

        // --- Helpers ---

        /// <summary>
        /// In-memory mock of <see cref="IMetaSaveService"/> for testing.
        /// </summary>
        private class MockMetaSaveService : IMetaSaveService
        {
            private string _json;

            public void Save(MetaSaveData data)
            {
                _json = UnityEngine.JsonUtility.ToJson(data);
            }

            public MetaSaveData Load()
            {
                if (string.IsNullOrEmpty(_json))
                    return new MetaSaveData();
                return UnityEngine.JsonUtility.FromJson<MetaSaveData>(_json);
            }

            public void Delete()
            {
                _json = null;
            }
        }
    }
}
