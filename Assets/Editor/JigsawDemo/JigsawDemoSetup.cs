using SimpleJigsaw;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor utility that creates the JigsawDemo scene and its required ScriptableObject assets.
/// Run via: Tools/Setup/Create Jigsaw Demo Scene
/// </summary>
public static class JigsawDemoSetup
{
    private const string DemoDir        = "Assets/JigsawDemo";
    private const string GridConfigPath      = "Assets/JigsawDemo/DemoGridConfig.asset";
    private const string RenderConfigPath    = "Assets/JigsawDemo/DemoPieceRenderConfig.asset";
    private const string ScenePath           = "Assets/Scenes/JigsawDemo.unity";

    // ClassicKnobProfile lives in the package — reference by path
    private const string KnobProfilePath = "Packages/com.simple-magic-studios.simple-jigsaw/Runtime/Configs/ClassicKnobProfile.asset";

    [MenuItem("Tools/Setup/Create Jigsaw Demo Scene")]
    public static void CreateJigsawDemoScene()
    {
        // --- 1. Ensure output directory exists ---
        if (!System.IO.Directory.Exists(DemoDir))
        {
            System.IO.Directory.CreateDirectory(DemoDir);
            AssetDatabase.Refresh();
        }

        // --- 2. Load the edge profile and render config from the package ---
        var knobProfile = AssetDatabase.LoadAssetAtPath<EdgeProfileConfig>(KnobProfilePath);
        if (knobProfile == null)
        {
            Debug.LogError($"[JigsawDemoSetup] Could not load ClassicKnobProfile from: {KnobProfilePath}\n" +
                           "Make sure the simple-jigsaw package is registered and Unity has finished importing.");
            return;
        }

        // --- 3. Create DemoGridConfig asset ---
        var gridConfig = AssetDatabase.LoadAssetAtPath<GridLayoutConfig>(GridConfigPath);
        if (gridConfig == null)
        {
            gridConfig = ScriptableObject.CreateInstance<GridLayoutConfig>();
            AssetDatabase.CreateAsset(gridConfig, GridConfigPath);
        }

        gridConfig.Rows         = 4;
        gridConfig.Columns      = 4;
        gridConfig.EdgeProfile  = knobProfile;
        gridConfig.PieceThickness = 0.05f;

        EditorUtility.SetDirty(gridConfig);

        // --- 4. Create DemoPieceRenderConfig asset ---
        var renderConfig = AssetDatabase.LoadAssetAtPath<PieceRenderConfig>(RenderConfigPath);
        if (renderConfig == null)
        {
            renderConfig = ScriptableObject.CreateInstance<PieceRenderConfig>();
            AssetDatabase.CreateAsset(renderConfig, RenderConfigPath);
        }

        renderConfig.PieceShader     = Shader.Find("SimpleJigsaw/PuzzlePiece");
        renderConfig.PieceThickness  = 0.05f;
        renderConfig.EdgeSubdivisions = 20;
        renderConfig.OutlineEnabled  = true;
        renderConfig.OutlineColor    = Color.white;
        renderConfig.OutlineWidth    = 3f;

        EditorUtility.SetDirty(renderConfig);
        AssetDatabase.SaveAssets();
        Debug.Log($"[JigsawDemoSetup] DemoGridConfig saved: {GridConfigPath}");

        // --- 4. Create the demo scene ---
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Remove default directional light — not needed for puzzle pieces
        var defaultLight = GameObject.Find("Directional Light");
        if (defaultLight != null)
            Object.DestroyImmediate(defaultLight);

        // Configure main camera — orthographic, positioned to frame a 4×4 grid centred at origin.
        // GridLayoutConfig produces cells in 0..1 space (GridPlanner normalised coords).
        // Piece SolvedPositions are in that space; PuzzleSceneDriver doesn't scale them.
        // Camera: orthographic size 0.7 fits the full board with a little margin.
        var cameraGo = GameObject.Find("Main Camera");
        if (cameraGo == null)
        {
            cameraGo = new GameObject("Main Camera");
            cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";
        }
        var cam = cameraGo.GetComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 0.7f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.15f, 0.15f, 0.18f, 1f);
        cameraGo.transform.position = new Vector3(0.5f, 0.5f, -10f);

        // Create PuzzleDriver GameObject with PuzzleSceneDriver component
        var driverGo = new GameObject("PuzzleDriver");
        var driver = driverGo.AddComponent<PuzzleSceneDriver>();

        // Wire SerializeField references using SerializedObject for proper asset linkage
        var so = new SerializedObject(driver);
        so.FindProperty("Config").objectReferenceValue       = gridConfig;
        so.FindProperty("RenderConfig").objectReferenceValue = renderConfig;
        so.FindProperty("Seed").intValue                     = 42;
        so.ApplyModifiedPropertiesWithoutUndo();

        // --- 5. Save the scene ---
        var saved = EditorSceneManager.SaveScene(scene, ScenePath);
        if (saved)
            Debug.Log($"[JigsawDemoSetup] Scene saved: {ScenePath}");
        else
            Debug.LogError($"[JigsawDemoSetup] Failed to save scene: {ScenePath}");

        AssetDatabase.Refresh();
        Debug.Log("[JigsawDemoSetup] Done. Open Assets/Scenes/JigsawDemo.unity and press Play.");
    }
}
