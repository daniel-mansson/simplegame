using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// View interface for the first-launch consent gate popup.
    ///
    /// Shows the Terms of Service and Privacy Policy links before the player
    /// can proceed. The player MUST tap Accept — there is no close button,
    /// no skip, and no dismiss path. This is intentional (R158, D094).
    /// </summary>
    public interface IConsentGateView : IPopupView
    {
        /// <summary>Fired when the player taps the Accept button.</summary>
        event Action OnAcceptClicked;

        /// <summary>
        /// Enables or disables the Accept button.
        /// Used to block double-taps while the flag is being written.
        /// </summary>
        void SetAcceptInteractable(bool interactable);
    }
}
