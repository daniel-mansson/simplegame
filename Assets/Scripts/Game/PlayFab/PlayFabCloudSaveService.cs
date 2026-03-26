using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// PlayFab-backed implementation of <see cref="ICloudSaveService"/>.
    /// Stores <see cref="MetaSaveData"/> as a single JSON value under the key "MetaSave"
    /// in PlayFab User Data (client-writable, private to the player).
    ///
    /// All calls are guarded by <see cref="IPlayFabAuthService.IsLoggedIn"/>.
    /// Network failures are logged and swallowed — the game continues with local data.
    /// </summary>
    public class PlayFabCloudSaveService : ICloudSaveService
    {
        private const string SaveKey = "MetaSave";

        private readonly IPlayFabAuthService _auth;

        public PlayFabCloudSaveService(IPlayFabAuthService auth)
        {
            _auth = auth;
        }

        /// <inheritdoc/>
        public async UniTask PushAsync(MetaSaveData data)
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log("[CloudSave] Skipping push — not logged in.");
                return;
            }

            var json = UnityEngine.JsonUtility.ToJson(data);
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { SaveKey, json } }
            };

            var tcs = new UniTaskCompletionSource();
            PlayFabError pushError = null;

            PlayFabClientAPI.UpdateUserData(request,
                _ => tcs.TrySetResult(),
                error =>
                {
                    pushError = error;
                    tcs.TrySetResult();
                });

            await tcs.Task;

            if (pushError != null)
                Debug.LogWarning($"[CloudSave] Push failed: {pushError.ErrorMessage}");
            else
                Debug.Log("[CloudSave] Push succeeded.");
        }

        /// <inheritdoc/>
        public async UniTask<MetaSaveData> PullAsync()
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log("[CloudSave] Skipping pull — not logged in.");
                return null;
            }

            var request = new GetUserDataRequest
            {
                Keys = new List<string> { SaveKey }
            };

            var tcs = new UniTaskCompletionSource<GetUserDataResult>();
            PlayFabError pullError = null;

            PlayFabClientAPI.GetUserData(request,
                result => tcs.TrySetResult(result),
                error =>
                {
                    pullError = error;
                    tcs.TrySetResult(null);
                });

            var result2 = await tcs.Task;

            if (pullError != null)
            {
                Debug.LogWarning($"[CloudSave] Pull failed: {pullError.ErrorMessage}");
                return null;
            }

            if (result2 == null || result2.Data == null || !result2.Data.ContainsKey(SaveKey))
            {
                Debug.Log("[CloudSave] No cloud save found — starting fresh.");
                return null;
            }

            var json = result2.Data[SaveKey].Value;
            if (string.IsNullOrEmpty(json))
                return null;

            var cloudData = UnityEngine.JsonUtility.FromJson<MetaSaveData>(json);
            Debug.Log($"[CloudSave] Pull succeeded. savedAt={cloudData?.savedAt}");
            return cloudData;
        }
    }
}
