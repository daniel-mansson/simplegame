using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // MockMetaSaveService: in-memory save service for CoinsService tests
    // ---------------------------------------------------------------------------
    internal class MockMetaSaveService : IMetaSaveService
    {
        private MetaSaveData _data = new MetaSaveData();

        public void Save(MetaSaveData data) => _data = data;
        public MetaSaveData Load() => _data;
        public void Delete() => _data = new MetaSaveData();
    }

    // ---------------------------------------------------------------------------
    // CoinsService tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class CoinsServiceTests
    {
        private MockMetaSaveService _saveService;
        private CoinsService _coins;

        [SetUp]
        public void SetUp()
        {
            _saveService = new MockMetaSaveService();
            _coins = new CoinsService(_saveService);
        }

        [Test]
        public void Balance_IsZero_OnNewService()
        {
            Assert.AreEqual(0, _coins.Balance, "Balance must be 0 on fresh CoinsService");
        }

        [Test]
        public void Earn_IncreasesBalance()
        {
            _coins.Earn(500);
            Assert.AreEqual(500, _coins.Balance, "Balance must be 500 after Earn(500)");
        }

        [Test]
        public void Earn_AccumulatesMultipleCalls()
        {
            _coins.Earn(500);
            _coins.Earn(1200);
            Assert.AreEqual(1700, _coins.Balance, "Balance must accumulate across Earn calls");
        }

        [Test]
        public void TrySpend_DeductsBalance_WhenSufficient()
        {
            _coins.Earn(500);
            var result = _coins.TrySpend(100);
            Assert.IsTrue(result, "TrySpend must return true when balance is sufficient");
            Assert.AreEqual(400, _coins.Balance, "Balance must be 400 after spending 100 from 500");
        }

        [Test]
        public void TrySpend_ReturnsFalse_WhenInsufficient()
        {
            _coins.Earn(50);
            var result = _coins.TrySpend(100);
            Assert.IsFalse(result, "TrySpend must return false when balance is insufficient");
            Assert.AreEqual(50, _coins.Balance, "Balance must not change when TrySpend fails");
        }

        [Test]
        public void TrySpend_ReturnsFalse_WhenBalanceIsZero()
        {
            var result = _coins.TrySpend(100);
            Assert.IsFalse(result, "TrySpend must return false when balance is zero");
        }

        [Test]
        public void Save_PersistsBalance()
        {
            _coins.Earn(500);
            _coins.Save();

            var freshService = new CoinsService(_saveService);
            Assert.AreEqual(500, freshService.Balance, "Balance must persist after Save() and reload");
        }

        [Test]
        public void ResetAll_SetsBalanceToZero()
        {
            _coins.Earn(500);
            _coins.ResetAll();
            Assert.AreEqual(0, _coins.Balance, "Balance must be 0 after ResetAll()");
        }

        [Test]
        public void Save_DoesNotClobberGoldenPieces()
        {
            // Golden pieces set independently
            var data = _saveService.Load();
            data.goldenPieces = 99;
            _saveService.Save(data);

            _coins.Earn(200);
            _coins.Save();

            var saved = _saveService.Load();
            Assert.AreEqual(99, saved.goldenPieces,
                "Saving coins must not overwrite golden pieces in shared MetaSaveData");
        }
    }
}
