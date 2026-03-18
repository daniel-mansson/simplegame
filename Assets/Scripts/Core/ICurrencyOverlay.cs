using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core
{
    /// <summary>
    /// Contract for a contextual currency overlay HUD.
    /// The overlay is shown explicitly by the context that needs it (e.g. LevelFailed popup)
    /// and hidden when that context dismisses.
    ///
    /// Sort order: sits above the popup blocker overlay (100) at sort order 120,
    /// but below active stacked popups (150+). Visible over non-stacked popups.
    /// </summary>
    public interface ICurrencyOverlay
    {
        /// <summary>Fade the overlay in and display it.</summary>
        UniTask ShowAsync(CancellationToken ct = default);

        /// <summary>Fade the overlay out and hide it.</summary>
        UniTask HideAsync(CancellationToken ct = default);

        /// <summary>Update the displayed currency balance text.</summary>
        void UpdateBalance(string text);
    }
}
