using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Singular MMP implementation of <see cref="ISingularService"/>.
    ///
    /// Delegates to SingularSDK static methods, compiled only when SINGULAR_ENABLED
    /// is set. Falls back to a log-only no-op otherwise so the project always compiles.
    ///
    /// ─── HOW TO ACTIVATE ────────────────────────────────────────────────────────
    /// 1. Install the Singular Unity SDK via Package Manager (see SINGULAR_SETUP.md)
    /// 2. Drag the SingularSDKObject prefab into the Boot scene
    /// 3. Paste SDK Key + Secret into the prefab's Inspector fields
    /// 4. Add SINGULAR_ENABLED to Project Settings → Player → Scripting Define Symbols
    /// 5. Add "Singular" to SimpleGame.Game.asmdef references
    /// ────────────────────────────────────────────────────────────────────────────
    /// </summary>
    public sealed class SingularService : ISingularService
    {
        public void ReportAdRevenue(string networkName, string currency, double revenue)
        {
#if SINGULAR_ENABLED
            if (revenue <= 0 || string.IsNullOrEmpty(currency))
            {
                Debug.LogWarning($"[SingularService] Skipping ad revenue — invalid data: network={networkName} currency={currency} revenue={revenue}");
                return;
            }
            var data = new SingularAdData(networkName, currency, revenue);
            SingularSDK.AdRevenue(data);
            Debug.Log($"[SingularService] AdRevenue reported — network={networkName} currency={currency} revenue={revenue:F6}");
#else
            Debug.Log($"[SingularService] SINGULAR_ENABLED not set — AdRevenue suppressed: network={networkName} revenue={revenue:F6}");
#endif
        }
    }
}
