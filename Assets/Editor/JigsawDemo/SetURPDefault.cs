using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Sets the JigsawDemoPipeline as the project's default Scriptable Render Pipeline.
/// The existing game (Boot, MainMenu, InGame) uses only uGUI Canvas which renders
/// correctly under URP — this switch should not break the game flow.
/// Run via: Tools/Setup/Set URP As Default Pipeline
/// </summary>
public static class SetURPDefault
{
    private const string PipelinePath = "Assets/JigsawDemo/JigsawDemoPipeline.asset";

    [MenuItem("Tools/Setup/Set URP As Default Pipeline")]
    public static void SetURP()
    {
        var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            Debug.LogError($"[SetURPDefault] Could not load pipeline asset from: {PipelinePath}");
            return;
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
        AssetDatabase.SaveAssets();

        Debug.Log($"[SetURPDefault] Default render pipeline set to: {PipelinePath}");
    }

    [MenuItem("Tools/Setup/Restore Built-in Pipeline")]
    public static void RestoreBuiltIn()
    {
        GraphicsSettings.defaultRenderPipeline = null;
        EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
        AssetDatabase.SaveAssets();
        Debug.Log("[SetURPDefault] Default render pipeline restored to built-in.");
    }
}
