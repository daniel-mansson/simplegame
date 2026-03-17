using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for <see cref="HeartService"/>.
    /// Pure in-memory — no Unity runtime dependencies.
    /// </summary>
    [TestFixture]
    public class HeartServiceTests
    {
        // --- Initial state ---

        [Test]
        public void NewService_ZeroHearts()
        {
            var svc = new HeartService();
            Assert.AreEqual(0, svc.RemainingHearts);
            Assert.IsFalse(svc.IsAlive);
        }

        // --- Reset ---

        [Test]
        public void Reset_SetsHeartCount()
        {
            var svc = new HeartService();
            svc.Reset(3);
            Assert.AreEqual(3, svc.RemainingHearts);
            Assert.IsTrue(svc.IsAlive);
        }

        [Test]
        public void Reset_OverwritesPreviousCount()
        {
            var svc = new HeartService();
            svc.Reset(5);
            svc.Reset(2);
            Assert.AreEqual(2, svc.RemainingHearts);
        }

        [Test]
        public void Reset_ZeroOrNegative_SetsToZero()
        {
            var svc = new HeartService();
            svc.Reset(3);
            svc.Reset(0);
            Assert.AreEqual(0, svc.RemainingHearts);
            Assert.IsFalse(svc.IsAlive);

            svc.Reset(-1);
            Assert.AreEqual(0, svc.RemainingHearts);
        }

        // --- UseHeart ---

        [Test]
        public void UseHeart_Decrements()
        {
            var svc = new HeartService();
            svc.Reset(3);

            bool result = svc.UseHeart();
            Assert.IsTrue(result);
            Assert.AreEqual(2, svc.RemainingHearts);
        }

        [Test]
        public void UseHeart_MultipleTimes_DecrementsCorrectly()
        {
            var svc = new HeartService();
            svc.Reset(3);

            svc.UseHeart();
            svc.UseHeart();
            Assert.AreEqual(1, svc.RemainingHearts);
            Assert.IsTrue(svc.IsAlive);
        }

        [Test]
        public void UseHeart_LastHeart_BecomesNotAlive()
        {
            var svc = new HeartService();
            svc.Reset(1);

            bool result = svc.UseHeart();
            Assert.IsTrue(result);
            Assert.AreEqual(0, svc.RemainingHearts);
            Assert.IsFalse(svc.IsAlive);
        }

        [Test]
        public void UseHeart_AtZero_ReturnsFalse()
        {
            var svc = new HeartService();
            svc.Reset(1);
            svc.UseHeart(); // Now at 0

            bool result = svc.UseHeart();
            Assert.IsFalse(result);
            Assert.AreEqual(0, svc.RemainingHearts);
        }

        [Test]
        public void UseHeart_NeverReset_ReturnsFalse()
        {
            var svc = new HeartService();
            bool result = svc.UseHeart();
            Assert.IsFalse(result);
            Assert.AreEqual(0, svc.RemainingHearts);
        }

        // --- Reset after use ---

        [Test]
        public void Reset_AfterUsingHearts_RestoresCount()
        {
            var svc = new HeartService();
            svc.Reset(3);
            svc.UseHeart();
            svc.UseHeart();
            Assert.AreEqual(1, svc.RemainingHearts);

            svc.Reset(3);
            Assert.AreEqual(3, svc.RemainingHearts);
            Assert.IsTrue(svc.IsAlive);
        }

        [Test]
        public void Reset_AfterDeath_RestoresCount()
        {
            var svc = new HeartService();
            svc.Reset(1);
            svc.UseHeart();
            Assert.IsFalse(svc.IsAlive);

            svc.Reset(3);
            Assert.AreEqual(3, svc.RemainingHearts);
            Assert.IsTrue(svc.IsAlive);
        }

        // --- Full sequence ---

        [Test]
        public void FullSequence_ThreeHearts_DrainToDeath()
        {
            var svc = new HeartService();
            svc.Reset(3);

            Assert.IsTrue(svc.UseHeart());  // 2 remaining
            Assert.IsTrue(svc.UseHeart());  // 1 remaining
            Assert.IsTrue(svc.UseHeart());  // 0 remaining
            Assert.IsFalse(svc.IsAlive);
            Assert.IsFalse(svc.UseHeart()); // Can't use when dead
        }
    }
}
