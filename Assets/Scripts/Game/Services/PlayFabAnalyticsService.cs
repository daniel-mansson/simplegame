using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// PlayFab-backed implementation of <see cref="IAnalyticsService"/>.
    /// Sends custom player events via <c>PlayFabClientAPI.WritePlayerEvent</c>.
    ///
    /// All calls are no-ops if <see cref="IPlayFabAuthService.IsLoggedIn"/> is false.
    /// Network failures are logged and swallowed — analytics is non-critical.
    ///
    /// Events are fire-and-forget: no UniTask wrapping needed since we don't
    /// need to await the result.
    /// </summary>
    public class PlayFabAnalyticsService : IAnalyticsService
    {
        private readonly IPlayFabAuthService _auth;

        public PlayFabAnalyticsService(IPlayFabAuthService auth)
        {
            _auth = auth;
        }

        public void TrackSessionStart() => Send("session_start", null);
        public void TrackSessionEnd()   => Send("session_end",   null);

        public void TrackLevelStarted(string levelId) =>
            Send("level_started", new Dictionary<string, object> { { "level_id", levelId } });

        public void TrackLevelCompleted(string levelId) =>
            Send("level_completed", new Dictionary<string, object> { { "level_id", levelId } });

        public void TrackLevelFailed(string levelId) =>
            Send("level_failed", new Dictionary<string, object> { { "level_id", levelId } });

        public void TrackCurrencyEarned(string currency, int amount) =>
            Send("currency_earned", new Dictionary<string, object>
            {
                { "currency", currency },
                { "amount",   amount   }
            });

        public void TrackCurrencySpent(string currency, int amount) =>
            Send("currency_spent", new Dictionary<string, object>
            {
                { "currency", currency },
                { "amount",   amount   }
            });

        public void TrackPlatformLinked(string platform) =>
            Send("platform_account_linked", new Dictionary<string, object> { { "platform", platform } });

        private void Send(string eventName, Dictionary<string, object> body)
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log($"[Analytics] Skipping '{eventName}' — not logged in.");
                return;
            }

            var request = new WriteClientPlayerEventRequest
            {
                EventName = eventName,
                Body      = body
            };

            PlayFabClientAPI.WritePlayerEvent(request,
                _ => Debug.Log($"[Analytics] Event '{eventName}' sent."),
                error => Debug.LogWarning($"[Analytics] Failed to send '{eventName}': {error.ErrorMessage}"));
        }
    }
}
