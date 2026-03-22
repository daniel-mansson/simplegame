using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// No-op implementation of <see cref="ISingularService"/>.
    /// Used in edit-mode tests and when SINGULAR_ENABLED is not set.
    /// </summary>
    public sealed class NullSingularService : ISingularService
    {
        public void ReportAdRevenue(string networkName, string currency, double revenue)
        {
            Debug.Log($"[NullSingularService] AdRevenue suppressed — network={networkName} currency={currency} revenue={revenue:F6}");
        }
    }
}
