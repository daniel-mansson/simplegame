using System;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.MainMenu
{
    public interface IMainMenuView : IView
    {
        event Action OnSettingsClicked;
        event Action OnPlayClicked;
        event Action OnResetProgressClicked;
        event Action OnNextEnvironmentClicked;
        event Action OnShopClicked;
        event Action OnShopBackClicked;

        /// <summary>
        /// Fired when an object is tapped. The int is the object index
        /// in the current environment's object list.
        /// </summary>
        event Action<int> OnObjectTapped;

        void UpdateEnvironmentName(string text);
        void UpdateBalance(string text);
        void UpdateLevelDisplay(string text);
        void UpdateObjects(ObjectDisplayData[] objects);

        /// <summary>Show or hide the Next Environment button.</summary>
        void SetNextEnvironmentVisible(bool visible);
    }
}
