using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor utility that creates the UI prefab kit under Assets/Prefabs/UI/.
/// Run via: Tools/Setup/Create UI Prefab Kit
///
/// Produces:
///   Assets/Prefabs/UI/BigPopupWindow.prefab
///   Assets/Prefabs/UI/SmallPopupWindow.prefab
///   Assets/Prefabs/UI/Buttons/PositiveButton.prefab
///   Assets/Prefabs/UI/Buttons/DestructiveButton.prefab
///   Assets/Prefabs/UI/Buttons/NeutralButton.prefab
///   Assets/Prefabs/UI/Text/TitleText.prefab
///   Assets/Prefabs/UI/Text/BodyText.prefab
///   Assets/Prefabs/UI/Text/ButtonLabel.prefab
///
/// Window prefabs: root has CanvasGroup; child "Panel" is the RectTransform for animation.
/// These match what PopupViewBase._canvasGroup and PopupViewBase._panel expect.
/// SceneSetup instantiates these as prefab instances and then adds the view MonoBehaviour on top.
/// </summary>
public static class PrefabKitSetup
{
    private const string UIPrefabDir         = "Assets/Prefabs/UI";
    private const string ButtonPrefabDir     = "Assets/Prefabs/UI/Buttons";
    private const string TextPrefabDir       = "Assets/Prefabs/UI/Text";

    // Panel rect: (xMin, yMin, xMax, yMax) as anchor fractions
    private static readonly Vector4 BigPanelRect   = new Vector4(0.1f, 0.2f, 0.9f, 0.8f);
    private static readonly Vector4 SmallPanelRect = new Vector4(0.15f, 0.3f, 0.85f, 0.7f);

    private static readonly Color DimColor         = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color PanelColor       = new Color(0.12f, 0.12f, 0.12f, 0.97f);
    private static readonly Color PositiveColor    = new Color(0.18f, 0.55f, 0.22f, 1f);
    private static readonly Color DestructiveColor = new Color(0.65f, 0.18f, 0.18f, 1f);
    private static readonly Color NeutralColor     = new Color(0.35f, 0.35f, 0.40f, 1f);

    [MenuItem("Tools/Setup/Create UI Prefab Kit")]
    public static void CreateUIPrefabKit()
    {
        EnsureDirectories();

        CreateWindowPrefab("BigPopupWindow",   BigPanelRect);
        CreateWindowPrefab("SmallPopupWindow", SmallPanelRect);

        CreateButtonPrefab("PositiveButton",    PositiveColor);
        CreateButtonPrefab("DestructiveButton", DestructiveColor);
        CreateButtonPrefab("NeutralButton",     NeutralColor);

        CreateTextPrefab("TitleText",    36f, FontStyles.Bold);
        CreateTextPrefab("BodyText",     24f, FontStyles.Normal);
        CreateTextPrefab("ButtonLabel",  20f, FontStyles.Normal);

        AssetDatabase.Refresh();
        Debug.Log("[PrefabKitSetup] UI prefab kit created in Assets/Prefabs/UI/.");
    }

    // ── Window prefabs ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a popup window shell prefab.
    /// Root: full-screen, has CanvasGroup (for PopupViewBase._canvasGroup) + dim background Image.
    /// Panel child: centered rect, dark background Image, RectTransform (for PopupViewBase._panel).
    /// </summary>
    private static void CreateWindowPrefab(string name, Vector4 panelAnchors)
    {
        // Root — full screen, CanvasGroup, dim background
        var root = new GameObject(name);
        var rootRect = root.AddComponent<RectTransform>();
        SetStretchRect(rootRect);

        var dimImage = root.AddComponent<Image>();
        dimImage.color = DimColor;
        dimImage.raycastTarget = true;

        var canvasGroup = root.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Panel child — animated region
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(panelAnchors.x, panelAnchors.y);
        panelRect.anchorMax = new Vector2(panelAnchors.z, panelAnchors.w);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage = panel.AddComponent<Image>();
        panelImage.color = PanelColor;
        panelImage.raycastTarget = true;

        // No PopupWindowShell here — SceneSetup adds the concrete view component
        // (ConfirmDialogView, LevelCompleteView, etc.) on the root and wires _canvasGroup/_panel.
        // The prefab is a pure structural template: CanvasGroup root + Panel child.

        SavePrefab(root, UIPrefabDir, name);
        Object.DestroyImmediate(root);
    }

    // ── Button prefabs ────────────────────────────────────────────────────────

    private static void CreateButtonPrefab(string name, Color color)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();

        var image = go.AddComponent<Image>();
        image.color = color;

        go.AddComponent<Button>();

        // TMP label child
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text = name;
        labelTmp.fontSize = 20f;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.fontStyle = FontStyles.Normal;
        var labelRect = labelGO.GetComponent<RectTransform>();
        SetStretchRect(labelRect);

        SavePrefab(go, ButtonPrefabDir, name);
        Object.DestroyImmediate(go);
    }

    // ── Text prefabs ──────────────────────────────────────────────────────────

    private static void CreateTextPrefab(string name, float fontSize, FontStyles style)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = name;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = true;

        SavePrefab(go, TextPrefabDir, name);
        Object.DestroyImmediate(go);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder(UIPrefabDir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        if (!AssetDatabase.IsValidFolder(ButtonPrefabDir))
            AssetDatabase.CreateFolder(UIPrefabDir, "Buttons");
        if (!AssetDatabase.IsValidFolder(TextPrefabDir))
            AssetDatabase.CreateFolder(UIPrefabDir, "Text");
    }

    private static void SavePrefab(GameObject go, string dir, string name)
    {
        var path = $"{dir}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Debug.Log($"[PrefabKitSetup] Saved {path}");
    }

    private static void SetStretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void WireField(Object component, string fieldName, Object value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"[PrefabKitSetup] Field '{fieldName}' not found on {component.GetType().Name}");
        }
    }
}
