using UnityEngine;

namespace SimpleGame.Game.Meta
{
    /// <summary>
    /// Top-level data asset defining the entire meta world.
    /// Contains an ordered list of <see cref="EnvironmentData"/> entries
    /// that the player progresses through. Environments unlock in mostly
    /// linear order, with occasional 1–3 available simultaneously.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldData", menuName = "PuzzleTap/World Data")]
    public class WorldData : ScriptableObject
    {
        [Tooltip("Ordered list of environments in the meta world")]
        public EnvironmentData[] environments;
    }
}
