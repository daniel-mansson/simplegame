using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor-only utility that creates placeholder scenes and registers them in EditorBuildSettings.
/// Callable via batchmode: -executeMethod SceneSetup.CreateAndRegisterScenes
/// or via the Unity menu: Tools/Setup/Create And Register Scenes
/// </summary>
public static class SceneSetup
{
    private const string ScenesDir = "Assets/Scenes";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string SettingsPath = "Assets/Scenes/Settings.unity";

    [MenuItem("Tools/Setup/Create And Register Scenes")]
    public static void CreateAndRegisterScenes()
    {
        // Ensure Assets/Scenes/ directory exists
        if (!System.IO.Directory.Exists(ScenesDir))
        {
            System.IO.Directory.CreateDirectory(ScenesDir);
            Debug.Log("[SceneSetup] Created directory: " + ScenesDir);
        }

        // Create MainMenu scene
        CreateScene(MainMenuPath, "MainMenu");

        // Create Settings scene
        CreateScene(SettingsPath, "Settings");

        // Register both scenes in EditorBuildSettings
        var buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(MainMenuPath, true),
            new EditorBuildSettingsScene(SettingsPath, true)
        };

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("[SceneSetup] Registered scenes in EditorBuildSettings: MainMenu, Settings");

        // Refresh the asset database so Unity picks up the new files
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetup] Scene setup complete. MainMenu and Settings scenes created and registered.");
    }

    private static void CreateScene(string path, string sceneName)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        bool saved = EditorSceneManager.SaveScene(scene, path);
        if (saved)
        {
            Debug.Log("[SceneSetup] Created scene: " + path);
        }
        else
        {
            Debug.LogError("[SceneSetup] Failed to save scene: " + path);
        }
    }
}
