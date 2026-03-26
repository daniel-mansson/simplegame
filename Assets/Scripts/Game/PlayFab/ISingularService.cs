namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for Singular MMP (Mobile Measurement Partner) integration.
    ///
    /// Used to report ad revenue for UA attribution and ROAS measurement.
    /// The real implementation delegates to SingularSDK static methods.
    /// NullSingularService is used in tests and when Singular is not installed.
    ///
    /// Implementations:
    ///   <see cref="SingularService"/>     — wraps SingularSDK (requires SINGULAR_ENABLED define).
    ///   <see cref="NullSingularService"/> — no-op, no SDK dependency.
    /// </summary>
    public interface ISingularService
    {
        /// <summary>
        /// Reports ad impression revenue to Singular for UA attribution.
        /// Called from the ad SDK's impression-level revenue callback.
        /// </summary>
        /// <param name="networkName">Ad network name (e.g. "UnityAds", "ironSource").</param>
        /// <param name="currency">ISO 4217 currency code (e.g. "USD").</param>
        /// <param name="revenue">Revenue amount in the given currency.</param>
        void ReportAdRevenue(string networkName, string currency, double revenue);
    }
}
