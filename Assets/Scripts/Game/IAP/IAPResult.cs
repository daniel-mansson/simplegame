namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Result returned by <see cref="IIAPService.BuyAsync"/>.
    /// </summary>
    public readonly struct IAPResult
    {
        /// <summary>How the purchase attempt resolved.</summary>
        public readonly IAPOutcome Outcome;

        /// <summary>
        /// Number of coins granted. Non-zero only when <see cref="Outcome"/> is
        /// <see cref="IAPOutcome.Success"/>.
        /// </summary>
        public readonly int CoinsGranted;

        public IAPResult(IAPOutcome outcome, int coinsGranted = 0)
        {
            Outcome = outcome;
            CoinsGranted = coinsGranted;
        }

        /// <summary>Convenience factory for a successful purchase.</summary>
        public static IAPResult Succeeded(int coinsGranted) =>
            new IAPResult(IAPOutcome.Success, coinsGranted);

        /// <summary>Convenience factory for a failed/cancelled purchase.</summary>
        public static IAPResult Failed(IAPOutcome outcome) =>
            new IAPResult(outcome, 0);

        public override string ToString() =>
            Outcome == IAPOutcome.Success
                ? $"IAPResult(Success, +{CoinsGranted} coins)"
                : $"IAPResult({Outcome})";
    }
}
