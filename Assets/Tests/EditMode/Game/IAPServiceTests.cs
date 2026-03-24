using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    // ---------------------------------------------------------------------------
    // Minimal stub ICoinsService for verifying coin grants
    // ---------------------------------------------------------------------------
    internal class StubCoinsService : ICoinsService
    {
        public int Balance { get; private set; }
        public int EarnCallCount { get; private set; }
        public int SaveCallCount { get; private set; }

        public void Earn(int amount) { Balance += amount; EarnCallCount++; }
        public bool TrySpend(int amount) { if (Balance < amount) return false; Balance -= amount; return true; }
        public void Save() => SaveCallCount++;
        public void ResetAll() => Balance = 0;
    }

    // ---------------------------------------------------------------------------
    // MockIAPService tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class MockIAPServiceTests
    {
        private static IAPMockConfig MakeConfig(IAPOutcome outcome, int coins = 500)
        {
            var config = ScriptableObject.CreateInstance<IAPMockConfig>();
            config.MockOutcome = outcome;
            config.CoinsGranted = coins;
            return config;
        }

        [Test]
        public async Task InitializeAsync_SetsIsInitialized()
        {
            var config = MakeConfig(IAPOutcome.Success);
            var service = new MockIAPService(config);

            Assert.IsFalse(service.IsInitialized);
            await service.InitializeAsync().AsTask();
            Assert.IsTrue(service.IsInitialized);
        }

        [Test]
        public async Task BuyAsync_Success_ReturnsSuccessWithCoins()
        {
            var config = MakeConfig(IAPOutcome.Success, coins: 1200);
            var coins = new StubCoinsService();
            var service = new MockIAPService(config, coins);

            var result = await service.BuyAsync("com.simplegame.coins.1200").AsTask();

            Assert.AreEqual(IAPOutcome.Success, result.Outcome);
            Assert.AreEqual(1200, result.CoinsGranted);
            Assert.AreEqual(1200, coins.Balance);
            Assert.AreEqual(1, coins.EarnCallCount);
            Assert.AreEqual(1, coins.SaveCallCount);
        }

        [Test]
        public async Task BuyAsync_Cancelled_ReturnsCancelledWithZeroCoins()
        {
            var config = MakeConfig(IAPOutcome.Cancelled);
            var coins = new StubCoinsService();
            var service = new MockIAPService(config, coins);

            var result = await service.BuyAsync("com.simplegame.coins.500").AsTask();

            Assert.AreEqual(IAPOutcome.Cancelled, result.Outcome);
            Assert.AreEqual(0, result.CoinsGranted);
            Assert.AreEqual(0, coins.Balance);
            Assert.AreEqual(0, coins.EarnCallCount);
        }

        [Test]
        public async Task BuyAsync_PaymentFailed_ReturnsFailureWithZeroCoins()
        {
            var config = MakeConfig(IAPOutcome.PaymentFailed);
            var coins = new StubCoinsService();
            var service = new MockIAPService(config, coins);

            var result = await service.BuyAsync("com.simplegame.coins.500").AsTask();

            Assert.AreEqual(IAPOutcome.PaymentFailed, result.Outcome);
            Assert.AreEqual(0, result.CoinsGranted);
            Assert.AreEqual(0, coins.Balance);
            Assert.AreEqual(0, coins.EarnCallCount);
        }

        [Test]
        public async Task BuyAsync_ValidationFailed_ReturnsFailureWithZeroCoins()
        {
            var config = MakeConfig(IAPOutcome.ValidationFailed);
            var coins = new StubCoinsService();
            var service = new MockIAPService(config, coins);

            var result = await service.BuyAsync("com.simplegame.coins.500").AsTask();

            Assert.AreEqual(IAPOutcome.ValidationFailed, result.Outcome);
            Assert.AreEqual(0, result.CoinsGranted);
            Assert.AreEqual(0, coins.Balance);
            Assert.AreEqual(0, coins.EarnCallCount);
        }

        [Test]
        public async Task BuyAsync_SuccessWithoutCoinsService_ReturnsSuccessCoins()
        {
            // No ICoinsService injected — result still reports coins granted
            var config = MakeConfig(IAPOutcome.Success, coins: 2500);
            var service = new MockIAPService(config, coins: null);

            var result = await service.BuyAsync("com.simplegame.coins.2500").AsTask();

            Assert.AreEqual(IAPOutcome.Success, result.Outcome);
            Assert.AreEqual(2500, result.CoinsGranted);
        }
    }

    // ---------------------------------------------------------------------------
    // IAPResult struct tests
    // ---------------------------------------------------------------------------
    [TestFixture]
    internal class IAPResultTests
    {
        [Test]
        public void Succeeded_SetsOutcomeAndCoins()
        {
            var r = IAPResult.Succeeded(500);
            Assert.AreEqual(IAPOutcome.Success, r.Outcome);
            Assert.AreEqual(500, r.CoinsGranted);
        }

        [Test]
        public void Failed_SetsOutcomeAndZeroCoins()
        {
            var r = IAPResult.Failed(IAPOutcome.PaymentFailed);
            Assert.AreEqual(IAPOutcome.PaymentFailed, r.Outcome);
            Assert.AreEqual(0, r.CoinsGranted);
        }

        [Test]
        public void ToString_Success_ContainsCoinCount()
        {
            var r = IAPResult.Succeeded(1200);
            StringAssert.Contains("1200", r.ToString());
            StringAssert.Contains("Success", r.ToString());
        }

        [Test]
        public void ToString_Failure_ContainsOutcomeName()
        {
            var r = IAPResult.Failed(IAPOutcome.Cancelled);
            StringAssert.Contains("Cancelled", r.ToString());
        }
    }
}
