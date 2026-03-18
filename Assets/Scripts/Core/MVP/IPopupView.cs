using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.MVP
{
    /// <summary>
    /// Contract for popup views. Extends IView so all popup presenters can use
    /// the shared view contract. Adds async animate-in and animate-out so each
    /// popup can play its own entrance/exit tween.
    ///
    /// Default implementations live in <see cref="PopupViewBase"/>.
    /// Override either method in a concrete view to replace the default.
    /// </summary>
    public interface IPopupView : IView
    {
        /// <summary>
        /// Plays the popup entrance animation and awaits its completion.
        /// Called by the container after SetActive(true).
        /// </summary>
        UniTask AnimateInAsync(CancellationToken ct = default);

        /// <summary>
        /// Plays the popup exit animation and awaits its completion.
        /// Called by the container before SetActive(false).
        /// </summary>
        UniTask AnimateOutAsync(CancellationToken ct = default);
    }
}
