using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for linking/unlinking platform accounts (Game Center, Google Play Games)
    /// to the player's PlayFab anonymous account.
    ///
    /// All methods are no-ops if the player is not logged in to PlayFab.
    /// Link methods return true on success, false on failure (already linked,
    /// platform not available, etc.).
    /// </summary>
    public interface IPlatformLinkService
    {
        /// <summary>Whether the player has linked a Game Center account.</summary>
        bool IsGameCenterLinked { get; }

        /// <summary>Whether the player has linked a Google Play Games account.</summary>
        bool IsGooglePlayLinked { get; }

        /// <summary>
        /// Links the current Game Center identity to the PlayFab account.
        /// Returns true on success.
        /// On iOS, Game Center must be signed in before calling this.
        /// </summary>
        UniTask<bool> LinkGameCenterAsync();

        /// <summary>
        /// Links the current Google Play Games identity to the PlayFab account.
        /// Returns true on success.
        /// On Android, the player must be signed into Google Play Games.
        /// </summary>
        UniTask<bool> LinkGooglePlayAsync();

        /// <summary>Unlinks the Game Center account. Returns true on success.</summary>
        UniTask<bool> UnlinkGameCenterAsync();

        /// <summary>Unlinks the Google Play Games account. Returns true on success.</summary>
        UniTask<bool> UnlinkGooglePlayAsync();

        /// <summary>
        /// Refreshes link status from PlayFab (checks which accounts are linked).
        /// Call on login or after Settings opens to get current state.
        /// </summary>
        UniTask RefreshLinkStatusAsync();
    }
}
