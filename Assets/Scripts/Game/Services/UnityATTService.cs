using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// iOS implementation of <see cref="IATTService"/> using a direct P/Invoke bridge
    /// to Apple's ATTrackingTransparency framework.
    ///
    /// No external package required — calls the native iOS ATT APIs directly via
    /// <c>[DllImport("__Internal")]</c>, the same pattern used by Unity's own ios-support
    /// package internally.
    ///
    /// A companion native source file must be placed in Assets/Plugins/iOS/ to provide
    /// the native implementations:
    ///   - <c>_RequestATTAuthorization()</c>  — calls requestTrackingAuthorization
    ///   - <c>_GetATTAuthorizationStatus()</c> — calls trackingAuthorizationStatus
    ///
    /// See <see cref="Assets/Plugins/iOS/ATTBridge.mm"/> (created alongside this file).
    ///
    /// Lifecycle:
    ///   1. Check current status. If already determined, return immediately.
    ///   2. Call native request, then poll every 100ms until status changes (max 30s).
    /// </summary>
    public sealed class UnityATTService : IATTService
    {
#if UNITY_IOS
        [DllImport("__Internal")] private static extern void _RequestATTAuthorization();
        [DllImport("__Internal")] private static extern int  _GetATTAuthorizationStatus();
#endif

        private const int StatusNotDetermined = 0;
        private const int StatusRestricted    = 1;
        private const int StatusDenied        = 2;
        private const int StatusAuthorized    = 3;

        private const float PollIntervalMs = 100f;
        private const float TimeoutMs      = 30_000f;

        public ATTAuthorizationStatus GetCurrentStatus()
        {
#if UNITY_IOS
            return MapStatus(_GetATTAuthorizationStatus());
#else
            return ATTAuthorizationStatus.NotDetermined;
#endif
        }

        public async UniTask<ATTAuthorizationStatus> RequestAuthorizationAsync()
        {
#if UNITY_IOS
            int current = _GetATTAuthorizationStatus();
            if (current != StatusNotDetermined)
            {
                var existing = MapStatus(current);
                Debug.Log($"[UnityATTService] ATT already determined: {existing}");
                return existing;
            }

            _RequestATTAuthorization();
            Debug.Log("[UnityATTService] ATT dialog shown — polling for user response.");

            float elapsed = 0f;
            while (elapsed < TimeoutMs)
            {
                await UniTask.Delay((int)PollIntervalMs);
                elapsed += PollIntervalMs;

                int status = _GetATTAuthorizationStatus();
                if (status != StatusNotDetermined)
                {
                    var result = MapStatus(status);
                    Debug.Log($"[UnityATTService] ATT result received: {result}");
                    return result;
                }
            }

            Debug.LogWarning("[UnityATTService] ATT poll timed out after 30s — proceeding without IDFA.");
            return ATTAuthorizationStatus.NotDetermined;
#else
            Debug.Log("[UnityATTService] Not running on iOS — ATT skipped.");
            return await UniTask.FromResult(ATTAuthorizationStatus.NotDetermined);
#endif
        }

        private static ATTAuthorizationStatus MapStatus(int native)
        {
            switch (native)
            {
                case StatusAuthorized:   return ATTAuthorizationStatus.Authorized;
                case StatusDenied:       return ATTAuthorizationStatus.Denied;
                case StatusRestricted:   return ATTAuthorizationStatus.Restricted;
                default:                 return ATTAuthorizationStatus.NotDetermined;
            }
        }
    }
}
