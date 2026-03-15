using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.TransitionManagement
{
    /// <summary>
    /// Contract for playing screen transition animations (e.g. fade-to-black).
    /// Implementations are responsible for animating only — input blocking is
    /// handled separately by <see cref="SimpleGame.Core.PopupManagement.IInputBlocker"/>.
    /// </summary>
    public interface ITransitionPlayer
    {
        /// <summary>Plays the "fade out" half of the transition (screen goes dark).</summary>
        UniTask FadeOutAsync(CancellationToken ct = default);

        /// <summary>Plays the "fade in" half of the transition (screen becomes visible).</summary>
        UniTask FadeInAsync(CancellationToken ct = default);
    }
}
