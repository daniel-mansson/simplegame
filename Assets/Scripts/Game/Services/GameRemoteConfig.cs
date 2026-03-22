namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Typed remote configuration values fetched from PlayFab Title Data.
    /// All fields have sane in-code defaults so the game works correctly
    /// when offline or when a key is absent from the remote config.
    ///
    /// PlayFab key names mirror the field names (snake_case in Title Data,
    /// mapped here for clarity).
    ///
    /// A/B test levers:
    ///   initial_hearts      — mistakes allowed per level attempt
    ///   golden_pieces_per_win — reward for completing a level
    ///   continue_cost_coins — coins spent to buy a retry after losing
    /// </summary>
    public struct GameRemoteConfig
    {
        // ── Economy ──────────────────────────────────────────────────────────

        /// <summary>Hearts (lives) the player starts each level attempt with.</summary>
        public int InitialHearts;

        /// <summary>Golden pieces awarded on level completion.</summary>
        public int GoldenPiecesPerWin;

        /// <summary>Coins spent when the player chooses to continue after losing.</summary>
        public int ContinueCostCoins;

        /// <summary>
        /// How many level completions between interstitial ad shows.
        /// An interstitial is shown after every Nth win (session-scoped counter).
        /// Set to 0 to disable interstitials entirely.
        /// </summary>
        public int InterstitialEveryNLevels;

        // ── Defaults (returned when fetch fails or key is absent) ─────────

        public static GameRemoteConfig Default => new GameRemoteConfig
        {
            InitialHearts           = 3,
            GoldenPiecesPerWin      = 5,
            ContinueCostCoins       = 100,
            InterstitialEveryNLevels = 3,
        };
    }
}
