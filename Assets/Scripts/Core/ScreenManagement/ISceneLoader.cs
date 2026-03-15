using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.ScreenManagement
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default);
        UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default);
    }
}
