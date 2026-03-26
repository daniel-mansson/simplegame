using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for cloud save operations backed by PlayFab User Data.
    /// Push serializes <see cref="MetaSaveData"/> to the cloud.
    /// Pull fetches cloud data and returns it for merging with local state.
    ///
    /// All methods are no-ops if the player is not logged in to PlayFab
    /// (<see cref="IPlayFabAuthService.IsLoggedIn"/> == false).
    /// </summary>
    public interface ICloudSaveService
    {
        /// <summary>
        /// Pushes <paramref name="data"/> to PlayFab User Data.
        /// No-op if not logged in.
        /// </summary>
        UniTask PushAsync(MetaSaveData data);

        /// <summary>
        /// Pulls cloud data from PlayFab User Data.
        /// Returns null if not logged in or if no cloud data exists yet.
        /// </summary>
        UniTask<MetaSaveData> PullAsync();
    }
}
