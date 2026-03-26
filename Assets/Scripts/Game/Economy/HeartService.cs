using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// In-memory per-level heart tracking. Hearts represent remaining
    /// mistakes allowed during gameplay. Call <see cref="Reset"/> at the
    /// start of each level, <see cref="UseHeart"/> on each incorrect action.
    /// No persistence — hearts reset each level.
    /// </summary>
    public class HeartService : IHeartService
    {
        private int _remaining;

        /// <inheritdoc/>
        public int RemainingHearts => _remaining;

        /// <inheritdoc/>
        public bool IsAlive => _remaining > 0;

        /// <inheritdoc/>
        public void Reset(int count)
        {
            if (count <= 0)
            {
                Debug.LogWarning($"[HeartService] Reset called with non-positive count: {count}");
                _remaining = 0;
                return;
            }

            _remaining = count;
        }

        /// <inheritdoc/>
        public bool UseHeart()
        {
            if (_remaining <= 0)
            {
                Debug.Log("[HeartService] No hearts remaining.");
                return false;
            }

            _remaining--;
            Debug.Log($"[HeartService] Heart used. Remaining: {_remaining}");
            return true;
        }
    }
}
