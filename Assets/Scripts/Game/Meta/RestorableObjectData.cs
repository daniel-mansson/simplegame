using UnityEngine;

namespace SimpleGame.Game.Meta
{
    /// <summary>
    /// Static data for a single restorable object in the meta world.
    /// Each object belongs to an <see cref="EnvironmentData"/> and can be
    /// restored step-by-step by spending golden puzzle pieces.
    ///
    /// The <see cref="blockedBy"/> list defines dependencies — this object
    /// cannot be worked on until all listed objects are fully restored.
    /// Structure is flat (not a tree): any object can block any other
    /// object within the same environment.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRestorableObject", menuName = "PuzzleTap/Restorable Object")]
    public class RestorableObjectData : ScriptableObject
    {
        [Tooltip("Display name shown in the UI")]
        public string displayName;

        [Tooltip("Total number of restoration steps to complete this object")]
        [Min(1)]
        public int totalSteps = 1;

        [Tooltip("Golden puzzle pieces required per restoration step")]
        [Min(1)]
        public int costPerStep = 1;

        [Tooltip("Objects that must be fully restored before this one can be worked on")]
        public RestorableObjectData[] blockedBy;

        /// <summary>
        /// Stable identifier used as the dictionary key in save data.
        /// Uses the asset name so it survives re-ordering and is human-readable.
        /// </summary>
        public string ObjectId => name;
    }
}
