using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates a minimal URP pipeline asset + renderer asset for the JigsawDemo scene
/// and wires the JigsawDemo camera to use it.
/// Run via: Tools/Setup/Setup JigsawDemo URP
/// </summary>
public static class JigsawDemoURPSetup
{
    private const string URPDir          = "Assets/JigsawDemo";
    private const string RendererPath    = "Assets/JigsawDemo/JigsawDemoRenderer.asset";
    private const string PipelinePath    = "Assets/JigsawDemo/JigsawDemoPipeline.asset";
    private const string ScenePath       = "Assets/Scenes/JigsawDemo.unity";

    [MenuItem("Tools/Setup/Setup JigsawDemo URP")]
    public static void SetupURP()
    {
        // --- 1. Create URP renderer asset ---
        var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(renderer, RendererPath);
        AssetDatabase.SaveAssets();

        // --- 2. Create URP pipeline asset referencing the renderer ---
        var pipeline = UniversalRenderPipelineAsset.Create(renderer);
        AssetDatabase.CreateAsset(pipeline, PipelinePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[JigsawDemoURPSetup] URP assets created:\n  Renderer: {RendererPath}\n  Pipeline: {PipelinePath}");

        // --- 3. Open JigsawDemo scene and wire the camera ---
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var cameraGo = GameObject.Find("Main Camera");
        if (cameraGo == null)
        {
            Debug.LogError("[JigsawDemoURPSetup] Could not find Main Camera in scene.");
            return;
        }

        // Add UniversalAdditionalCameraData if not present
        var cameraData = cameraGo.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
            cameraData = cameraGo.AddComponent<UniversalAdditionalCameraData>();

        // Set the renderer to use the JigsawDemo renderer (index -1 = default, use SetRenderer)
        cameraData.SetRenderer(0);

        // Save the scene
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();

        Debug.Log("[JigsawDemoURPSetup] Camera wired to URP renderer. " +
                  "NOTE: You must also set the JigsawDemo pipeline asset as the override " +
                  "in Edit > Project Settings > Quality for the JigsawDemo scene, " +
                  "OR set it as the default render pipeline in Edit > Project Settings > Graphics.");
    }
}
