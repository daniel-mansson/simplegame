using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Handles the actual loading, display, and hiding of popup views.
    /// Mirrors ISceneLoader — pure async contract with no Unity types.
    /// </summary>
    public interface IPopupContainer
    {
        UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default);
        UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default);
    }
}
