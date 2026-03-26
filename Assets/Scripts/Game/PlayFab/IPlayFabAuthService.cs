using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for PlayFab anonymous authentication.
    /// Implemented by <see cref="PlayFabAuthService"/>.
    /// Mocked in tests.
    /// </summary>
    public interface IPlayFabAuthService
    {
        /// <summary>Whether the player is currently logged in to PlayFab.</summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// The PlayFab Player ID for the current session.
        /// Empty string if not logged in.
        /// </summary>
        string PlayFabId { get; }

        /// <summary>
        /// Logs the player in anonymously using a stable device-derived custom ID.
        /// Creates a new account if none exists for this device.
        /// On success, <see cref="IsLoggedIn"/> becomes true and <see cref="PlayFabId"/> is set.
        /// Safe to call multiple times — subsequent calls re-authenticate the same account.
        /// </summary>
        UniTask LoginAsync();
    }
}
