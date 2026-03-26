using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// No-op implementation of <see cref="IATTService"/> for the Unity Editor and Android.
    ///
    /// Always reports <see cref="ATTAuthorizationStatus.NotDetermined"/> — the game
    /// proceeds as if ATT was not requested (ads still work, just without IDFA).
    /// </summary>
    public sealed class NullATTService : IATTService
    {
        public UniTask<ATTAuthorizationStatus> RequestAuthorizationAsync()
            => UniTask.FromResult(ATTAuthorizationStatus.NotDetermined);

        public ATTAuthorizationStatus GetCurrentStatus()
            => ATTAuthorizationStatus.NotDetermined;
    }
}
