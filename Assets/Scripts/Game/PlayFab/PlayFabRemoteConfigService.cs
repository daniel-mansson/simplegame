using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Fetches <see cref="GameRemoteConfig"/> from PlayFab Title Data.
    ///
    /// Keys stored in PlayFab Game Manager → Title Data (not Player Data):
    ///   "initial_hearts"        → int
    ///   "golden_pieces_per_win" → int
    ///   "continue_cost_coins"   → int
    ///
    /// All keys are optional — missing keys fall back to <see cref="GameRemoteConfig.Default"/>.
    /// Network failures also fall back silently; the game always has playable values.
    ///
    /// Not logged-in guard: if <see cref="IPlayFabAuthService.IsLoggedIn"/> is false,
    /// FetchAsync returns immediately with defaults.
    /// </summary>
    public class PlayFabRemoteConfigService : IRemoteConfigService
    {
        private const string KeyInitialHearts              = "initial_hearts";
        private const string KeyGoldenPiecesPerWin         = "golden_pieces_per_win";
        private const string KeyContinueCostCoins          = "continue_cost_coins";
        private const string KeyInterstitialEveryNLevels   = "interstitial_every_n_levels";

        private readonly IPlayFabAuthService _auth;

        public GameRemoteConfig Config { get; private set; } = GameRemoteConfig.Default;

        public PlayFabRemoteConfigService(IPlayFabAuthService auth)
        {
            _auth = auth;
        }

        public async UniTask FetchAsync()
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.Log("[RemoteConfig] Not logged in — using default config.");
                return;
            }

            var tcs = new UniTaskCompletionSource<GetTitleDataResult>();

            PlayFabClientAPI.GetTitleData(
                new GetTitleDataRequest
                {
                    Keys = new List<string>
                    {
                        KeyInitialHearts,
                        KeyGoldenPiecesPerWin,
                        KeyContinueCostCoins,
                        KeyInterstitialEveryNLevels,
                    }
                },
                result => tcs.TrySetResult(result),
                error =>
                {
                    Debug.LogWarning($"[RemoteConfig] Fetch failed: {error.ErrorMessage} — using defaults.");
                    tcs.TrySetResult(null);
                });

            var result = await tcs.Task;
            if (result?.Data == null)
            {
                Config = GameRemoteConfig.Default;
                return;
            }

            var cfg = GameRemoteConfig.Default;

            if (result.Data.TryGetValue(KeyInitialHearts, out var rawHearts) &&
                int.TryParse(rawHearts, out var hearts) && hearts > 0)
                cfg.InitialHearts = hearts;

            if (result.Data.TryGetValue(KeyGoldenPiecesPerWin, out var rawGolden) &&
                int.TryParse(rawGolden, out var golden) && golden >= 0)
                cfg.GoldenPiecesPerWin = golden;

            if (result.Data.TryGetValue(KeyContinueCostCoins, out var rawContinue) &&
                int.TryParse(rawContinue, out var continueCost) && continueCost >= 0)
                cfg.ContinueCostCoins = continueCost;

            if (result.Data.TryGetValue(KeyInterstitialEveryNLevels, out var rawInterstitial) &&
                int.TryParse(rawInterstitial, out var interstitialN) && interstitialN >= 0)
                cfg.InterstitialEveryNLevels = interstitialN;

            Config = cfg;
            Debug.Log($"[RemoteConfig] Loaded — hearts:{cfg.InitialHearts} golden:{cfg.GoldenPiecesPerWin} continue:{cfg.ContinueCostCoins} interstitialEveryN:{cfg.InterstitialEveryNLevels}");
        }
    }
}
