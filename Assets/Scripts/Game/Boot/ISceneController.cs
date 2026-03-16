using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Game;

namespace SimpleGame.Game.Boot
{
    /// <summary>
    /// Contract for a per-scene async controller. RunAsync loops internally
    /// handling all in-scene actions (including inline popups) and returns
    /// only when navigation away is decided. The return value is the screen
    /// to navigate to next.
    /// </summary>
    public interface ISceneController
    {
        UniTask<ScreenId> RunAsync(CancellationToken ct = default);
    }
}
