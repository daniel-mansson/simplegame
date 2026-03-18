using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Serializable data container for meta world save state.
    /// Holds per-object restoration progress and golden piece balance.
    ///
    /// Uses a list of <see cref="ObjectProgress"/> entries instead of
    /// Dictionary because Unity's <c>JsonUtility</c> does not support
    /// Dictionary serialization.
    /// </summary>
    [Serializable]
    public class MetaSaveData
    {
        /// <summary>Golden puzzle piece balance.</summary>
        public int goldenPieces;

        /// <summary>Coin balance (separate from golden pieces; used for Continue and shop).</summary>
        public int coins;

        /// <summary>Per-object restoration progress.</summary>
        public List<ObjectProgress> objectProgress = new List<ObjectProgress>();

        /// <summary>
        /// Gets the current step count for the given object ID.
        /// Returns 0 if the object has no saved progress.
        /// </summary>
        public int GetSteps(string objectId)
        {
            for (int i = 0; i < objectProgress.Count; i++)
            {
                if (objectProgress[i].objectId == objectId)
                    return objectProgress[i].currentSteps;
            }
            return 0;
        }

        /// <summary>
        /// Sets the step count for the given object ID.
        /// Creates a new entry if one doesn't exist.
        /// </summary>
        public void SetSteps(string objectId, int steps)
        {
            for (int i = 0; i < objectProgress.Count; i++)
            {
                if (objectProgress[i].objectId == objectId)
                {
                    var entry = objectProgress[i];
                    entry.currentSteps = steps;
                    objectProgress[i] = entry;
                    return;
                }
            }
            objectProgress.Add(new ObjectProgress { objectId = objectId, currentSteps = steps });
        }
    }

    /// <summary>
    /// One entry of per-object restoration progress.
    /// Serializable by <c>JsonUtility</c>.
    /// </summary>
    [Serializable]
    public struct ObjectProgress
    {
        public string objectId;
        public int currentSteps;
    }
}
