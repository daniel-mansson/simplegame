using SimpleGame.Core.MVP;
using SimpleGame.Core.Unity.PopupManagement;
using SimpleGame.Core.Unity.TransitionManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Settings;
using SimpleJigsaw;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Editor-only utility that creates placeholder scenes and registers them in EditorBuildSettings.
/// Callable via batchmode: -executeMethod SceneSetup.CreateAndRegisterScenes
/// or via the Unity menu: Tools/Setup/Create And Register Scenes
/// </summary>
public static class SceneSetup
{
    private const string ScenesDir = "Assets/Scenes";
    private const string BootPath = "Assets/Scenes/Boot.unity";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    private const string SettingsPath = "Assets/Scenes/Settings.unity";
    private const string InGamePath = "Assets/Scenes/InGame.unity";

    [MenuItem("Tools/Setup/Create And Register Scenes")]
    public static void CreateAndRegisterScenes()
    {
        if (!System.IO.Directory.Exists(ScenesDir))
        {
            System.IO.Directory.CreateDirectory(ScenesDir);
            Debug.Log("[SceneSetup] Created directory: " + ScenesDir);
        }

        CreateBootScene();
        CreateMainMenuScene();
        CreateSettingsScene();
        CreateInGameScene();

        var buildScenes = new[]
        {
            new EditorBuildSettingsScene(BootPath, true),
            new EditorBuildSettingsScene(MainMenuPath, true),
            new EditorBuildSettingsScene(SettingsPath, true),
            new EditorBuildSettingsScene(InGamePath, true)
        };

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("[SceneSetup] Registered scenes in EditorBuildSettings: Boot(0), MainMenu(1), Settings(2), InGame(3)");
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetup] Scene setup complete.");
    }

    // ── Boot Scene ───────────────────────────────────────────────────────────

    private static void CreateBootScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // GameBootstrapper
        var bootstrapperGO = new GameObject("GameBootstrapper");
        var bootstrapper = bootstrapperGO.AddComponent<GameBootstrapper>();

        // Wire WorldData if available
        var worldData = AssetDatabase.LoadAssetAtPath<SimpleGame.Game.Meta.WorldData>("Assets/Data/WorldData.asset");
        if (worldData != null)
            WireSerializedField(bootstrapper, "_worldData", worldData);
        else
            Debug.LogWarning("[SceneSetup] WorldData.asset not found — run Tools/Setup/Create Test World Data first.");

        // EventSystem
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // Popup animation config — create if missing, then load
        const string animConfigPath = "Assets/Data/PopupAnimationConfig.asset";
        var animConfig = AssetDatabase.LoadAssetAtPath<PopupAnimationConfig>(animConfigPath);
        if (animConfig == null)
        {
            animConfig = ScriptableObject.CreateInstance<PopupAnimationConfig>();
            System.IO.Directory.CreateDirectory("Assets/Data");
            AssetDatabase.CreateAsset(animConfig, animConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetup] Created PopupAnimationConfig.asset at Assets/Data/");
        }

        // InputBlocker Canvas (sort order 100)
        CreateFullScreenCanvas("InputBlocker", 100, out var inputBlockerCanvas);
        var inputBlockerCanvasGroup = inputBlockerCanvas.gameObject.AddComponent<CanvasGroup>();
        inputBlockerCanvasGroup.blocksRaycasts = false;
        inputBlockerCanvasGroup.interactable = true;
        inputBlockerCanvasGroup.alpha = 0f;

        // Full-screen dark Image child — the CanvasGroup alpha animation fades this in/out.
        // Without a visual element the blocker canvas renders nothing regardless of alpha.
        var blockerImageGO = new GameObject("BlockerImage");
        blockerImageGO.transform.SetParent(inputBlockerCanvas.transform, false);
        var blockerRect = blockerImageGO.AddComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;
        blockerImageGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        var inputBlocker = inputBlockerCanvas.gameObject.AddComponent<UnityInputBlocker>();
        WireSerializedField(inputBlocker, "_canvasGroup", inputBlockerCanvasGroup);
        WireSerializedField(inputBlocker, "_animConfig",  animConfig);
        WireSerializedField(bootstrapper, "_inputBlocker", inputBlocker);

        // Transition overlay from prefab
        var transitionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TransitionOverlay.prefab");
        if (transitionPrefab != null)
        {
            var transitionInstance = (GameObject)PrefabUtility.InstantiatePrefab(transitionPrefab);
            transitionInstance.SetActive(false);
            WireSerializedField(bootstrapper, "_transitionPlayer", transitionInstance.GetComponent<UnityTransitionPlayer>());
        }
        else
        {
            Debug.LogWarning("[SceneSetup] TransitionOverlay.prefab not found.");
        }

        // Currency Overlay Canvas (sort order 120 — above blocker 100, below stacked popups 150+)
        CreateFullScreenCanvas("CurrencyOverlay", 120, out var overlayCanvas);
        var overlayCanvasGroup = overlayCanvas.gameObject.AddComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.blocksRaycasts = false;

        // Coin balance text
        var balanceLabelGO = new GameObject("CoinsBalanceText");
        balanceLabelGO.transform.SetParent(overlayCanvas.transform, false);
        var balanceRect = balanceLabelGO.AddComponent<RectTransform>();
        balanceRect.anchorMin = new Vector2(0.6f, 0.88f);
        balanceRect.anchorMax = new Vector2(0.98f, 0.99f);
        balanceRect.sizeDelta = Vector2.zero;
        balanceRect.anchoredPosition = Vector2.zero;
        var balanceTmp = balanceLabelGO.AddComponent<TMPro.TextMeshProUGUI>();
        balanceTmp.text = "Coins: 0";
        balanceTmp.alignment = TMPro.TextAlignmentOptions.Right;
        balanceTmp.fontSize = 22;
        balanceTmp.color = Color.white;

        var overlayComponent = overlayCanvas.gameObject.AddComponent<SimpleGame.Core.Unity.UnityCurrencyOverlay>();
        WireSerializedField(overlayComponent, "_canvasGroup", overlayCanvasGroup);
        WireSerializedField(overlayComponent, "_balanceText", balanceTmp);
        WireSerializedField(bootstrapper, "_currencyOverlay", overlayComponent);

        // Popup Canvas (sort order 300)
        CreateFullScreenCanvas("PopupCanvas", 300, out var popupCanvas);
        var popupContainer = popupCanvas.gameObject.AddComponent<UnityViewContainer>();
        WireSerializedField(bootstrapper, "_viewContainer", popupContainer);
        WireSerializedField(popupContainer, "_inputBlocker", inputBlocker);  // for dynamic sort order on stacking

        // Instantiate popup prefabs — each prefab already has view, animConfig, and all
        // child refs wired inside the asset. Run "Create Popup Prefabs" if any are missing.
        WireSerializedField(popupContainer, "_confirmDialogPopup",  InstantiatePopupPrefab("ConfirmDialogPopup",  popupCanvas.transform));
        WireSerializedField(popupContainer, "_levelCompletePopup",  InstantiatePopupPrefab("LevelCompletePopup",  popupCanvas.transform));
        WireSerializedField(popupContainer, "_levelFailedPopup",    InstantiatePopupPrefab("LevelFailedPopup",    popupCanvas.transform));
        WireSerializedField(popupContainer, "_rewardedAdPopup",     InstantiatePopupPrefab("RewardedAdPopup",     popupCanvas.transform));
        WireSerializedField(popupContainer, "_iapPurchasePopup",    InstantiatePopupPrefab("IAPPurchasePopup",    popupCanvas.transform));
        WireSerializedField(popupContainer, "_objectRestoredPopup", InstantiatePopupPrefab("ObjectRestoredPopup", popupCanvas.transform));
        WireSerializedField(popupContainer, "_shopPopup",           InstantiatePopupPrefab("ShopPopup",           popupCanvas.transform));
        WireSerializedField(popupContainer, "_consentGatePopup",    InstantiatePopupPrefab("ConsentGatePopup",    popupCanvas.transform));

        bool saved = EditorSceneManager.SaveScene(scene, BootPath);
        Debug.Log(saved ? "[SceneSetup] Boot scene saved." : "[SceneSetup] ERROR saving Boot scene.");
    }

    // ── MainMenu Scene ───────────────────────────────────────────────────────

    private static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateSceneCamera("MainMenuCamera");
        CreateFullScreenCanvas("Canvas", 0, out var canvas);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;

        // ── HomePanel — all home content lives here; toggled by InSceneScreenManager ──
        var homePanelGO = new GameObject("HomePanel", typeof(RectTransform));
        homePanelGO.transform.SetParent(canvas.transform, false);
        var homePanelRect = homePanelGO.GetComponent<RectTransform>();
        homePanelRect.anchorMin = Vector2.zero;
        homePanelRect.anchorMax = Vector2.one;
        homePanelRect.sizeDelta = Vector2.zero;
        homePanelGO.SetActive(true);

        // All home-screen UI is parented to HomePanel so it hides when switching to Shop
        Transform homeParent = homePanelGO.transform;

        // Environment name
        var envNameGO = CreateText("EnvironmentNameText", "Garden", homeParent,
            new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), 32);

        // Balance
        var balanceGO = CreateText("BalanceText", "0 Golden Pieces", homeParent,
            new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.85f), 24);

        // Level display
        var levelGO = CreateText("LevelText", "Level 1", homeParent,
            new Vector2(0.3f, 0.7f), new Vector2(0.7f, 0.78f), 28);

        // Objects container — VerticalLayoutGroup for dynamically created buttons
        var objectsContainerGO = new GameObject("ObjectsContainer", typeof(RectTransform));
        objectsContainerGO.transform.SetParent(homeParent, false);
        var objectsRect = objectsContainerGO.GetComponent<RectTransform>();
        objectsRect.anchorMin = new Vector2(0.05f, 0.25f);
        objectsRect.anchorMax = new Vector2(0.95f, 0.68f);
        objectsRect.sizeDelta = Vector2.zero;
        objectsRect.anchoredPosition = Vector2.zero;

        var vlg = objectsContainerGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        var fitter = objectsContainerGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Play button
        CreateButton("PlayButton", "Play", homeParent, out var playButtonGO);
        SetRect(playButtonGO, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.22f));

        // Next Environment button (hidden by default)
        CreateButton("NextEnvironmentButton", "Next Environment →", homeParent, out var nextEnvButtonGO);
        SetRect(nextEnvButtonGO, new Vector2(0.3f, 0.01f), new Vector2(0.7f, 0.09f));
        nextEnvButtonGO.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        nextEnvButtonGO.SetActive(false);

        // Settings button
        CreateButton("SettingsButton", "Settings", homeParent, out var settingsButtonGO);
        SetRect(settingsButtonGO, new Vector2(0.7f, 0.88f), new Vector2(0.95f, 0.98f));

        // Shop button
        CreateButton("ShopButton", "Shop", homeParent, out var shopButtonGO);
        SetRect(shopButtonGO, new Vector2(0.36f, 0.88f), new Vector2(0.68f, 0.98f));
        shopButtonGO.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 1f);

        // Reset Progress button
        CreateButton("ResetProgressButton", "Reset Progress", homeParent, out var resetButtonGO);
        SetRect(resetButtonGO, new Vector2(0.05f, 0.88f), new Vector2(0.33f, 0.98f));
        resetButtonGO.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f, 1f);

        // ── ShopPanel — all shop content lives here; starts inactive ──────────
        var shopPanelGO = new GameObject("ShopPanel", typeof(RectTransform));
        shopPanelGO.transform.SetParent(canvas.transform, false);
        var shopPanelRect = shopPanelGO.GetComponent<RectTransform>();
        shopPanelRect.anchorMin = Vector2.zero;
        shopPanelRect.anchorMax = Vector2.one;
        shopPanelRect.sizeDelta = Vector2.zero;
        shopPanelGO.SetActive(false);

        Transform shopParent = shopPanelGO.transform;

        // Shop title
        CreateText("ShopTitleText", "Shop", shopParent,
            new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.98f), 36);

        // Balance / status text (TMP — ShopView._statusText is TMP_Text)
        var shopStatusGO = new GameObject("ShopStatusText");
        shopStatusGO.transform.SetParent(shopParent, false);
        shopStatusGO.AddComponent<RectTransform>();
        SetRect(shopStatusGO, new Vector2(0.1f, 0.79f), new Vector2(0.9f, 0.89f));
        var shopStatusTmp = shopStatusGO.AddComponent<TMPro.TextMeshProUGUI>();
        shopStatusTmp.text = "Your balance: 0 coins";
        shopStatusTmp.fontSize = 22;
        shopStatusTmp.alignment = TMPro.TextAlignmentOptions.Center;
        shopStatusTmp.color = Color.white;

        // ShopBack button
        CreateButton("ShopBackButton", "← Back", shopParent, out var shopBackButtonGO);
        SetRect(shopBackButtonGO, new Vector2(0.05f, 0.88f), new Vector2(0.35f, 0.98f));
        shopBackButtonGO.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

        // Coin pack buttons
        CreateButton("Pack1Button", "500 Coins — €1.99", shopParent, out var pack1GO);
        SetRect(pack1GO, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.78f));
        pack1GO.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

        CreateButton("Pack2Button", "1200 Coins — €3.99", shopParent, out var pack2GO);
        SetRect(pack2GO, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.61f));
        pack2GO.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

        CreateButton("Pack3Button", "2500 Coins — €7.99", shopParent, out var pack3GO);
        SetRect(pack3GO, new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.44f));
        pack3GO.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

        var shopView = shopPanelGO.AddComponent<ShopView>();
        WireSerializedField(shopView, "_cancelButton", shopBackButtonGO.GetComponent<Button>());
        // Wire status text for balance display
        {
            var so = new UnityEditor.SerializedObject(shopView);
            var statusProp = so.FindProperty("_statusText");
            if (statusProp != null)
                statusProp.objectReferenceValue = shopStatusTmp;
            else
                Debug.LogWarning("[SceneSetup] _statusText property not found on ShopView");
            var arr = so.FindProperty("_packButtons");
            if (arr != null)
            {
                arr.arraySize = 3;
                arr.GetArrayElementAtIndex(0).objectReferenceValue = pack1GO.GetComponent<Button>();
                arr.GetArrayElementAtIndex(1).objectReferenceValue = pack2GO.GetComponent<Button>();
                arr.GetArrayElementAtIndex(2).objectReferenceValue = pack3GO.GetComponent<Button>();
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── MainMenuView on Canvas root (wires home-screen buttons only) ──────
        var mainMenuView = canvas.gameObject.AddComponent<MainMenuView>();
        WireSerializedField(mainMenuView, "_settingsButton", settingsButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_playButton", playButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_resetProgressButton", resetButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_nextEnvironmentButton", nextEnvButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_shopButton", shopButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_shopBackButton", shopBackButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_environmentNameText", envNameGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_balanceText", balanceGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_levelDisplayText", levelGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_objectsContainer", objectsContainerGO.GetComponent<RectTransform>());

        // MainMenuSceneController
        var sceneControllerGO = new GameObject("MainMenuSceneController");
        var mainMenuController = sceneControllerGO.AddComponent<MainMenuSceneController>();
        WireSerializedField(mainMenuController, "_mainMenuView", mainMenuView);
        WireSerializedField(mainMenuController, "_homePanel", homePanelGO);
        WireSerializedField(mainMenuController, "_shopPanel", shopPanelGO);
        WireSerializedField(mainMenuController, "_shopView", shopView);

        bool saved = EditorSceneManager.SaveScene(scene, MainMenuPath);
        Debug.Log(saved ? "[SceneSetup] MainMenu scene saved." : "[SceneSetup] ERROR saving MainMenu scene.");
    }

    // ── Settings Scene ───────────────────────────────────────────────────────

    private static void CreateSettingsScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateSceneCamera("SettingsCamera");
        CreateFullScreenCanvas("Canvas", 0, out var canvas);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;

        var titleGO = CreateText("TitleText", "Settings", canvas.transform,
            new Vector2(0.2f, 0.7f), new Vector2(0.8f, 0.9f), 40);

        CreateButton("BackButton", "Back", canvas.transform, out var backButtonGO);
        SetRect(backButtonGO, new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.55f));

        var settingsView = canvas.gameObject.AddComponent<SettingsView>();
        WireSerializedField(settingsView, "_backButton", backButtonGO.GetComponent<Button>());
        WireSerializedField(settingsView, "_titleText", titleGO.GetComponent<Text>());

        var sceneControllerGO = new GameObject("SettingsSceneController");
        var settingsController = sceneControllerGO.AddComponent<SettingsSceneController>();
        WireSerializedField(settingsController, "_settingsView", settingsView);

        bool saved = EditorSceneManager.SaveScene(scene, SettingsPath);
        Debug.Log(saved ? "[SceneSetup] Settings scene saved." : "[SceneSetup] ERROR saving Settings scene.");
    }

    // ── InGame Scene ─────────────────────────────────────────────────────────

    private static void CreateInGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateSceneCamera("InGameCamera");

        // PhysicsRaycaster on the camera lets OnMouseDown fire on 3D piece colliders
        cam.gameObject.AddComponent<PhysicsRaycaster>();

        // CameraController enables orthographic pan by dragging on the board.
        // UGUI Screen Space Overlay elements block pointer-down before it reaches this,
        // so slot buttons and board drag do not conflict.
        cam.gameObject.AddComponent<CameraController>();

        // Build InGame canvas -- Screen Space Overlay so UGUI raycasts are processed
        // before physics, ensuring slot buttons (added in S03) block board pan drag.
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder  = 0;
        // Add GraphicRaycaster so UGUI buttons receive pointer events
        canvasGO.AddComponent<GraphicRaycaster>();
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // ── HUD: top strip (y 88–97%) ─────────────────────────────────────
        var levelGO   = CreateText("LevelText",       "Level 1", canvas.transform,
            new Vector2(0.25f, 0.90f), new Vector2(0.75f, 0.99f), 32);
        var heartsGO  = CreateText("HeartsText",      "❤ ❤ ❤",  canvas.transform,
            new Vector2(0.02f, 0.90f), new Vector2(0.30f, 0.99f), 26);
        var counterGO = CreateText("PieceCounterText","0/0",      canvas.transform,
            new Vector2(0.70f, 0.90f), new Vector2(0.98f, 0.99f), 26);

        // ── DeckView — world-space canvas for the deck tray ──────────────
        // Parented under the camera so it follows camera pan/zoom automatically.
        var deckViewGO = new GameObject("DeckView");
        var deckViewCanvas = deckViewGO.AddComponent<Canvas>();
        deckViewCanvas.renderMode   = RenderMode.WorldSpace;
        deckViewCanvas.sortingOrder = 5;
        deckViewCanvas.worldCamera  = cam;
        deckViewGO.AddComponent<GraphicRaycaster>();
        var deckViewRT = deckViewGO.GetComponent<RectTransform>();
        if (deckViewRT == null) deckViewRT = deckViewGO.AddComponent<RectTransform>();

        // Parent under camera — DeckView must remain a child of the camera so it
        // follows pan/zoom (see M023 context: "must remain attached").
        deckViewGO.transform.SetParent(cam.transform, false);
        deckViewRT.localRotation     = Quaternion.Euler(35.972f, 0f, 0f);
        deckViewRT.localScale        = new Vector3(0.01f, 0.01f, 0.01f);
        deckViewRT.localPosition     = new Vector3(0f, 0f, 11.17f);
        deckViewRT.anchoredPosition  = new Vector2(0f, -5.15f);
        deckViewRT.sizeDelta         = new Vector2(628.27f, 200f);

        // HorizontalLayoutGroup container
        var slotContainerGO = new GameObject("SlotContainer");
        slotContainerGO.transform.SetParent(deckViewGO.transform, false);
        var slotContainerRect = slotContainerGO.AddComponent<RectTransform>();
        slotContainerRect.anchorMin        = Vector2.zero;
        slotContainerRect.anchorMax        = Vector2.one;
        slotContainerRect.offsetMin        = Vector2.zero;
        slotContainerRect.offsetMax        = Vector2.zero;
        var hlg = slotContainerGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childAlignment         = TextAnchor.MiddleCenter;
        hlg.spacing                = 0f;
        hlg.padding                = new RectOffset(4, 4, 4, 4);

        // Pre-placed slot buttons — one per max supported slot count.
        // DeckView.Init(n) activates the first n and deactivates the rest.
        const int maxSlots = 5;
        var slotButtons = new Button[maxSlots];
        for (int i = 0; i < maxSlots; i++)
        {
            var slotGO = new GameObject($"Slot_{i}");
            slotGO.transform.SetParent(slotContainerGO.transform, false);
            slotGO.AddComponent<RectTransform>();

            var img = slotGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 1.0f, 0.45f);

            var btn = slotGO.AddComponent<Button>();
            btn.targetGraphic = img;

            // Tint states — keep semi-transparent for diagnostics
            var cols = btn.colors;
            cols.normalColor      = new Color(1f, 1f, 1f, 0.45f);
            cols.highlightedColor = new Color(1f, 1f, 1f, 0.65f);
            cols.pressedColor     = new Color(0.7f, 0.7f, 0.7f, 0.65f);
            btn.colors            = cols;

            slotGO.SetActive(false); // DeckView.Init enables the active ones
            slotButtons[i] = btn;
        }

        var deckView = deckViewGO.AddComponent<DeckView>();
        WireSerializedField(deckView, "_slots", slotButtons);

        // ── Wire InGameView ────────────────────────────────────────────────
        var inGameView = canvas.gameObject.AddComponent<InGameView>();
        WireSerializedField(inGameView, "_heartsText",       heartsGO.GetComponent<Text>());
        WireSerializedField(inGameView, "_pieceCounterText", counterGO.GetComponent<Text>());
        WireSerializedField(inGameView, "_levelText",        levelGO.GetComponent<Text>());
        WireSerializedField(inGameView, "_deckView",         deckView);

        // ── PuzzleParent — world-space root for 3-D piece meshes ──────────
        var puzzleParentGO = new GameObject("PuzzleParent");
        puzzleParentGO.transform.SetParent(null);

        // ── PuzzleStageController — owns all 3D piece/tray logic ─────────
        var stageGO = new GameObject("PuzzleStageController");
        var stage = stageGO.AddComponent<PuzzleStageController>();
        WireSerializedField(stage, "_inGameView",   inGameView);
        WireSerializedField(stage, "_deckView",     deckView);
        WireSerializedField(stage, "_puzzleParent", puzzleParentGO.transform);

        var gridConfig   = AssetDatabase.LoadAssetAtPath<GridLayoutConfig>("Assets/Data/DefaultGridConfig.asset");
        var renderConfig = AssetDatabase.LoadAssetAtPath<PieceRenderConfig>("Assets/Data/DefaultPieceRenderConfig.asset");
        if (gridConfig   != null) WireSerializedField(stage, "_gridLayoutConfig", gridConfig);
        else Debug.LogWarning("[SceneSetup] DefaultGridConfig.asset not found.");
        if (renderConfig != null) WireSerializedField(stage, "_pieceRenderConfig", renderConfig);

        // Transition overlay — same prefab used by Boot scene
        var transitionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TransitionOverlay.prefab");
        if (transitionPrefab != null)
        {
            var transitionInstance = (GameObject)PrefabUtility.InstantiatePrefab(transitionPrefab);
            transitionInstance.name = "RetryTransitionOverlay";
            transitionInstance.SetActive(false);
            WireSerializedField(stage, "_transitionPlayer", transitionInstance.GetComponent<UnityTransitionPlayer>());
        }
        else
        {
            Debug.LogWarning("[SceneSetup] TransitionOverlay.prefab not found — retry will use runtime fallback.");
        }

        // ── InGameSceneController ─────────────────────────────────────────
        var sceneControllerGO = new GameObject("InGameSceneController");
        var inGameController = sceneControllerGO.AddComponent<InGameSceneController>();
        sceneControllerGO.AddComponent<InGameDebugMenu>();
        WireSerializedField(inGameController, "_inGameView", inGameView);
        WireSerializedField(inGameController, "_stage",      stage);

        // ── CameraConfig ──────────────────────────────────────────────────
        // CameraController._config must be wired so the level-start sequence
        // (overview snap → hold → zoom to first placement area) runs correctly.
        var cameraConfigPath = "Assets/Data/CameraConfig.asset";
        var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(cameraConfigPath);
        if (cameraConfig == null)
        {
            cameraConfig = ScriptableObject.CreateInstance<CameraConfig>();
            if (!System.IO.Directory.Exists("Assets/Data"))
                System.IO.Directory.CreateDirectory("Assets/Data");
            AssetDatabase.CreateAsset(cameraConfig, cameraConfigPath);
        }
        var camController = cam.gameObject.GetComponent<CameraController>();
        WireSerializedField(camController, "_config", cameraConfig);

        bool saved = EditorSceneManager.SaveScene(scene, InGamePath);
        Debug.Log(saved ? "[SceneSetup] InGame scene saved." : "[SceneSetup] ERROR saving InGame scene.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void SetStretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static Camera CreateSceneCamera(string name)
        => SceneSetupHelpers.CreateSceneCamera(name);

    private static void WireSerializedField(Component component, string fieldName, Object value)
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
            Debug.LogWarning($"[SceneSetup] Field '{fieldName}' not found on {component.GetType().Name}");
        }
    }

    private static void WireSerializedField(Component component, string fieldName, Object[] values)
    {
        var so   = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop == null || !prop.isArray)
        {
            Debug.LogWarning($"[SceneSetup] Array field '{fieldName}' not found on {component.GetType().Name}");
            return;
        }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateFullScreenCanvas(string name, int sortOrder, out Canvas result)
        => SceneSetupHelpers.CreateFullScreenCanvas(name, sortOrder, out result);

    private static void CreateButton(string name, string label, Transform parent, out GameObject result)
        => SceneSetupHelpers.CreateButton(name, label, parent, out result);

    /// <summary>
    /// Instantiates a popup prefab from Assets/Prefabs/Game/Popups/ as a connected prefab
    /// instance inside <paramref name="parent"/>. The prefab already has all view fields
    /// wired. Starts inactive (hidden until PopupManager shows it).
    /// </summary>
    private static GameObject InstantiatePopupPrefab(string name, Transform parent)
    {
        var path   = $"Assets/Prefabs/Game/Popups/{name}.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[SceneSetup] Popup prefab not found: {path} — run Tools/Setup/Create Popup Prefabs first.");
            return null;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.SetActive(false);
        return go;
    }

    private static GameObject CreateText(string name, string defaultText, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.text = defaultText;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false; // display-only; no pointer events needed
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return go;
    }
}

internal class SceneSetupHelpers
{
    internal static Camera CreateSceneCamera(string name)
    {
        var go = new GameObject(name);
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.depth = 0;
        go.transform.position = new Vector3(0f, 0f, -10f);
        return cam;
    }

    internal static void CreateFullScreenCanvas(string name, int sortOrder, out Canvas result)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();
        result = canvas;
    }

    internal static void CreateButton(string name, string label, Transform parent, out GameObject result)
    {
        var buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        buttonGO.AddComponent<RectTransform>();
        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        buttonGO.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        var text = textGO.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 20;
        text.color = Color.white;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        result = buttonGO;
    }
}
