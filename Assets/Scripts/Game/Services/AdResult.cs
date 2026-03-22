namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Result of an ad show attempt.
    /// </summary>
    public enum AdResult
    {
        /// <summary>The ad played to completion — grant the reward.</summary>
        Completed,

        /// <summary>The ad was skipped by the player — do not grant the reward.</summary>
        Skipped,

        /// <summary>The ad failed during playback (SDK error, connectivity loss).</summary>
        Failed,

        /// <summary>No ad was loaded at the time Show was called — SDK not ready or no fill.</summary>
        NotLoaded,
    }
}
