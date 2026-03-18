using System.Collections.Generic;

namespace SimpleGame.Core.ScreenManagement
{
    /// <summary>
    /// Manages in-scene screen navigation by toggling GameObject panels.
    /// Unlike <see cref="ScreenManager{TScreenId}"/>, no scene loading occurs —
    /// panels are pre-existing GameObjects in the same scene, switched via SetActive.
    ///
    /// One screen is active at a time. History stack enables back-navigation.
    /// </summary>
    public interface IInSceneScreenManager<TScreenId> where TScreenId : struct, System.Enum
    {
        /// <summary>The currently active screen, or null if none has been shown.</summary>
        TScreenId? CurrentScreen { get; }

        /// <summary>True when the back stack has at least one entry.</summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Activates the panel for <paramref name="screenId"/>, deactivates the current panel,
        /// and pushes the current screen onto the back stack.
        /// No-ops if <paramref name="screenId"/> is already the current screen.
        /// </summary>
        void ShowScreen(TScreenId screenId);

        /// <summary>
        /// Pops the back stack and returns to the previous screen.
        /// No-ops if the stack is empty.
        /// </summary>
        void GoBack();
    }
}
