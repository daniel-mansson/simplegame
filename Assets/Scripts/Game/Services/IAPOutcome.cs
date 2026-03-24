namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Outcome of an in-app purchase attempt.
    /// </summary>
    public enum IAPOutcome
    {
        /// <summary>Purchase succeeded and was validated by PlayFab. Coins have been granted.</summary>
        Success,

        /// <summary>The player dismissed the store sheet without completing the purchase.</summary>
        Cancelled,

        /// <summary>The store rejected the payment (e.g. insufficient funds, card declined).</summary>
        PaymentFailed,

        /// <summary>The store transaction succeeded but PlayFab receipt validation rejected it.</summary>
        ValidationFailed,
    }
}
