using NUnit.Framework;
using SimpleGame.Game.Services;

namespace SimpleGame.Tests.Game
{
    [TestFixture]
    internal class GameSessionServiceTests
    {
        private GameSessionService _session;

        [SetUp]
        public void SetUp()
        {
            _session = new GameSessionService();
        }

        [Test]
        public void DefaultState_AllZeroOrNone()
        {
            Assert.AreEqual(0, _session.CurrentLevelId);
            Assert.AreEqual(0, _session.CurrentScore);
            Assert.AreEqual(GameOutcome.None, _session.Outcome);
        }

        [Test]
        public void ResetForNewGame_SetsLevelId()
        {
            _session.ResetForNewGame(5);
            Assert.AreEqual(5, _session.CurrentLevelId);
        }

        [Test]
        public void ResetForNewGame_ResetsScoreToZero()
        {
            _session.CurrentScore = 42;
            _session.ResetForNewGame(1);
            Assert.AreEqual(0, _session.CurrentScore);
        }

        [Test]
        public void ResetForNewGame_ResetsOutcomeToNone()
        {
            _session.Outcome = GameOutcome.Win;
            _session.ResetForNewGame(1);
            Assert.AreEqual(GameOutcome.None, _session.Outcome);
        }

        [Test]
        public void CurrentScore_ReadWrite()
        {
            _session.CurrentScore = 100;
            Assert.AreEqual(100, _session.CurrentScore);
        }

        [Test]
        public void Outcome_ReadWrite()
        {
            _session.Outcome = GameOutcome.Lose;
            Assert.AreEqual(GameOutcome.Lose, _session.Outcome);

            _session.Outcome = GameOutcome.Win;
            Assert.AreEqual(GameOutcome.Win, _session.Outcome);
        }

        [Test]
        public void ResetForNewGame_CalledTwice_SecondCallOverrides()
        {
            _session.ResetForNewGame(3);
            _session.CurrentScore = 50;
            _session.Outcome = GameOutcome.Win;

            _session.ResetForNewGame(7);

            Assert.AreEqual(7, _session.CurrentLevelId);
            Assert.AreEqual(0, _session.CurrentScore);
            Assert.AreEqual(GameOutcome.None, _session.Outcome);
        }
    }

    [TestFixture]
    internal class ProgressionServiceTests
    {
        private ProgressionService _progression;

        [SetUp]
        public void SetUp()
        {
            _progression = new ProgressionService();
        }

        [Test]
        public void InitialLevel_IsOne()
        {
            Assert.AreEqual(1, _progression.CurrentLevel);
        }

        [Test]
        public void RegisterWin_AdvancesLevel()
        {
            _progression.RegisterWin(10);
            Assert.AreEqual(2, _progression.CurrentLevel);
        }

        [Test]
        public void RegisterWin_CalledMultipleTimes_AdvancesEachTime()
        {
            _progression.RegisterWin(10);
            _progression.RegisterWin(20);
            _progression.RegisterWin(30);
            Assert.AreEqual(4, _progression.CurrentLevel);
        }

        [Test]
        public void RegisterWin_WithZeroScore_StillAdvances()
        {
            _progression.RegisterWin(0);
            Assert.AreEqual(2, _progression.CurrentLevel);
        }

        [Test]
        public void RegisterWin_LogsScore()
        {
            // Debug.Log is called internally — we verify it doesn't throw
            // and that the level advances (observable side effect).
            // Direct log capture would require LogAssert which is play-mode only.
            Assert.DoesNotThrow(() => _progression.RegisterWin(42));
            Assert.AreEqual(2, _progression.CurrentLevel);
        }
    }
}
