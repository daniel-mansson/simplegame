using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// View interface for the Shop popup.
    /// Exposes events for each coin pack purchase and cancel.
    /// The three packs are addressed by index (0, 1, 2) to keep the interface
    /// generic — the presenter decides pack contents.
    /// </summary>
    public interface IShopView : IPopupView
    {
        /// <summary>Fired when the player taps a coin pack button. int = pack index (0, 1, 2).</summary>
        event Action<int> OnPackClicked;

        /// <summary>Fired when the player taps the Cancel / Close button.</summary>
        event Action OnCancelClicked;

        /// <summary>Updates the label on a pack button (e.g. "500 Coins\n€1.99").</summary>
        void UpdatePackLabel(int packIndex, string text);

        /// <summary>Shows or hides a pack button. Hidden buttons are not clickable.</summary>
        void SetPackVisible(int packIndex, bool visible);

        /// <summary>Shows a status message (e.g. "Purchase complete!").</summary>
        void UpdateStatus(string text);
    }
}
