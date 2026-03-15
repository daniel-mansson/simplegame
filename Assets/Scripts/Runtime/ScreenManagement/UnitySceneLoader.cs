using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.ScreenManagement;
using UnityEngine.SceneManagement;

namespace SimpleGame.Runtime.ScreenManagement
{
    /// <summary>
    /// Production implementation of ISceneLoader that delegates to Unity's
    /// SceneManager using additive scene loading mode.
    /// </summary>
    public class UnitySceneLoader : ISceneLoader
    {
        public async UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: ct);
        }

        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            await SceneManager.UnloadSceneAsync(sceneName)
                .ToUniTask(cancellationToken: ct);
        }
    }
}
