using SimpleGame.Game.Meta;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Runtime service that tracks meta world restoration progress.
    /// Loads state from <see cref="IMetaSaveService"/> on initialization,
    /// provides queries for object progress, blocked state, and environment
    /// completion, and persists changes on demand.
    ///
    /// Does NOT own golden piece balance — that is <see cref="GoldenPieceService"/>'s
    /// responsibility. This service only tracks restoration step counts.
    /// </summary>
    public class MetaProgressionService
    {
        private readonly WorldData _worldData;
        private readonly IMetaSaveService _saveService;
        private MetaSaveData _saveData;

        public MetaProgressionService(WorldData worldData, IMetaSaveService saveService)
        {
            _worldData = worldData;
            _saveService = saveService;
            _saveData = _saveService.Load();
        }

        /// <summary>The full world data asset.</summary>
        public WorldData WorldData => _worldData;

        /// <summary>
        /// Gets the current restoration step count for the given object.
        /// Returns 0 if no progress exists.
        /// </summary>
        public int GetCurrentSteps(RestorableObjectData obj)
        {
            return _saveData.GetSteps(obj.ObjectId);
        }

        /// <summary>
        /// Whether the given object is fully restored (current steps == total steps).
        /// </summary>
        public bool IsObjectComplete(RestorableObjectData obj)
        {
            return GetCurrentSteps(obj) >= obj.totalSteps;
        }

        /// <summary>
        /// Whether the given object is blocked. An object is blocked if any
        /// entry in its <c>blockedBy</c> list is not fully restored.
        /// </summary>
        public bool IsBlocked(RestorableObjectData obj)
        {
            if (obj.blockedBy == null || obj.blockedBy.Length == 0)
                return false;

            for (int i = 0; i < obj.blockedBy.Length; i++)
            {
                if (obj.blockedBy[i] != null && !IsObjectComplete(obj.blockedBy[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the given environment is complete (all objects fully restored).
        /// </summary>
        public bool IsEnvironmentComplete(EnvironmentData env)
        {
            if (env.objects == null || env.objects.Length == 0)
                return true;

            for (int i = 0; i < env.objects.Length; i++)
            {
                if (env.objects[i] != null && !IsObjectComplete(env.objects[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to restore one step on the given object.
        /// Fails if the object is blocked, already complete, or null.
        /// Does NOT check or deduct golden pieces — that is the caller's responsibility.
        /// </summary>
        /// <returns>true if the step was applied, false otherwise.</returns>
        public bool TryRestoreStep(RestorableObjectData obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[MetaProgressionService] TryRestoreStep called with null object.");
                return false;
            }

            if (IsBlocked(obj))
            {
                Debug.Log($"[MetaProgressionService] Cannot restore '{obj.displayName}' — blocked by dependencies.");
                return false;
            }

            if (IsObjectComplete(obj))
            {
                Debug.Log($"[MetaProgressionService] '{obj.displayName}' is already fully restored.");
                return false;
            }

            var current = GetCurrentSteps(obj);
            _saveData.SetSteps(obj.ObjectId, current + 1);

            if (IsObjectComplete(obj))
                Debug.Log($"[MetaProgressionService] '{obj.displayName}' fully restored!");

            return true;
        }

        /// <summary>Persist current state to the save service.</summary>
        public void Save()
        {
            // Reload the latest save data to pick up changes made by other
            // services (e.g. GoldenPieceService writing goldenPieces),
            // then apply our object progress before persisting.
            var latest = _saveService.Load();
            latest.objectProgress = _saveData.objectProgress;
            _saveService.Save(latest);
            _saveData = latest;
        }

        /// <summary>
        /// Reload state from the save service. Useful after external changes
        /// (e.g. golden piece service updating the same save data).
        /// </summary>
        public void Reload()
        {
            _saveData = _saveService.Load();
        }

        /// <summary>
        /// Reset all progression (for testing or debug purposes).
        /// Deletes the save and reloads fresh state.
        /// </summary>
        public void ResetAll()
        {
            _saveService.Delete();
            _saveData = new MetaSaveData();
        }
    }
}
