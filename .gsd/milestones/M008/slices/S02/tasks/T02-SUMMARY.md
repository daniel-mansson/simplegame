---
id: T02
parent: S02
milestone: M008
provides:
  - Assets/Prefabs/UI/BigPopupWindow.prefab — root CanvasGroup + dim Image + Panel child with PopupWindowShell
  - Assets/Prefabs/UI/SmallPopupWindow.prefab — smaller panel variant
  - Assets/Prefabs/UI/Buttons/PositiveButton.prefab — green Button + TMP Label child
  - Assets/Prefabs/UI/Buttons/DestructiveButton.prefab — red Button + TMP Label child
  - Assets/Prefabs/UI/Buttons/NeutralButton.prefab — grey Button + TMP Label child
  - Assets/Prefabs/UI/Text/TitleText.prefab — TMP_Text fontSize 36 bold
  - Assets/Prefabs/UI/Text/BodyText.prefab — TMP_Text fontSize 24
  - Assets/Prefabs/UI/Text/ButtonLabel.prefab — TMP_Text fontSize 20
  - Assets/Editor/PrefabKitSetup.cs — menu item Tools/Setup/Create UI Prefab Kit
  - Assets/Scripts/Core/MVP/PopupWindowShell.cs — concrete PopupViewBase for window prefabs
requires:
  - slice: S02/T01
    provides: TMP asmdef references
affects: [S03]
key_files:
  - Assets/Editor/PrefabKitSetup.cs
  - Assets/Scripts/Core/MVP/PopupWindowShell.cs
  - Assets/Prefabs/UI/BigPopupWindow.prefab
  - Assets/Prefabs/UI/SmallPopupWindow.prefab
  - Assets/Prefabs/UI/Buttons/PositiveButton.prefab
  - Assets/Prefabs/UI/Buttons/DestructiveButton.prefab
  - Assets/Prefabs/UI/Buttons/NeutralButton.prefab
  - Assets/Prefabs/UI/Text/TitleText.prefab
  - Assets/Prefabs/UI/Text/BodyText.prefab
  - Assets/Prefabs/UI/Text/ButtonLabel.prefab
key_decisions:
  - "PopupWindowShell concrete subclass of PopupViewBase needed — abstract class can't be added as component directly"
  - "Window prefabs wire _canvasGroup and _panel on PopupWindowShell at prefab creation time"
  - "DismissAllAsync bug fixed: must Unblock() per popup (not just last one) — blockCount must reach 0"
patterns_established:
  - "PrefabKitSetup.cs pattern: create GO in memory → SaveAsPrefabAsset → DestroyImmediate"
drill_down_paths:
  - .gsd/milestones/M008/slices/S02/tasks/T02-PLAN.md
duration: 20min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T02: Create Prefab Assets via Editor Script

**All 8 UI prefab assets created — BigPopup/SmallPopup window shells with PopupWindowShell, 3 button variants, 3 TMP text styles; DismissAllAsync unblock bug fixed; 169 tests green.**

## What Happened

Created `PrefabKitSetup.cs` with `[MenuItem("Tools/Setup/Create UI Prefab Kit")]`. Window prefabs needed a concrete `PopupWindowShell : PopupViewBase` since abstract classes can't be added as components in the editor. Each window prefab has: a root with CanvasGroup (dim Image, maps to `_canvasGroup`) and a Panel child RectTransform (dark background, maps to `_panel`) — both wired via `SerializedObject` at creation time.

Button prefabs use `TextMeshProUGUI` for labels. Text prefabs are standalone `TextMeshProUGUI` GameObjects with appropriate font sizes.

During test verification, found the `DismissAllAsync` rewrite from S01/T02 had a bug: calling `Unblock()` only on the last popup while `ShowPopupAsync` calls `Block()` per show means the block count never reaches zero. Fixed to call `Unblock()` per popup in the loop (matching original behavior), with `FadeOutAsync.Forget()` only on the final iteration.

## Deviations

PopupWindowShell not in original plan — required addition to make PopupViewBase usable on prefabs.
DismissAllAsync bug fix — regression from S01/T02 orchestration rewrite.

## Files Created/Modified
- `Assets/Editor/PrefabKitSetup.cs` — new, prefab creation menu item
- `Assets/Scripts/Core/MVP/PopupWindowShell.cs` — new, concrete PopupViewBase
- `Assets/Prefabs/UI/` — all 8 prefabs created
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — DismissAllAsync unblock fix
