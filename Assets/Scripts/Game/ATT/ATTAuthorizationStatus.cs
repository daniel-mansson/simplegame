namespace SimpleGame.Game.Services
{
    /// <summary>
    /// App Tracking Transparency authorization status values.
    /// Mirrors Apple's ATTrackingManager.AuthorizationStatus.
    /// </summary>
    public enum ATTAuthorizationStatus
    {
        /// <summary>User has not been asked yet (or status is unknown on non-iOS).</summary>
        NotDetermined = 0,

        /// <summary>Authorization restricted (e.g. parental controls).</summary>
        Restricted = 1,

        /// <summary>User denied tracking.</summary>
        Denied = 2,

        /// <summary>User authorized tracking — IDFA is available.</summary>
        Authorized = 3,
    }
}
