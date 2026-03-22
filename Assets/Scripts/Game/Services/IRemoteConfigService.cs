using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Fetches remote configuration from the backend and exposes typed values.
    /// Implementations must be safe to call before gameplay starts and must
    /// fall back to <see cref="GameRemoteConfig.Default"/> on failure.
    /// </summary>
    public interface IRemoteConfigService
    {
        /// <summary>
        /// The currently active config. Returns <see cref="GameRemoteConfig.Default"/>
        /// before <see cref="FetchAsync"/> completes or if fetch failed.
        /// </summary>
        GameRemoteConfig Config { get; }

        /// <summary>
        /// Fetches latest config from the backend.
        /// Safe to call while offline — falls back to defaults on error.
        /// Await this during boot before constructing gameplay services.
        /// </summary>
        UniTask FetchAsync();
    }
}
