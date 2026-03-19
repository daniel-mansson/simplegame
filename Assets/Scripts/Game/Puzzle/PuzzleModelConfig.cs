using UnityEngine;

namespace SimpleGame.Game.Puzzle
{
    /// <summary>
    /// Configuration asset for <c>PuzzleModel</c>.
    /// Assign to <c>InGameSceneController._puzzleModelConfig</c> in the Inspector.
    ///
    /// Create via: <b>Assets → Create → SimpleGame → Puzzle Model Config</b>
    /// </summary>
    [CreateAssetMenu(
        fileName = "PuzzleModelConfig",
        menuName = "SimpleGame/Puzzle Model Config",
        order    = 10)]
    public sealed class PuzzleModelConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Number of independent slots the player sees. Each slot draws from the shared deck independently. Must be ≥ 1.")]
        private int _slotCount = 3;

        /// <summary>
        /// Number of independent slots. Clamped to a minimum of 1.
        /// </summary>
        public int SlotCount => Mathf.Max(1, _slotCount);
    }
}
