using SimpleGame.Core.Unity.PopupManagement;
using SimpleGame.Core.Unity.TransitionManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Settings;
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

        // InputBlocker Canvas (sort order 100)
        CreateFullScreenCanvas("InputBlocker", 100, out var inputBlockerCanvas);
        var inputBlockerCanvasGroup = inputBlockerCanvas.gameObject.AddComponent<CanvasGroup>();
        inputBlockerCanvasGroup.blocksRaycasts = false;
        inputBlockerCanvasGroup.interactable = true;
        inputBlockerCanvasGroup.alpha = 1f;
        var inputBlocker = inputBlockerCanvas.gameObject.AddComponent<UnityInputBlocker>();
        WireSerializedField(inputBlocker, "_canvasGroup", inputBlockerCanvasGroup);

        // Transition overlay from prefab
        var transitionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TransitionOverlay.prefab");
        if (transitionPrefab != null)
        {
            var transitionInstance = (GameObject)PrefabUtility.InstantiatePrefab(transitionPrefab);
            transitionInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[SceneSetup] TransitionOverlay.prefab not found.");
        }

        // Popup Canvas (sort order 300)
        CreateFullScreenCanvas("PopupCanvas", 300, out var popupCanvas);
        var popupContainer = popupCanvas.gameObject.AddComponent<UnityPopupContainer>();

        // --- ConfirmDialog popup ---
        var confirmDialogGO = CreatePopupDialog(popupCanvas.transform, "ConfirmDialogPopup",
            "Are you sure?", null, null,
            new[] { ("ConfirmButton", "OK"), ("CancelButton", "Cancel") });
        var confirmDialogView = confirmDialogGO.AddComponent<ConfirmDialogView>();
        WireSerializedField(confirmDialogView, "_confirmButton", confirmDialogGO.transform.Find("Panel/ConfirmButton").GetComponent<Button>());
        WireSerializedField(confirmDialogView, "_cancelButton", confirmDialogGO.transform.Find("Panel/CancelButton").GetComponent<Button>());
        WireSerializedField(confirmDialogView, "_messageText", confirmDialogGO.transform.Find("Panel/TitleText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_confirmDialogPopup", confirmDialogGO);
        confirmDialogGO.SetActive(false);

        // --- LevelComplete popup ---
        var levelCompleteGO = CreatePopupDialog(popupCanvas.transform, "LevelCompletePopup",
            "Level Complete!", "Score: 0", "+0 Golden Pieces",
            new[] { ("ContinueButton", "Continue") });
        var levelCompleteView = levelCompleteGO.AddComponent<LevelCompleteView>();
        WireSerializedField(levelCompleteView, "_continueButton", levelCompleteGO.transform.Find("Panel/ContinueButton").GetComponent<Button>());
        WireSerializedField(levelCompleteView, "_scoreText", levelCompleteGO.transform.Find("Panel/ScoreText").GetComponent<Text>());
        WireSerializedField(levelCompleteView, "_levelText", levelCompleteGO.transform.Find("Panel/TitleText").GetComponent<Text>());
        WireSerializedField(levelCompleteView, "_goldenPiecesText", levelCompleteGO.transform.Find("Panel/LevelText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_levelCompletePopup", levelCompleteGO);
        levelCompleteGO.SetActive(false);

        // --- LevelFailed popup ---
        var levelFailedGO = CreatePopupDialog(popupCanvas.transform, "LevelFailedPopup",
            "Level Failed!", "Score: 0", "Level 1",
            new[] { ("RetryButton", "Retry"), ("WatchAdButton", "Watch Ad"), ("QuitButton", "Quit") });
        var levelFailedView = levelFailedGO.AddComponent<LevelFailedView>();
        WireSerializedField(levelFailedView, "_retryButton", levelFailedGO.transform.Find("Panel/RetryButton").GetComponent<Button>());
        WireSerializedField(levelFailedView, "_watchAdButton", levelFailedGO.transform.Find("Panel/WatchAdButton").GetComponent<Button>());
        WireSerializedField(levelFailedView, "_quitButton", levelFailedGO.transform.Find("Panel/QuitButton").GetComponent<Button>());
        WireSerializedField(levelFailedView, "_scoreText", levelFailedGO.transform.Find("Panel/ScoreText").GetComponent<Text>());
        WireSerializedField(levelFailedView, "_levelText", levelFailedGO.transform.Find("Panel/LevelText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_levelFailedPopup", levelFailedGO);
        levelFailedGO.SetActive(false);

        // --- RewardedAd popup ---
        var rewardedAdGO = CreatePopupDialog(popupCanvas.transform, "RewardedAdPopup",
            "Watch Ad?", "Watch a short ad for a reward", null,
            new[] { ("WatchButton", "Watch"), ("SkipButton", "Skip") });
        var rewardedAdView = rewardedAdGO.AddComponent<RewardedAdView>();
        WireSerializedField(rewardedAdView, "_watchButton", rewardedAdGO.transform.Find("Panel/WatchButton").GetComponent<Button>());
        WireSerializedField(rewardedAdView, "_skipButton", rewardedAdGO.transform.Find("Panel/SkipButton").GetComponent<Button>());
        WireSerializedField(rewardedAdView, "_statusText", rewardedAdGO.transform.Find("Panel/ScoreText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_rewardedAdPopup", rewardedAdGO);
        rewardedAdGO.SetActive(false);

        // --- IAPPurchase popup ---
        var iapGO = CreatePopupDialog(popupCanvas.transform, "IAPPurchasePopup",
            "Purchase", "50 Golden Pieces", "$0.99",
            new[] { ("PurchaseButton", "Buy"), ("CancelButton", "Cancel") });
        var iapView = iapGO.AddComponent<IAPPurchaseView>();
        WireSerializedField(iapView, "_purchaseButton", iapGO.transform.Find("Panel/PurchaseButton").GetComponent<Button>());
        WireSerializedField(iapView, "_cancelButton", iapGO.transform.Find("Panel/CancelButton").GetComponent<Button>());
        WireSerializedField(iapView, "_itemNameText", iapGO.transform.Find("Panel/TitleText").GetComponent<Text>());
        WireSerializedField(iapView, "_priceText", iapGO.transform.Find("Panel/ScoreText").GetComponent<Text>());
        WireSerializedField(iapView, "_statusText", iapGO.transform.Find("Panel/LevelText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_iapPurchasePopup", iapGO);
        iapGO.SetActive(false);

        // --- ObjectRestored popup ---
        var objRestoredGO = CreatePopupDialog(popupCanvas.transform, "ObjectRestoredPopup",
            "Object Restored!", null, null,
            new[] { ("ContinueButton", "Continue") });
        var objRestoredView = objRestoredGO.AddComponent<ObjectRestoredView>();
        WireSerializedField(objRestoredView, "_continueButton", objRestoredGO.transform.Find("Panel/ContinueButton").GetComponent<Button>());
        WireSerializedField(objRestoredView, "_objectNameText", objRestoredGO.transform.Find("Panel/TitleText").GetComponent<Text>());
        WireSerializedField(popupContainer, "_objectRestoredPopup", objRestoredGO);
        objRestoredGO.SetActive(false);

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

        // Environment name
        var envNameGO = CreateText("EnvironmentNameText", "Garden", canvas.transform,
            new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), 32);

        // Balance
        var balanceGO = CreateText("BalanceText", "0 Golden Pieces", canvas.transform,
            new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.85f), 24);

        // Level display
        var levelGO = CreateText("LevelText", "Level 1", canvas.transform,
            new Vector2(0.3f, 0.7f), new Vector2(0.7f, 0.78f), 28);

        // Objects container — VerticalLayoutGroup for dynamically created buttons
        var objectsContainerGO = new GameObject("ObjectsContainer", typeof(RectTransform));
        objectsContainerGO.transform.SetParent(canvas.transform, false);
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

        // ContentSizeFitter so the container grows with its children
        var fitter = objectsContainerGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Play button
        CreateButton("PlayButton", "Play", canvas.transform, out var playButtonGO);
        SetRect(playButtonGO, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.22f));

        // Next Environment button (hidden by default — shown when env is complete)
        CreateButton("NextEnvironmentButton", "Next Environment →", canvas.transform, out var nextEnvButtonGO);
        SetRect(nextEnvButtonGO, new Vector2(0.3f, 0.01f), new Vector2(0.7f, 0.09f));
        nextEnvButtonGO.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 1f);
        nextEnvButtonGO.SetActive(false);

        // Settings button
        CreateButton("SettingsButton", "Settings", canvas.transform, out var settingsButtonGO);
        SetRect(settingsButtonGO, new Vector2(0.7f, 0.88f), new Vector2(0.95f, 0.98f));

        // Reset Progress button
        CreateButton("ResetProgressButton", "Reset Progress", canvas.transform, out var resetButtonGO);
        SetRect(resetButtonGO, new Vector2(0.05f, 0.88f), new Vector2(0.35f, 0.98f));
        resetButtonGO.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f, 1f);

        // Wire MainMenuView
        var mainMenuView = canvas.gameObject.AddComponent<MainMenuView>();
        WireSerializedField(mainMenuView, "_settingsButton", settingsButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_playButton", playButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_resetProgressButton", resetButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_nextEnvironmentButton", nextEnvButtonGO.GetComponent<Button>());
        WireSerializedField(mainMenuView, "_environmentNameText", envNameGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_balanceText", balanceGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_levelDisplayText", levelGO.GetComponent<Text>());
        WireSerializedField(mainMenuView, "_objectsContainer", objectsContainerGO.GetComponent<RectTransform>());

        // MainMenuSceneController
        var sceneControllerGO = new GameObject("MainMenuSceneController");
        var mainMenuController = sceneControllerGO.AddComponent<MainMenuSceneController>();
        WireSerializedField(mainMenuController, "_mainMenuView", mainMenuView);

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
        CreateFullScreenCanvas("Canvas", 0, out var canvas);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;

        // Level label
        var levelGO = CreateText("LevelText", "Level 1", canvas.transform,
            new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.97f), 32);

        // Hearts display
        var heartsGO = CreateText("HeartsText", "3", canvas.transform,
            new Vector2(0.05f, 0.88f), new Vector2(0.2f, 0.97f), 28);

        // Piece counter
        var pieceCounterGO = CreateText("PieceCounterText", "0/10", canvas.transform,
            new Vector2(0.3f, 0.72f), new Vector2(0.7f, 0.85f), 36);

        // Place correct button
        CreateButton("PlaceCorrectButton", "Place Correct", canvas.transform, out var placeCorrectGO);
        SetRect(placeCorrectGO, new Vector2(0.55f, 0.35f), new Vector2(0.9f, 0.55f));

        // Place incorrect button
        CreateButton("PlaceIncorrectButton", "Place Incorrect", canvas.transform, out var placeIncorrectGO);
        SetRect(placeIncorrectGO, new Vector2(0.1f, 0.35f), new Vector2(0.45f, 0.55f));

        // Wire InGameView
        var inGameView = canvas.gameObject.AddComponent<InGameView>();
        WireSerializedField(inGameView, "_placeCorrectButton", placeCorrectGO.GetComponent<Button>());
        WireSerializedField(inGameView, "_placeIncorrectButton", placeIncorrectGO.GetComponent<Button>());
        WireSerializedField(inGameView, "_heartsText", heartsGO.GetComponent<Text>());
        WireSerializedField(inGameView, "_pieceCounterText", pieceCounterGO.GetComponent<Text>());
        WireSerializedField(inGameView, "_levelText", levelGO.GetComponent<Text>());

        // InGameSceneController
        var sceneControllerGO = new GameObject("InGameSceneController");
        var inGameController = sceneControllerGO.AddComponent<InGameSceneController>();
        WireSerializedField(inGameController, "_inGameView", inGameView);

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

    private static void CreateFullScreenCanvas(string name, int sortOrder, out Canvas result)
        => SceneSetupHelpers.CreateFullScreenCanvas(name, sortOrder, out result);

    private static void CreateButton(string name, string label, Transform parent, out GameObject result)
        => SceneSetupHelpers.CreateButton(name, label, parent, out result);

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
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        return go;
    }

    private static GameObject CreatePopupDialog(Transform parent, string name,
        string titleDefault, string scoreDefault, string levelDefault,
        (string name, string label)[] buttons)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.AddComponent<Canvas>();
        root.AddComponent<CanvasGroup>();
        root.AddComponent<GraphicRaycaster>();
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        SetStretchRect(panel.GetComponent<RectTransform>());

        // Title
        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panel.transform, false);
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = titleDefault ?? "";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontSize = 30;
        titleText.color = Color.white;
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.sizeDelta = Vector2.zero;

        // Score text (may be null)
        if (scoreDefault != null)
        {
            var scoreGO = new GameObject("ScoreText");
            scoreGO.transform.SetParent(panel.transform, false);
            var scoreTextComp = scoreGO.AddComponent<Text>();
            scoreTextComp.text = scoreDefault;
            scoreTextComp.alignment = TextAnchor.MiddleCenter;
            scoreTextComp.fontSize = 24;
            scoreTextComp.color = Color.white;
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.2f, 0.52f);
            scoreRect.anchorMax = new Vector2(0.8f, 0.68f);
            scoreRect.sizeDelta = Vector2.zero;
        }

        // Level text (may be null)
        if (levelDefault != null)
        {
            var levelGO = new GameObject("LevelText");
            levelGO.transform.SetParent(panel.transform, false);
            var levelTextComp = levelGO.AddComponent<Text>();
            levelTextComp.text = levelDefault;
            levelTextComp.alignment = TextAnchor.MiddleCenter;
            levelTextComp.fontSize = 20;
            levelTextComp.color = Color.white;
            var levelRect = levelGO.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.2f, 0.38f);
            levelRect.anchorMax = new Vector2(0.8f, 0.52f);
            levelRect.sizeDelta = Vector2.zero;
        }

        // Buttons
        float buttonWidth = 0.22f;
        float gap = 0.04f;
        float totalWidth = buttons.Length * buttonWidth + (buttons.Length - 1) * gap;
        float startX = 0.5f - totalWidth / 2f;

        for (int i = 0; i < buttons.Length; i++)
        {
            float x = startX + i * (buttonWidth + gap);
            CreateButton(buttons[i].name, buttons[i].label, panel.transform, out var btnGO);
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(x, 0.1f);
            btnRect.anchorMax = new Vector2(x + buttonWidth, 0.32f);
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;
        }

        return root;
    }
}

internal class SceneSetupHelpers
{
    internal static Camera CreateSceneCamera(string name)
    {
        var go = new GameObject(name);
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
