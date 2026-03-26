using System;
using System.Collections.Generic;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Pure static merge logic for <see cref="MetaSaveData"/> cloud sync.
    ///
    /// Strategy: take-max per field. All tracked fields (<c>coins</c>,
    /// <c>goldenPieces</c>, per-object <c>currentSteps</c>) are monotonically
    /// increasing in normal gameplay — taking the max value is always safe and
    /// can never roll back legitimate progress.
    ///
    /// <c>savedAt</c> is set to <c>DateTimeOffset.UtcNow.ToUnixTimeSeconds()</c>
    /// on each merge to stamp when the merged result was produced.
    ///
    /// No Unity dependencies — fully testable in edit-mode.
    /// </summary>
    public static class MetaSaveMerge
    {
        /// <summary>
        /// Merges <paramref name="local"/> and <paramref name="cloud"/> by taking
        /// the maximum value for each field. Either argument may be null
        /// (treated as an empty save with all zeros).
        /// Returns a new <see cref="MetaSaveData"/> containing the merged result.
        /// </summary>
        public static MetaSaveData TakeMax(MetaSaveData local, MetaSaveData cloud)
        {
            var a = local ?? new MetaSaveData();
            var b = cloud ?? new MetaSaveData();

            var merged = new MetaSaveData
            {
                coins = Math.Max(a.coins, b.coins),
                goldenPieces = Math.Max(a.goldenPieces, b.goldenPieces),
                savedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                objectProgress = MergeObjectProgress(a.objectProgress, b.objectProgress)
            };

            return merged;
        }

        private static List<ObjectProgress> MergeObjectProgress(
            List<ObjectProgress> a,
            List<ObjectProgress> b)
        {
            // Build a dictionary from the first list, then merge in the second.
            var result = new Dictionary<string, int>();

            if (a != null)
            {
                foreach (var entry in a)
                    result[entry.objectId] = entry.currentSteps;
            }

            if (b != null)
            {
                foreach (var entry in b)
                {
                    if (result.TryGetValue(entry.objectId, out var existing))
                        result[entry.objectId] = Math.Max(existing, entry.currentSteps);
                    else
                        result[entry.objectId] = entry.currentSteps;
                }
            }

            var list = new List<ObjectProgress>(result.Count);
            foreach (var kv in result)
                list.Add(new ObjectProgress { objectId = kv.Key, currentSteps = kv.Value });

            return list;
        }
    }
}
