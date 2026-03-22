using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// PlayFab-backed implementation of <see cref="IPlatformLinkService"/>.
    ///
    /// Game Center (iOS): Uses <c>LoginWithGameCenter</c> / <c>LinkGameCenterAccount</c>.
    ///   - Game Center auth ticket must be obtained via platform-native APIs before calling Link.
    ///   - On a real iOS device, <c>Social.localUser.Authenticate</c> must succeed first.
    ///   - In Play mode on desktop, this will fail gracefully with a NotImplemented error.
    ///
    /// Google Play Games (Android): Uses <c>LinkGooglePlayGamesServicesAccount</c>.
    ///   - Requires the Google Play Games Unity plugin and a server auth code.
    ///   - On a real Android device, the Google Play Games sign-in flow must succeed first.
    ///   - In Play mode on desktop, this will fail gracefully with a NotImplemented error.
    ///
    /// Link status is tracked in-memory and refreshed from PlayFab via <see cref="RefreshLinkStatusAsync"/>.
    /// Persisted across sessions via PlayerPrefs as a lightweight cache (authoritative source is PlayFab).
    /// </summary>
    public class PlayFabPlatformLinkService : IPlatformLinkService
    {
        private const string GameCenterLinkedPrefsKey = "PlayFab_GameCenterLinked";
        private const string GooglePlayLinkedPrefsKey = "PlayFab_GooglePlayLinked";

        private readonly IPlayFabAuthService _auth;

        public bool IsGameCenterLinked { get; private set; }
        public bool IsGooglePlayLinked { get; private set; }

        public PlayFabPlatformLinkService(IPlayFabAuthService auth)
        {
            _auth = auth;
            // Restore cached link status from PlayerPrefs (best-effort, refreshed on login)
            IsGameCenterLinked = UnityEngine.PlayerPrefs.GetInt(GameCenterLinkedPrefsKey, 0) == 1;
            IsGooglePlayLinked = UnityEngine.PlayerPrefs.GetInt(GooglePlayLinkedPrefsKey, 0) == 1;
        }

        /// <inheritdoc/>
        public async UniTask<bool> LinkGameCenterAsync()
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log("[PlatformLink] Skipping Game Center link — not logged in.");
                return false;
            }

#if UNITY_IOS
            // Obtain Game Center identity via native iOS APIs.
            // This requires Social.localUser.Authenticate() to have succeeded.
            // The auth token is then passed to PlayFab for verification.
            var gameCenterId = UnityEngine.Social.localUser.id;
            if (string.IsNullOrEmpty(gameCenterId))
            {
                Debug.LogWarning("[PlatformLink] Game Center user not authenticated — cannot link.");
                return false;
            }

            var request = new LinkGameCenterAccountRequest
            {
                GameCenterId = gameCenterId,
                ForceLink = false
            };

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.LinkGameCenterAccount(request,
                _ =>
                {
                    Debug.Log("[PlatformLink] Game Center linked successfully.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogWarning($"[PlatformLink] Game Center link failed: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            var success = await tcs.Task;
            if (success)
            {
                IsGameCenterLinked = true;
                PersistLinkStatus();
            }
            return success;
#else
            Debug.Log("[PlatformLink] Game Center is only available on iOS.");
            await UniTask.CompletedTask;
            return false;
#endif
        }

        /// <inheritdoc/>
        public async UniTask<bool> LinkGooglePlayAsync()
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log("[PlatformLink] Skipping Google Play link — not logged in.");
                return false;
            }

#if UNITY_ANDROID
            // Obtain a server auth code from Google Play Games Unity plugin.
            // This requires the Google Play Games plugin to be installed and configured.
            // PlayGamesClientConfiguration must be initialized before this call.
            // The server auth code is passed to PlayFab for server-side verification.
            var serverAuthCode = Google.Play.GameServices.PlayGamesPlatform.Instance?.GetServerAuthCode();
            if (string.IsNullOrEmpty(serverAuthCode))
            {
                Debug.LogWarning("[PlatformLink] Google Play Games server auth code unavailable — is the plugin configured?");
                return false;
            }

            var request = new LinkGooglePlayGamesServicesAccountRequest
            {
                ServerAuthCode = serverAuthCode,
                ForceLink = false
            };

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.LinkGooglePlayGamesServicesAccount(request,
                _ =>
                {
                    Debug.Log("[PlatformLink] Google Play Games linked successfully.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogWarning($"[PlatformLink] Google Play Games link failed: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            var success = await tcs.Task;
            if (success)
            {
                IsGooglePlayLinked = true;
                PersistLinkStatus();
            }
            return success;
#else
            Debug.Log("[PlatformLink] Google Play Games is only available on Android.");
            await UniTask.CompletedTask;
            return false;
#endif
        }

        /// <inheritdoc/>
        public async UniTask<bool> UnlinkGameCenterAsync()
        {
            if (!_auth.IsLoggedIn) return false;

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.UnlinkGameCenterAccount(new UnlinkGameCenterAccountRequest(),
                _ =>
                {
                    Debug.Log("[PlatformLink] Game Center unlinked.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogWarning($"[PlatformLink] Game Center unlink failed: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            var success = await tcs.Task;
            if (success)
            {
                IsGameCenterLinked = false;
                PersistLinkStatus();
            }
            return success;
        }

        /// <inheritdoc/>
        public async UniTask<bool> UnlinkGooglePlayAsync()
        {
            if (!_auth.IsLoggedIn) return false;

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.UnlinkGooglePlayGamesServicesAccount(new UnlinkGooglePlayGamesServicesAccountRequest(),
                _ =>
                {
                    Debug.Log("[PlatformLink] Google Play Games unlinked.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogWarning($"[PlatformLink] Google Play Games unlink failed: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            var success = await tcs.Task;
            if (success)
            {
                IsGooglePlayLinked = false;
                PersistLinkStatus();
            }
            return success;
        }

        /// <inheritdoc/>
        public async UniTask RefreshLinkStatusAsync()
        {
            if (!_auth.IsLoggedIn) return;

            var tcs = new UniTaskCompletionSource<GetAccountInfoResult>();
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                result => tcs.TrySetResult(result),
                error =>
                {
                    Debug.LogWarning($"[PlatformLink] GetAccountInfo failed: {error.ErrorMessage}");
                    tcs.TrySetResult(null);
                });

            var result2 = await tcs.Task;
            if (result2?.AccountInfo == null) return;

            IsGameCenterLinked = result2.AccountInfo.GameCenterInfo != null &&
                                  !string.IsNullOrEmpty(result2.AccountInfo.GameCenterInfo.GameCenterId);
            IsGooglePlayLinked = result2.AccountInfo.GooglePlayGamesInfo != null &&
                                  !string.IsNullOrEmpty(result2.AccountInfo.GooglePlayGamesInfo.GooglePlayGamesPlayerId);

            PersistLinkStatus();
            Debug.Log($"[PlatformLink] Status refreshed — GameCenter:{IsGameCenterLinked} GooglePlay:{IsGooglePlayLinked}");
        }

        private void PersistLinkStatus()
        {
            UnityEngine.PlayerPrefs.SetInt(GameCenterLinkedPrefsKey, IsGameCenterLinked ? 1 : 0);
            UnityEngine.PlayerPrefs.SetInt(GooglePlayLinkedPrefsKey, IsGooglePlayLinked ? 1 : 0);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
