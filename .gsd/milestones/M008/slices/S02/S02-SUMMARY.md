---
id: S02
milestone: M008
provides:
  - Unity.TextMeshPro in Game and Editor asmdefs
  - PopupWindowShell — concrete MonoBehaviour subclass of PopupViewBase for use on prefabs
  - BigPopupWindow.prefab — full-screen dim + centered dark panel, PopupWindowShell wired
  - SmallPopupWindow.prefab — smaller panel variant
  - PositiveButton.prefab, DestructiveButton.prefab, NeutralButton.prefab — Button + TMP Label
  - TitleText.prefab (36pt bold), BodyText.prefab (24pt), ButtonLabel.prefab (20pt) — all TMP
  - PrefabKitSetup.cs — reproducible menu item to regenerate prefabs
  - PopupManager.DismissAllAsync bug fix (Unblock per popup, not per dismiss-all)
requires:
  - slice: S01
    provides: PopupViewBase, animation contracts
affects: [S03, S04]
key_files:
  - Assets/Editor/PrefabKitSetup.cs
  - Assets/Scripts/Core/MVP/PopupWindowShell.cs
  - Assets/Scripts/Game/SimpleGame.Game.asmdef
  - Assets/Editor/SimpleGame.Editor.asmdef
  - Assets/Prefabs/UI/BigPopupWindow.prefab
  - Assets/Prefabs/UI/SmallPopupWindow.prefab
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
key_decisions:
  - "PopupWindowShell needed as concrete PopupViewBase — Unity editor can't place abstract components"
  - "TMP bundled in com.unity.ugui 2.0.0 — no manifest change needed"
  - "DismissAllAsync must Unblock() per popup to balance Block() calls from ShowPopupAsync"
patterns_established:
  - "PrefabKitSetup.cs: create in memory → SaveAsPrefabAsset → DestroyImmediate"
  - "Window shell prefabs carry PopupWindowShell for animation; view MonoBehaviour is added by SceneSetup on top"
drill_down_paths:
  - .gsd/milestones/M008/slices/S02/tasks/T01-SUMMARY.md
  - .gsd/milestones/M008/slices/S02/tasks/T02-SUMMARY.md
duration: 25min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S02: TMP Prefab Kit

**TMP wired into asmdefs, 8 prefab assets created, DismissAllAsync bug fixed; 169 tests green.**

## What Happened

TMP was already bundled in `com.unity.ugui` — just needed asmdef references. Created `PrefabKitSetup.cs` to generate all 8 prefab assets programmatically. Window prefabs required `PopupWindowShell` (a concrete `PopupViewBase`) since abstract MonoBehaviours can't be placed in the editor directly. Prefabs carry their `_canvasGroup` and `_panel` refs wired at creation time, ready for SceneSetup to instantiate and build popup GOs on top of in S03.

Caught a regression in `DismissAllAsync` from S01: the unblock only fired once (last popup) but `Block()` fires per show, so block count never reached zero. Fixed to unblock per popup.

## Files Created/Modified
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — TMP reference added
- `Assets/Editor/SimpleGame.Editor.asmdef` — TMP + TMP.Editor references added
- `Assets/Editor/PrefabKitSetup.cs` — new
- `Assets/Scripts/Core/MVP/PopupWindowShell.cs` — new
- All 8 prefab assets under `Assets/Prefabs/UI/`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — DismissAllAsync fix
