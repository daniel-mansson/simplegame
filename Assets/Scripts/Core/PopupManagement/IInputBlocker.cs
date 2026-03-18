using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Prevents user interaction with the UI while popups are open.
    /// Implementations must use reference counting:
    ///   - Block() increments the count
    ///   - Unblock() decrements it (clamped at 0)
    ///   - IsBlocked returns true when count > 0
    /// This ensures nested show/dismiss calls stay balanced.
    ///
    /// FadeInAsync / FadeOutAsync animate the visual overlay.
    /// The caller is responsible for timing: Block() before FadeInAsync,
    /// Unblock() before FadeOutAsync (not after).
    /// </summary>
    public interface IInputBlocker
    {
        /// <summary>Increments the block count and activates input blocking.</summary>
        void Block();

        /// <summary>Decrements the block count; deactivates blocking when count reaches 0.</summary>
        void Unblock();

        /// <summary>True when at least one Block() call is unmatched by Unblock().</summary>
        bool IsBlocked { get; }

        /// <summary>
        /// Animates the blocking overlay to fully visible.
        /// Should be awaited concurrently with the popup's AnimateInAsync.
        /// </summary>
        UniTask FadeInAsync(CancellationToken ct = default);

        /// <summary>
        /// Animates the blocking overlay to invisible.
        /// Caller unblocks input BEFORE calling this — the fade plays in the background.
        /// </summary>
        UniTask FadeOutAsync(CancellationToken ct = default);
    }
}
