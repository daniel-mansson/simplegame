namespace SimpleGame.Core.MVP
{
    /// <summary>
    /// Concrete MonoBehaviour that makes <see cref="PopupViewBase"/> usable on popup window
    /// shell prefabs. Provides no additional behaviour beyond the default LitMotion animations
    /// defined in the base class.
    ///
    /// Window shell prefabs (BigPopupWindow, SmallPopupWindow) use this as their animation
    /// component. Concrete popup views (ConfirmDialogView, etc.) inherit PopupViewBase directly
    /// and override AnimateInAsync/AnimateOutAsync if custom animation is needed.
    ///
    /// The <see cref="PopupViewBase._canvasGroup"/> and <see cref="PopupViewBase._panel"/>
    /// fields are wired by PrefabKitSetup when the prefab is created.
    /// </summary>
    public sealed class PopupWindowShell : PopupViewBase { }
}
