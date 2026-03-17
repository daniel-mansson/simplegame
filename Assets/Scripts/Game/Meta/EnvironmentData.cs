using UnityEngine;

namespace SimpleGame.Game.Meta
{
    /// <summary>
    /// Static data for one environment (scenario) in the meta world.
    /// An environment contains multiple <see cref="RestorableObjectData"/>
    /// entries that the player restores by spending golden puzzle pieces.
    ///
    /// The environment is considered complete when all its objects are
    /// fully restored.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnvironment", menuName = "PuzzleTap/Environment")]
    public class EnvironmentData : ScriptableObject
    {
        [Tooltip("Display name of this environment (e.g. 'Garden', 'Town Square')")]
        public string environmentName;

        [Tooltip("All restorable objects in this environment")]
        public RestorableObjectData[] objects;
    }
}
