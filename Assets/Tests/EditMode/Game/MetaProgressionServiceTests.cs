using System.Collections.Generic;
using NUnit.Framework;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for <see cref="MetaProgressionService"/>,
    /// <see cref="MetaSaveData"/>, and persistence round-trips.
    /// Uses in-memory mock save service to avoid PlayerPrefs side effects.
    /// </summary>
    [TestFixture]
    public class MetaProgressionServiceTests
    {
        private RestorableObjectData _fountain;
        private RestorableObjectData _bench;
        private RestorableObjectData _gazebo; // blocked by fountain
        private EnvironmentData _garden;
        private WorldData _worldData;
        private MockMetaSaveService _saveService;

        [SetUp]
        public void SetUp()
        {
            _fountain = CreateObject("Fountain", totalSteps: 3, costPerStep: 1, blockedBy: null);
            _bench = CreateObject("Bench", totalSteps: 2, costPerStep: 1, blockedBy: null);
            _gazebo = CreateObject("Gazebo", totalSteps: 4, costPerStep: 2, blockedBy: new[] { _fountain });

            _garden = ScriptableObject.CreateInstance<EnvironmentData>();
            _garden.environmentName = "Garden";
            _garden.objects = new[] { _fountain, _bench, _gazebo };

            _worldData = ScriptableObject.CreateInstance<WorldData>();
            _worldData.environments = new[] { _garden };

            _saveService = new MockMetaSaveService();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_fountain);
            UnityEngine.Object.DestroyImmediate(_bench);
            UnityEngine.Object.DestroyImmediate(_gazebo);
            UnityEngine.Object.DestroyImmediate(_garden);
            UnityEngine.Object.DestroyImmediate(_worldData);
        }

        // --- Basic progression ---

        [Test]
        public void NewService_AllObjectsHaveZeroProgress()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.AreEqual(0, svc.GetCurrentSteps(_fountain));
            Assert.AreEqual(0, svc.GetCurrentSteps(_bench));
            Assert.AreEqual(0, svc.GetCurrentSteps(_gazebo));
        }

        [Test]
        public void TryRestoreStep_UnblockedObject_IncrementsProgress()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            bool result = svc.TryRestoreStep(_fountain);
            Assert.IsTrue(result);
            Assert.AreEqual(1, svc.GetCurrentSteps(_fountain));
        }

        [Test]
        public void TryRestoreStep_MultipleTimes_TracksCorrectly()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            svc.TryRestoreStep(_fountain);
            svc.TryRestoreStep(_fountain);
            Assert.AreEqual(2, svc.GetCurrentSteps(_fountain));
        }

        [Test]
        public void TryRestoreStep_AlreadyComplete_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            for (int i = 0; i < _fountain.totalSteps; i++)
                svc.TryRestoreStep(_fountain);

            Assert.IsTrue(svc.IsObjectComplete(_fountain));
            Assert.IsFalse(svc.TryRestoreStep(_fountain));
        }

        // --- Blocked state ---

        [Test]
        public void IsBlocked_NoBlockers_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.IsFalse(svc.IsBlocked(_fountain));
            Assert.IsFalse(svc.IsBlocked(_bench));
        }

        [Test]
        public void IsBlocked_BlockerNotComplete_ReturnsTrue()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.IsTrue(svc.IsBlocked(_gazebo));
        }

        [Test]
        public void IsBlocked_BlockerComplete_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            for (int i = 0; i < _fountain.totalSteps; i++)
                svc.TryRestoreStep(_fountain);

            Assert.IsFalse(svc.IsBlocked(_gazebo));
        }

        [Test]
        public void TryRestoreStep_BlockedObject_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.IsFalse(svc.TryRestoreStep(_gazebo));
            Assert.AreEqual(0, svc.GetCurrentSteps(_gazebo));
        }

        [Test]
        public void TryRestoreStep_BlockedThenUnblocked_Works()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);

            // Blocked initially
            Assert.IsFalse(svc.TryRestoreStep(_gazebo));

            // Complete the blocker
            for (int i = 0; i < _fountain.totalSteps; i++)
                svc.TryRestoreStep(_fountain);

            // Now unblocked
            Assert.IsTrue(svc.TryRestoreStep(_gazebo));
            Assert.AreEqual(1, svc.GetCurrentSteps(_gazebo));
        }

        // --- Environment completion ---

        [Test]
        public void IsEnvironmentComplete_NothingRestored_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.IsFalse(svc.IsEnvironmentComplete(_garden));
        }

        [Test]
        public void IsEnvironmentComplete_AllRestored_ReturnsTrue()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);

            // Complete fountain
            for (int i = 0; i < _fountain.totalSteps; i++)
                svc.TryRestoreStep(_fountain);
            // Complete bench
            for (int i = 0; i < _bench.totalSteps; i++)
                svc.TryRestoreStep(_bench);
            // Now gazebo is unblocked — complete it
            for (int i = 0; i < _gazebo.totalSteps; i++)
                svc.TryRestoreStep(_gazebo);

            Assert.IsTrue(svc.IsEnvironmentComplete(_garden));
        }

        [Test]
        public void IsEnvironmentComplete_PartiallyRestored_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            for (int i = 0; i < _fountain.totalSteps; i++)
                svc.TryRestoreStep(_fountain);

            Assert.IsFalse(svc.IsEnvironmentComplete(_garden));
        }

        // --- Persistence ---

        [Test]
        public void SaveAndReload_PreservesProgress()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            svc.TryRestoreStep(_fountain);
            svc.TryRestoreStep(_fountain);
            svc.TryRestoreStep(_bench);
            svc.Save();

            // Create a new service with the same save service
            var svc2 = new MetaProgressionService(_worldData, _saveService);
            Assert.AreEqual(2, svc2.GetCurrentSteps(_fountain));
            Assert.AreEqual(1, svc2.GetCurrentSteps(_bench));
            Assert.AreEqual(0, svc2.GetCurrentSteps(_gazebo));
        }

        [Test]
        public void ResetAll_ClearsAllProgress()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            svc.TryRestoreStep(_fountain);
            svc.Save();
            svc.ResetAll();

            Assert.AreEqual(0, svc.GetCurrentSteps(_fountain));
        }

        // --- MetaSaveData unit tests ---

        [Test]
        public void MetaSaveData_GetSteps_UnknownId_ReturnsZero()
        {
            var data = new MetaSaveData();
            Assert.AreEqual(0, data.GetSteps("nonexistent"));
        }

        [Test]
        public void MetaSaveData_SetSteps_CreatesEntry()
        {
            var data = new MetaSaveData();
            data.SetSteps("obj1", 5);
            Assert.AreEqual(5, data.GetSteps("obj1"));
        }

        [Test]
        public void MetaSaveData_SetSteps_UpdatesExistingEntry()
        {
            var data = new MetaSaveData();
            data.SetSteps("obj1", 3);
            data.SetSteps("obj1", 7);
            Assert.AreEqual(7, data.GetSteps("obj1"));
        }

        // --- Null safety ---

        [Test]
        public void TryRestoreStep_NullObject_ReturnsFalse()
        {
            var svc = new MetaProgressionService(_worldData, _saveService);
            Assert.IsFalse(svc.TryRestoreStep(null));
        }

        // --- Helpers ---

        private static RestorableObjectData CreateObject(string displayName, int totalSteps, int costPerStep,
            RestorableObjectData[] blockedBy)
        {
            var obj = ScriptableObject.CreateInstance<RestorableObjectData>();
            obj.name = displayName; // name is used as ObjectId
            obj.displayName = displayName;
            obj.totalSteps = totalSteps;
            obj.costPerStep = costPerStep;
            obj.blockedBy = blockedBy ?? new RestorableObjectData[0];
            return obj;
        }

        /// <summary>
        /// In-memory mock of <see cref="IMetaSaveService"/> for testing.
        /// Avoids PlayerPrefs side effects in edit-mode tests.
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
