using Cysharp.Threading.Tasks;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Contract for iOS App Tracking Transparency (ATT) authorization.
    ///
    /// On iOS: shows the native "Allow Tracking?" system dialog once per install.
    /// On Android / Editor: no-op — returns <see cref="ATTAuthorizationStatus.NotDetermined"/>.
    ///
    /// Per Apple's guidelines, <see cref="RequestAuthorizationAsync"/> must be called
    /// before any SDK that accesses the IDFA (e.g. Unity LevelPlay / ironSource).
    /// The result should not block game progression — ads work in either state.
    ///
    /// Implementations:
    ///   <see cref="UnityATTService"/> — wraps ATTrackingStatusBinding from com.unity.ads.ios-support (#if UNITY_IOS).
    ///   <see cref="NullATTService"/>  — no-op, for Editor and Android.
    /// </summary>
    public interface IATTService
    {
        /// <summary>
        /// Requests ATT authorization from the user.
        ///
        /// On iOS 14.5+: shows the native system dialog if status is <see cref="ATTAuthorizationStatus.NotDetermined"/>.
        /// If already determined, returns the existing status immediately without showing a dialog.
        /// On non-iOS: returns <see cref="ATTAuthorizationStatus.NotDetermined"/> immediately.
        /// </summary>
        UniTask<ATTAuthorizationStatus> RequestAuthorizationAsync();

        /// <summary>Returns the current ATT authorization status without showing a dialog.</summary>
        ATTAuthorizationStatus GetCurrentStatus();
    }
}
