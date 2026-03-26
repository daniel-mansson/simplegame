using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// View interface for the first-launch platform link prompt.
    /// Shows the player options to link Game Center, link Google Play Games, or skip.
    /// The popup is shown once and never again if skipped.
    /// </summary>
    public interface IPlatformLinkView : IPopupView
    {
        /// <summary>Fired when the player taps "Link Game Center".</summary>
        event Action OnLinkGameCenterClicked;

        /// <summary>Fired when the player taps "Link Google Play".</summary>
        event Action OnLinkGooglePlayClicked;

        /// <summary>Fired when the player taps "Skip" or dismisses the prompt.</summary>
        event Action OnSkipClicked;

        /// <summary>Updates the displayed link status text for each platform.</summary>
        void UpdateLinkStatus(bool gameCenterLinked, bool googlePlayLinked);
    }
}
