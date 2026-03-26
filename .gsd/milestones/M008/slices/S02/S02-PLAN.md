# S02: TMP Prefab Kit

**Goal:** Add TextMeshPro to Game and Editor asmdefs, then create 8 prefab assets for popup window shells, buttons, and text styles.

**Demo:** All 8 prefab assets exist in `Assets/Prefabs/UI/`. The Game and Editor assemblies compile with TMP. ConfirmDialog rebuilt from BigPopup + prefab components as proof that the wiring works.

## Must-Haves

- `SimpleGame.Game.asmdef` references `Unity.TextMeshPro`
- `SimpleGame.Editor.asmdef` references `Unity.TextMeshPro` and `Unity.TextMeshPro.Editor`
- `Assets/Prefabs/UI/BigPopupWindow.prefab` — full-screen background canvas + centered panel, has TitleText and BodyText TMP children, PopupViewBase _canvasGroup and _panel exposed
- `Assets/Prefabs/UI/SmallPopupWindow.prefab` — smaller panel variant
- `Assets/Prefabs/UI/Buttons/PositiveButton.prefab` — green Button + TMP label child
- `Assets/Prefabs/UI/Buttons/DestructiveButton.prefab` — red Button + TMP label child
- `Assets/Prefabs/UI/Buttons/NeutralButton.prefab` — grey Button + TMP label child
- `Assets/Prefabs/UI/Text/TitleText.prefab` — TMP_Text, large, bold, center
- `Assets/Prefabs/UI/Text/BodyText.prefab` — TMP_Text, medium, center
- `Assets/Prefabs/UI/Text/ButtonLabel.prefab` — TMP_Text, small, for use inside button prefabs
- Project compiles with no errors after asmdef changes
- EditMode tests still green (169)

## Tasks

- [x] **T01: Add TMP to asmdefs**
  Update SimpleGame.Game.asmdef and SimpleGame.Editor.asmdef to reference Unity.TextMeshPro / Unity.TextMeshPro.Editor. Verify compilation clean.

- [x] **T02: Create prefab assets via editor script**
  Write a one-shot editor utility (or extend SceneSetup) to create and save the 8 prefab assets programmatically. Prefabs created via `PrefabUtility.SaveAsPrefabAsset`. Directory `Assets/Prefabs/UI/` created if needed.

## Files Likely Touched

- `Assets/Scripts/Game/SimpleGame.Game.asmdef`
- `Assets/Editor/SimpleGame.Editor.asmdef`
- New: `Assets/Editor/PrefabKitSetup.cs` — editor utility to create prefab assets
- New: `Assets/Prefabs/UI/BigPopupWindow.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/SmallPopupWindow.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Buttons/PositiveButton.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Buttons/DestructiveButton.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Buttons/NeutralButton.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Text/TitleText.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Text/BodyText.prefab` (+ .meta)
- New: `Assets/Prefabs/UI/Text/ButtonLabel.prefab` (+ .meta)
