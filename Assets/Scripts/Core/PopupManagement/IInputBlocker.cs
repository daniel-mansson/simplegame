namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Prevents user interaction with the UI while popups are open.
    /// Implementations must use reference counting:
    ///   - Block() increments the count
    ///   - Unblock() decrements it (clamped at 0)
    ///   - IsBlocked returns true when count > 0
    /// This ensures nested show/dismiss calls stay balanced.
    /// </summary>
    public interface IInputBlocker
    {
        /// <summary>Increments the block count and activates input blocking.</summary>
        void Block();

        /// <summary>Decrements the block count; deactivates blocking when count reaches 0.</summary>
        void Unblock();

        /// <summary>True when at least one Block() call is unmatched by Unblock().</summary>
        bool IsBlocked { get; }
    }
}
