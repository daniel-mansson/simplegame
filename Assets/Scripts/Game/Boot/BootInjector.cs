using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleGame.Game.Boot
{
    /// <summary>
    /// Ensures the Boot scene is loaded regardless of which scene play mode
    /// started from. Runs after the initial scene loads.
    ///
    /// In a real build, Boot is index 0 and always loads first — this is a no-op.
    /// In the editor, a developer may press Play from MainMenu or Settings:
    /// this detects the missing Boot and loads it additively so GameBootstrapper
    /// can initialize infrastructure before the scene's SceneController runs.
    /// </summary>
    public static class BootInjector
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootLoaded()
        {
            // If Boot is already the active scene (normal play flow), nothing to do.
            if (SceneManager.GetActiveScene().name == "Boot")
                return;

            // Standalone dev scenes that intentionally run without the game boot flow.
            if (SceneManager.GetActiveScene().name == "JigsawDemo")
                return;

            // Check whether Boot is already loaded as an additive scene.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == "Boot")
                    return;
            }

            // Boot is missing — load it additively so GameBootstrapper can run.
            Debug.Log("[BootInjector] Boot scene not present — loading additively.");
            SceneManager.LoadScene("Boot", LoadSceneMode.Additive);
        }
    }
}
