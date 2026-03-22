using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public interface IRewardedAdView : IPopupView
    {
        event Action OnWatchClicked;
        event Action OnSkipClicked;
        void UpdateStatus(string text);

        /// <summary>
        /// Enables or disables the Watch button.
        /// Call with <c>false</c> when no ad is loaded — grays out the button
        /// and prevents the player from attempting to show an unavailable ad.
        /// </summary>
        void SetWatchInteractable(bool interactable);
    }
}
