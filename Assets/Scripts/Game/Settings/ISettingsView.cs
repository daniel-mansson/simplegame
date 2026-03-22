using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Settings
{
    public interface ISettingsView : IView
    {
        event Action OnBackClicked;

        /// <summary>Fired when the player taps "Link Game Center".</summary>
        event Action OnLinkGameCenterClicked;

        /// <summary>Fired when the player taps "Link Google Play".</summary>
        event Action OnLinkGooglePlayClicked;

        /// <summary>Fired when the player taps "Unlink Game Center".</summary>
        event Action OnUnlinkGameCenterClicked;

        /// <summary>Fired when the player taps "Unlink Google Play".</summary>
        event Action OnUnlinkGooglePlayClicked;

        void UpdateTitle(string text);

        /// <summary>Updates the displayed link status for each platform.</summary>
        void UpdateLinkStatus(bool gameCenterLinked, bool googlePlayLinked);
    }
}
