using System;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Authenticates the player anonymously via PlayFab using a stable device-derived custom ID.
    /// Persists the resulting PlayFab Player ID in PlayerPrefs for diagnostic logging.
    ///
    /// Uses <c>LoginWithCustomID</c> with <c>CreateAccount = true</c>, so:
    /// - First launch: creates a new anonymous PlayFab account.
    /// - Subsequent launches: recovers the same account via the same device ID.
    ///
    /// The entity token is session-scoped and refreshed on each login call.
    /// Only the PlayFab Player ID is persisted locally (for logging/diagnostics).
    /// </summary>
    public class PlayFabAuthService : IPlayFabAuthService
    {
        private const string PlayerIdPrefsKey = "PlayFab_PlayerId";

        public bool IsLoggedIn { get; private set; }
        public string PlayFabId { get; private set; } = string.Empty;

        /// <summary>
        /// Logs in anonymously. Resolves successfully on login success.
        /// Throws <see cref="PlayFabLoginException"/> on failure.
        /// </summary>
        public async UniTask LoginAsync()
        {
            var customId = GetStableDeviceId();
            var request = new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = false
                }
            };

            var tcs = new UniTaskCompletionSource();
            string resultPlayFabId = null;
            PlayFabError loginError = null;

            PlayFabClientAPI.LoginWithCustomID(request,
                result =>
                {
                    resultPlayFabId = result.PlayFabId;
                    tcs.TrySetResult();
                },
                error =>
                {
                    loginError = error;
                    tcs.TrySetResult();
                });

            await tcs.Task;

            if (loginError != null)
            {
                Debug.LogError($"[PlayFabAuth] Login failed: {loginError.ErrorMessage} (code: {loginError.Error})");
                throw new PlayFabLoginException(loginError.ErrorMessage, loginError.Error);
            }

            PlayFabId = resultPlayFabId;
            IsLoggedIn = true;

            PlayerPrefs.SetString(PlayerIdPrefsKey, PlayFabId);
            PlayerPrefs.Save();

            Debug.Log($"[PlayFabAuth] Logged in. PlayFabId: {PlayFabId}");
        }

        /// <summary>
        /// Returns a stable device identifier suitable for use as a PlayFab Custom ID.
        /// Uses <c>SystemInfo.deviceUniqueIdentifier</c> which is stable across reinstalls
        /// on both iOS and Android.
        /// </summary>
        private static string GetStableDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }

    /// <summary>
    /// Thrown when PlayFab login fails.
    /// </summary>
    public class PlayFabLoginException : Exception
    {
        public PlayFabErrorCode ErrorCode { get; }

        public PlayFabLoginException(string message, PlayFabErrorCode errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
