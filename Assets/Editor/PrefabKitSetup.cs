using SimpleGame.Core.MVP;
using SimpleGame.Game.Popup;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor utility that creates the UI prefab kit under Assets/Prefabs/UI/.
/// Run via: Tools/Setup/Create UI Prefab Kit
///
/// Layer 1 — primitive component prefabs:
///   BigPopupWindow, SmallPopupWindow
///   Buttons/PositiveButton, DestructiveButton, NeutralButton
///   Text/TitleText, BodyText, ButtonLabel
///
/// Layer 2 — popup prefabs (nested, built on top of layer 1):
///   Popups/ConfirmDialogPopup, LevelCompletePopup, LevelFailedPopup,
///   Popups/RewardedAdPopup, IAPPurchasePopup, ObjectRestoredPopup
///
/// Each popup prefab nests the window shell and button/text component prefabs
/// with live prefab connections. The view MonoBehaviour, _animConfig, and all
/// serialized field refs are wired inside the prefab asset itself.
///
/// SceneSetup instantiates each popup prefab into PopupCanvas — no inline
/// construction, no unpacking.
/// </summary>
public static class PrefabKitSetup
{
    private const string UIPrefabDir     = "Assets/Prefabs/UI";
    private const string ButtonPrefabDir = "Assets/Prefabs/UI/Buttons";
    private const string TextPrefabDir   = "Assets/Prefabs/UI/Text";
    private const string PopupPrefabDir  = "Assets/Prefabs/UI/Popups";
    private const string AnimConfigPath  = "Assets/Data/PopupAnimationConfig.asset";

    // Panel rect: (xMin, yMin, xMax, yMax) as anchor fractions
    private static readonly Vector4 BigPanelRect   = new Vector4(0.1f,  0.2f,  0.9f,  0.8f);
    private static readonly Vector4 SmallPanelRect = new Vector4(0.15f, 0.3f,  0.85f, 0.7f);

    private static readonly Color PanelColor       = new Color(0.12f, 0.12f, 0.12f, 0.97f);
    private static readonly Color PositiveColor    = new Color(0.18f, 0.55f, 0.22f, 1f);
    private static readonly Color DestructiveColor = new Color(0.65f, 0.18f, 0.18f, 1f);
    private static readonly Color NeutralColor     = new Color(0.35f, 0.35f, 0.40f, 1f);

    // ── Menu items ────────────────────────────────────────────────────────────

    [MenuItem("Tools/Setup/Create UI Prefab Kit")]
    public static void CreateUIPrefabKit()
    {
        EnsureDirectories();
        CreateComponentPrefabs();
        AssetDatabase.Refresh();
        Debug.Log("[PrefabKitSetup] UI component prefabs created.");
    }

    [MenuItem("Tools/Setup/Create Popup Prefabs")]
    public static void CreatePopupPrefabsMenu()
    {
        EnsureDirectories();

        var animConfig = LoadOrCreateAnimConfig();

        CreateConfirmDialogPrefab(animConfig);
        CreateLevelCompletePrefab(animConfig);
        CreateLevelFailedPrefab(animConfig);
        CreateRewardedAdPrefab(animConfig);
        CreateIAPPurchasePrefab(animConfig);
        CreateObjectRestoredPrefab(animConfig);

        AssetDatabase.Refresh();
        Debug.Log("[PrefabKitSetup] Popup prefabs created in Assets/Prefabs/UI/Popups/.");
    }

    // ── Component prefab creation ─────────────────────────────────────────────

    private static void CreateComponentPrefabs()
    {
        CreateWindowPrefab("BigPopupWindow",   BigPanelRect);
        CreateWindowPrefab("SmallPopupWindow", SmallPanelRect);

        CreateButtonPrefab("PositiveButton",    PositiveColor);
        CreateButtonPrefab("DestructiveButton", DestructiveColor);
        CreateButtonPrefab("NeutralButton",     NeutralColor);

        CreateTextPrefab("TitleText",   36f, FontStyles.Bold);
        CreateTextPrefab("BodyText",    24f, FontStyles.Normal);
        CreateTextPrefab("ButtonLabel", 20f, FontStyles.Normal);
    }

    // ── Popup prefab creation ─────────────────────────────────────────────────

    private static void CreateConfirmDialogPrefab(PopupAnimationConfig animConfig)
    {
        var bigWindow = LoadPrefab(UIPrefabDir, "BigPopupWindow");
        var root      = InstantiateNested(bigWindow, "ConfirmDialogPopup");
        var panel     = root.transform.Find("Panel");

        var view = root.AddComponent<ConfirmDialogView>();
        WireField(view, "_animConfig",     animConfig);
        WireField(view, "_canvasGroup",    root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",          panel.GetComponent<RectTransform>());
        WireField(view, "_messageText",    AddTextPrefab(panel, "TitleText",     "MessageText",    "Are you sure?",  new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.88f)));
        WireField(view, "_confirmButton",  AddButtonPrefab(panel, "PositiveButton",  "ConfirmButton",  "OK",             new Vector2(0.08f, 0.08f), new Vector2(0.45f, 0.30f)));
        WireField(view, "_cancelButton",   AddButtonPrefab(panel, "NeutralButton",   "CancelButton",   "Cancel",         new Vector2(0.55f, 0.08f), new Vector2(0.92f, 0.30f)));

        SavePopupPrefab(root, "ConfirmDialogPopup");
    }

    private static void CreateLevelCompletePrefab(PopupAnimationConfig animConfig)
    {
        var bigWindow = LoadPrefab(UIPrefabDir, "BigPopupWindow");
        var root      = InstantiateNested(bigWindow, "LevelCompletePopup");
        var panel     = root.transform.Find("Panel");

        var view = root.AddComponent<LevelCompleteView>();
        WireField(view, "_animConfig",       animConfig);
        WireField(view, "_canvasGroup",      root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",            panel.GetComponent<RectTransform>());
        WireField(view, "_levelText",        AddTextPrefab(panel, "TitleText", "LevelText",        "Level Complete!", new Vector2(0.1f,  0.72f), new Vector2(0.9f,  0.92f)));
        WireField(view, "_scoreText",        AddTextPrefab(panel, "BodyText",  "ScoreText",        "Score: 0",        new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.70f)));
        WireField(view, "_goldenPiecesText", AddTextPrefab(panel, "BodyText",  "GoldenPiecesText", "+0 Pieces",       new Vector2(0.15f, 0.34f), new Vector2(0.85f, 0.52f)));
        WireField(view, "_continueButton",   AddButtonPrefab(panel, "PositiveButton", "ContinueButton", "Continue",  new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.30f)));

        SavePopupPrefab(root, "LevelCompletePopup");
    }

    private static void CreateLevelFailedPrefab(PopupAnimationConfig animConfig)
    {
        var bigWindow = LoadPrefab(UIPrefabDir, "BigPopupWindow");
        var root      = InstantiateNested(bigWindow, "LevelFailedPopup");
        var panel     = root.transform.Find("Panel");

        var view = root.AddComponent<LevelFailedView>();
        WireField(view, "_animConfig",    animConfig);
        WireField(view, "_canvasGroup",   root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",         panel.GetComponent<RectTransform>());
        WireField(view, "_levelText",     AddTextPrefab(panel, "TitleText", "LevelText",  "Level Failed!", new Vector2(0.1f,  0.72f), new Vector2(0.9f,  0.92f)));
        WireField(view, "_scoreText",     AddTextPrefab(panel, "BodyText",  "ScoreText",  "Score: 0",      new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.70f)));
        WireField(view, "_retryButton",   AddButtonPrefab(panel, "PositiveButton",   "RetryButton",   "Retry",    new Vector2(0.04f, 0.08f), new Vector2(0.34f, 0.30f)));
        WireField(view, "_watchAdButton", AddButtonPrefab(panel, "NeutralButton",    "WatchAdButton", "Watch Ad", new Vector2(0.37f, 0.08f), new Vector2(0.63f, 0.30f)));
        WireField(view, "_quitButton",    AddButtonPrefab(panel, "DestructiveButton","QuitButton",    "Quit",     new Vector2(0.66f, 0.08f), new Vector2(0.96f, 0.30f)));

        SavePopupPrefab(root, "LevelFailedPopup");
    }

    private static void CreateRewardedAdPrefab(PopupAnimationConfig animConfig)
    {
        var smallWindow = LoadPrefab(UIPrefabDir, "SmallPopupWindow");
        var root        = InstantiateNested(smallWindow, "RewardedAdPopup");
        var panel       = root.transform.Find("Panel");

        var view = root.AddComponent<RewardedAdView>();
        WireField(view, "_animConfig",   animConfig);
        WireField(view, "_canvasGroup",  root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",        panel.GetComponent<RectTransform>());
        WireField(view, "_statusText",   AddTextPrefab(panel, "BodyText", "StatusText", "Watch a short ad for a reward?", new Vector2(0.1f,  0.45f), new Vector2(0.9f,  0.88f)));
        WireField(view, "_watchButton",  AddButtonPrefab(panel, "PositiveButton", "WatchButton", "Watch", new Vector2(0.08f, 0.08f), new Vector2(0.45f, 0.36f)));
        WireField(view, "_skipButton",   AddButtonPrefab(panel, "NeutralButton",  "SkipButton",  "Skip",  new Vector2(0.55f, 0.08f), new Vector2(0.92f, 0.36f)));

        SavePopupPrefab(root, "RewardedAdPopup");
    }

    private static void CreateIAPPurchasePrefab(PopupAnimationConfig animConfig)
    {
        var smallWindow = LoadPrefab(UIPrefabDir, "SmallPopupWindow");
        var root        = InstantiateNested(smallWindow, "IAPPurchasePopup");
        var panel       = root.transform.Find("Panel");

        var view = root.AddComponent<IAPPurchaseView>();
        WireField(view, "_animConfig",      animConfig);
        WireField(view, "_canvasGroup",     root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",           panel.GetComponent<RectTransform>());
        WireField(view, "_itemNameText",    AddTextPrefab(panel, "TitleText", "ItemNameText", "50 Golden Pieces", new Vector2(0.1f,  0.65f), new Vector2(0.9f,  0.88f)));
        WireField(view, "_priceText",       AddTextPrefab(panel, "TitleText", "PriceText",    "$0.99",            new Vector2(0.2f,  0.45f), new Vector2(0.8f,  0.63f)));
        WireField(view, "_statusText",      AddTextPrefab(panel, "BodyText",  "StatusText",   "",                 new Vector2(0.1f,  0.30f), new Vector2(0.9f,  0.44f)));
        WireField(view, "_purchaseButton",  AddButtonPrefab(panel, "PositiveButton",   "PurchaseButton", "Buy",    new Vector2(0.08f, 0.05f), new Vector2(0.45f, 0.28f)));
        WireField(view, "_cancelButton",    AddButtonPrefab(panel, "DestructiveButton","CancelButton",   "Cancel", new Vector2(0.55f, 0.05f), new Vector2(0.92f, 0.28f)));

        SavePopupPrefab(root, "IAPPurchasePopup");
    }

    private static void CreateObjectRestoredPrefab(PopupAnimationConfig animConfig)
    {
        var smallWindow = LoadPrefab(UIPrefabDir, "SmallPopupWindow");
        var root        = InstantiateNested(smallWindow, "ObjectRestoredPopup");
        var panel       = root.transform.Find("Panel");

        var view = root.AddComponent<ObjectRestoredView>();
        WireField(view, "_animConfig",       animConfig);
        WireField(view, "_canvasGroup",      root.GetComponent<CanvasGroup>());
        WireField(view, "_panel",            panel.GetComponent<RectTransform>());
        WireField(view, "_objectNameText",   AddTextPrefab(panel, "TitleText",     "ObjectNameText",  "Object Restored!", new Vector2(0.1f,  0.55f), new Vector2(0.9f,  0.88f)));
        WireField(view, "_continueButton",   AddButtonPrefab(panel, "PositiveButton", "ContinueButton", "Continue",       new Vector2(0.25f, 0.10f), new Vector2(0.75f, 0.38f)));

        SavePopupPrefab(root, "ObjectRestoredPopup");
    }

    // ── Nested prefab helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Instantiates a prefab as the root of a new popup — no parent, no scene.
    /// The connection to the source prefab is preserved until SaveAsPrefabAsset.
    /// </summary>
    private static GameObject InstantiateNested(GameObject sourcePrefab, string name)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
        go.name = name;
        return go;
    }

    /// <summary>
    /// Instantiates a text prefab into <paramref name="parent"/>, renames it, sets
    /// the default text and anchor rect, and returns the TMP_Text component.
    /// The prefab connection is preserved in the saved asset.
    /// </summary>
    private static TMP_Text AddTextPrefab(Transform parent, string prefabName, string childName,
        string defaultText, Vector2 anchorMin, Vector2 anchorMax)
    {
        var prefab = LoadPrefab(TextPrefabDir, prefabName);
        var go     = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name    = childName;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin        = anchorMin;
        rect.anchorMax        = anchorMax;
        rect.sizeDelta        = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = defaultText;

        return tmp;
    }

    /// <summary>
    /// Instantiates a button prefab into <paramref name="parent"/>, renames it, sets
    /// the label text and anchor rect, and returns the Button component.
    /// The prefab connection is preserved in the saved asset.
    /// </summary>
    private static Button AddButtonPrefab(Transform parent, string prefabName, string childName,
        string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var prefab = LoadPrefab(ButtonPrefabDir, prefabName);
        var go     = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name    = childName;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin        = anchorMin;
        rect.anchorMax        = anchorMax;
        rect.sizeDelta        = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;

        return go.GetComponent<Button>();
    }

    private static void SavePopupPrefab(GameObject root, string name)
    {
        var path   = $"{PopupPrefabDir}/{name}.prefab";
        var saved  = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        if (saved != null)
            Debug.Log($"[PrefabKitSetup] Saved {path}");
        else
            Debug.LogError($"[PrefabKitSetup] Failed to save {path}");
    }

    // ── Primitive prefab builders ─────────────────────────────────────────────

    private static void CreateWindowPrefab(string name, Vector4 panelAnchors)
    {
        // Root — full screen, CanvasGroup only (no dim Image — InputBlocker owns the overlay)
        var root     = new GameObject(name);
        var rootRect = root.AddComponent<RectTransform>();
        SetStretchRect(rootRect);

        var canvasGroup = root.AddComponent<CanvasGroup>();
        canvasGroup.alpha           = 1f;
        canvasGroup.interactable    = true;
        canvasGroup.blocksRaycasts  = true;

        // Panel child — animated region
        var panel     = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(panelAnchors.x, panelAnchors.y);
        panelRect.anchorMax        = new Vector2(panelAnchors.z, panelAnchors.w);
        panelRect.sizeDelta        = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage           = panel.AddComponent<Image>();
        panelImage.color         = PanelColor;
        panelImage.raycastTarget = true;

        SaveComponentPrefab(root, UIPrefabDir, name);
        Object.DestroyImmediate(root);
    }

    private static void CreateButtonPrefab(string name, Color color)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();

        var image   = go.AddComponent<Image>();
        image.color = color;
        go.AddComponent<Button>();

        var labelGO  = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text      = name;
        labelTmp.fontSize  = 20f;
        labelTmp.color     = Color.white;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.fontStyle = FontStyles.Normal;
        SetStretchRect(labelGO.GetComponent<RectTransform>());

        SaveComponentPrefab(go, ButtonPrefabDir, name);
        Object.DestroyImmediate(go);
    }

    private static void CreateTextPrefab(string name, float fontSize, FontStyles style)
    {
        var go  = new GameObject(name);
        go.AddComponent<RectTransform>();

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text            = name;
        tmp.fontSize        = fontSize;
        tmp.color           = Color.white;
        tmp.alignment       = TextAlignmentOptions.Center;
        tmp.fontStyle       = style;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        SaveComponentPrefab(go, TextPrefabDir, name);
        Object.DestroyImmediate(go);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder(UIPrefabDir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        if (!AssetDatabase.IsValidFolder(ButtonPrefabDir))
            AssetDatabase.CreateFolder(UIPrefabDir, "Buttons");
        if (!AssetDatabase.IsValidFolder(TextPrefabDir))
            AssetDatabase.CreateFolder(UIPrefabDir, "Text");
        if (!AssetDatabase.IsValidFolder(PopupPrefabDir))
            AssetDatabase.CreateFolder(UIPrefabDir, "Popups");
    }

    private static void SaveComponentPrefab(GameObject go, string dir, string name)
    {
        var path = $"{dir}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Debug.Log($"[PrefabKitSetup] Saved {path}");
    }

    private static GameObject LoadPrefab(string dir, string name)
    {
        var path   = $"{dir}/{name}.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            Debug.LogError($"[PrefabKitSetup] Prefab not found: {path} — run 'Create UI Prefab Kit' first.");
        return prefab;
    }

    private static PopupAnimationConfig LoadOrCreateAnimConfig()
    {
        var cfg = AssetDatabase.LoadAssetAtPath<PopupAnimationConfig>(AnimConfigPath);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<PopupAnimationConfig>();
            System.IO.Directory.CreateDirectory("Assets/Data");
            AssetDatabase.CreateAsset(cfg, AnimConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[PrefabKitSetup] Created PopupAnimationConfig.asset");
        }
        return cfg;
    }

    private static void SetStretchRect(RectTransform rect)
    {
        rect.anchorMin        = Vector2.zero;
        rect.anchorMax        = Vector2.one;
        rect.sizeDelta        = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void WireField(Object component, string fieldName, Object value)
    {
        var so   = new SerializedObject(component);
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
