namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for sending analytics events to PlayFab.
    /// All methods are no-ops if the player is not logged in.
    /// Events use PlayFab's <c>WritePlayerEvent</c> API.
    ///
    /// Event naming convention: snake_case, e.g. "session_start", "level_completed".
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>Fired when the game boots and PlayFab login succeeds.</summary>
        void TrackSessionStart();

        /// <summary>Fired when the app is backgrounded or closed.</summary>
        void TrackSessionEnd();

        /// <summary>Fired when the player begins a puzzle level.</summary>
        void TrackLevelStarted(string levelId);

        /// <summary>Fired when the player completes a puzzle level successfully.</summary>
        void TrackLevelCompleted(string levelId);

        /// <summary>Fired when the player fails a puzzle level (hearts depleted).</summary>
        void TrackLevelFailed(string levelId);

        /// <summary>Fired when any currency is earned. <paramref name="currency"/> is "coins" or "golden_pieces".</summary>
        void TrackCurrencyEarned(string currency, int amount);

        /// <summary>Fired when any currency is spent. <paramref name="currency"/> is "coins" or "golden_pieces".</summary>
        void TrackCurrencySpent(string currency, int amount);

        /// <summary>Fired when the player successfully links a platform account.</summary>
        void TrackPlatformLinked(string platform);

        /// <summary>
        /// Fired when an ad starts playing (impression).
        /// <paramref name="adType"/> is "rewarded" or "interstitial".
        /// </summary>
        void TrackAdImpression(string adType);

        /// <summary>
        /// Fired when an ad plays to completion and the reward is granted.
        /// <paramref name="adType"/> is "rewarded" or "interstitial".
        /// </summary>
        void TrackAdCompleted(string adType);

        /// <summary>
        /// Fired when the player skips an ad before completion.
        /// <paramref name="adType"/> is "rewarded" or "interstitial".
        /// </summary>
        void TrackAdSkipped(string adType);

        /// <summary>
        /// Fired when an ad fails to load (no fill, adblocker, SDK error).
        /// <paramref name="adType"/> is "rewarded" or "interstitial".
        /// </summary>
        void TrackAdFailedToLoad(string adType);
    }
}
