# T02: Create Prefab Assets via Editor Script

**Slice:** S02
**Milestone:** M008

## Goal

Write `PrefabKitSetup.cs` — an Editor-only script with a menu item that creates and saves all 8 UI prefab assets. Run it via Unity MCP to generate the prefabs.

## Must-Haves

### Truths
- `Assets/Prefabs/UI/BigPopupWindow.prefab` exists and contains: root GameObject with CanvasGroup + RectTransform; child "Panel" with Image and RectTransform; "Panel" has PopupViewBase _canvasGroup and _panel serialized (via the PopupWindowBase component if needed, or just the structure for SceneSetup to use)
- `Assets/Prefabs/UI/SmallPopupWindow.prefab` exists — same structure, smaller panel size
- `Assets/Prefabs/UI/Buttons/PositiveButton.prefab` — GameObject with Button + Image (green) + TMP_Text child
- `Assets/Prefabs/UI/Buttons/DestructiveButton.prefab` — same but red
- `Assets/Prefabs/UI/Buttons/NeutralButton.prefab` — same but grey
- `Assets/Prefabs/UI/Text/TitleText.prefab` — TMP_Text, fontSize 36, bold, white, center
- `Assets/Prefabs/UI/Text/BodyText.prefab` — TMP_Text, fontSize 24, white, center
- `Assets/Prefabs/UI/Text/ButtonLabel.prefab` — TMP_Text, fontSize 20, white, center

### Artifacts
- `Assets/Editor/PrefabKitSetup.cs` — editor script with [MenuItem("Tools/Setup/Create UI Prefab Kit")]
- All 8 prefab files exist after menu item runs

### Key Links
- BigPopupWindow/SmallPopupWindow have a `PopupViewBase` component — but wait: PopupViewBase is abstract. Window prefabs should have a `PopupWindowShell` MonoBehaviour that holds `_canvasGroup` (on root) and `_panel` (the Panel child RectTransform) as serialized fields, for SceneSetup to wire when it builds popup GameObjects from these prefabs.
- Actually: the prefabs are structural containers. The actual PopupViewBase component lives on the popup view GameObject (e.g. ConfirmDialogView). The window prefab just provides the visual shell (CanvasGroup root + Panel image). SceneSetup instantiates the prefab and adds the view component on top.

## Steps

1. Create `Assets/Editor/PrefabKitSetup.cs` with [MenuItem("Tools/Setup/Create UI Prefab Kit")]
2. Create directories `Assets/Prefabs/UI/`, `Assets/Prefabs/UI/Buttons/`, `Assets/Prefabs/UI/Text/`
3. Create BigPopupWindow: root GO + CanvasGroup + stretch rect; Panel child + Image (dark semi-transparent) + centered rect (0.1–0.9 x, 0.2–0.8 y)
4. Create SmallPopupWindow: same but Panel rect 0.15–0.85 x, 0.3–0.7 y
5. Create PositiveButton: GO + Button + Image (green) + TMP_Text child "Label"
6. Create DestructiveButton: same with red Image
7. Create NeutralButton: same with grey Image
8. Create TitleText: GO + TMP_Text, fontSize 36, bold, white, center, auto-size min 24 max 48
9. Create BodyText: GO + TMP_Text, fontSize 24, white, center
10. Create ButtonLabel: GO + TMP_Text, fontSize 20, white, center
11. Save all as prefabs via `PrefabUtility.SaveAsPrefabAsset`
12. Run via MCP: `execute_menu_item "Tools/Setup/Create UI Prefab Kit"`

## Context
- TMP namespace: `TMPro` — `TMP_Text`, `TextMeshProUGUI`
- Use `TextMeshProUGUI` for UI canvas text (not `TextMeshPro` which is for world space)
- `PrefabUtility.SaveAsPrefabAsset(go, path)` — returns the saved prefab asset; destroy the temp GO after saving
- Window prefab root needs CanvasGroup for PopupViewBase._canvasGroup; Panel child needs to be a RectTransform for PopupViewBase._panel
- K001: use MCP via shell quoting for menu paths with spaces
