using UnityEngine;
using SimpleJigsaw;

/// <summary>
/// MonoBehaviour that drives a visual jigsaw puzzle inspection scene.
/// Attach this to a GameObject in a Unity scene, assign a GridLayoutConfig asset and
/// (optionally) a texture and PieceRenderConfig, then press Play to see the assembled puzzle.
///
/// Set UseTessellation = true and assign a TessellationConfig to use the tessellation path
/// (hex, TriHex, etc.) instead of the rectangular grid path.
/// </summary>
public class PuzzleSceneDriver : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    public GridLayoutConfig Config;
    public int Seed = 42;

    [Header("Puzzle Configuration -- Tessellation")]
    public TessellationConfig TessConfig;
    public bool UseTessellation = false;

    [Header("Board Shape")]
    public BoardShapeConfig BoardShape;

    [Header("Rendering")]
    public Texture2D PuzzleTexture;
    public PieceRenderConfig RenderConfig;

    private void Start()
    {
        // Build render config at runtime if not assigned in Inspector
        PieceRenderConfig activeConfig = RenderConfig;
        if (activeConfig == null)
        {
            activeConfig = ScriptableObject.CreateInstance<PieceRenderConfig>();
            activeConfig.PieceShader = Shader.Find("SimpleJigsaw/PuzzlePiece");
        }
        if (activeConfig.PieceShader == null)
        {
            Debug.LogError("[PuzzleSceneDriver] PieceRenderConfig.PieceShader is null.");
            return;
        }
        if (PuzzleTexture != null && activeConfig.FrontTexture == null)
        {
            activeConfig.FrontTexture = PuzzleTexture;
        }

        PuzzleBoard board;
        if (UseTessellation)
        {
            if (TessConfig == null)
            {
                Debug.LogError("[PuzzleSceneDriver] TessConfig is null -- assign in Inspector.");
                return;
            }
            // Apply edge subdivision override from render config
            if (TessConfig.EdgeProfile != null)
            {
                var samplesField = TessConfig.EdgeProfile.GetType().GetField("SamplesPerEdge");
                if (samplesField != null)
                    samplesField.SetValue(TessConfig.EdgeProfile, activeConfig.EdgeSubdivisions);
            }
            board = BoardFactory.Generate(TessConfig, BoardShape, Seed);
        }
        else
        {
            if (Config == null)
            {
                Debug.LogError("[PuzzleSceneDriver] Config is null -- assign in Inspector.");
                return;
            }
            // Apply edge subdivision override from render config (existing logic)
            if (Config.EdgeProfile != null)
            {
                var samplesField = Config.EdgeProfile.GetType().GetField("SamplesPerEdge");
                if (samplesField != null)
                    samplesField.SetValue(Config.EdgeProfile, activeConfig.EdgeSubdivisions);
            }
            board = BoardFactory.Generate(Config, BoardShape, Seed);
        }

        PieceObjectFactory.CreateAll(board, activeConfig, transform);
    }

    private void Regenerate()
    {
        // Destroy all child GameObjects (pieces)
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        // Re-run full generation pipeline
        Start();
    }

    private string BuildConfigSummary()
    {
        string tess;
        if (UseTessellation)
        {
            string tileType = TessConfig != null ? TessConfig.TileType.ToString() : "?";
            int rings = TessConfig != null ? TessConfig.Rings : 0;
            tess = $"{tileType} R{rings}";
        }
        else
        {
            int rows = Config != null ? Config.Rows : 0;
            int cols = Config != null ? Config.Columns : 0;
            tess = $"Rect {rows}x{cols}";
        }

        string shape = BoardShape != null ? BoardShape.ShapeType.ToString() : "None";

        string profileName = "None";
        EdgeProfileConfig profile = UseTessellation
            ? TessConfig?.EdgeProfile
            : Config?.EdgeProfile;
        if (profile != null)
            profileName = profile.GetType().Name.Replace("EdgeProfile", "");

        return $"{tess} | {shape} | {profileName} | Seed:{Seed}";
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 120, 35), "Regenerate"))
            Regenerate();

        var summaryStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };
        GUI.Label(new Rect(10, 50, 500, 25), BuildConfigSummary(), summaryStyle);
    }
}
