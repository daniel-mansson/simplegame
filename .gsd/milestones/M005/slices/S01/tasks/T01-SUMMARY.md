---
id: T01
parent: S01
milestone: M005
provides:
  - UnityTransitionPlayer rewritten with LitMotion BindToAlpha/ToUniTask
  - TransitionOverlay.prefab — self-contained transition prefab asset
  - CreateTransitionPrefab.cs editor utility for prefab creation
  - SimpleGame.Core.asmdef updated with LitMotion references
  - SceneSetup.cs instantiates prefab instead of building inline
requires: []
affects: []
key_files:
  - Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs
  - Assets/Prefabs/TransitionOverlay.prefab
  - Assets/Scripts/Core/SimpleGame.Core.asmdef
  - Assets/Editor/CreateTransitionPrefab.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - "LitMotion BindToAlpha + ToUniTask replaces manual while loop (D034)"
  - "Prefab self-contains Canvas + CanvasGroup + Image + MonoBehaviour (D033)"
patterns_established:
  - "Prefab-based UI overlay pattern: editor script creates prefab, SceneSetup instantiates it"
drill_down_paths:
  - .gsd/milestones/M005/slices/S01/tasks/T01-PLAN.md
duration: 30min
verification_result: pass
completed_at: 2026-03-16T19:55:00Z
---

# T01: LitMotion transition player with prefab extraction

**Rewrote UnityTransitionPlayer to use LMotion.Create().BindToAlpha().ToUniTask() and extracted transition overlay into a self-contained prefab**

## What Happened

Replaced the manual while-loop alpha interpolation in `UnityTransitionPlayer` with LitMotion's builder API: `LMotion.Create(0f, 1f, _fadeDuration).BindToAlpha(_canvasGroup).ToUniTask(ct)`. This required adding `LitMotion` and `LitMotion.Extensions` assembly references to `SimpleGame.Core.asmdef`.

Created `CreateTransitionPrefab.cs` editor utility that programmatically builds the prefab: Canvas (sort order 200, overlay mode) + CanvasScaler + GraphicRaycaster + CanvasGroup (alpha=0, no raycasts) + black Image + UnityTransitionPlayer (wired to CanvasGroup). Saved as `Assets/Prefabs/TransitionOverlay.prefab`.

Updated `SceneSetup.cs` to instantiate the prefab via `PrefabUtility.InstantiatePrefab` instead of building the transition overlay inline from individual components.

`ITransitionPlayer` interface is completely unchanged. All 98/98 edit-mode tests pass.

## Deviations

None.

## Files Created/Modified
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — rewritten with LitMotion, no while loops
- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — added LitMotion and LitMotion.Extensions references
- `Assets/Prefabs/TransitionOverlay.prefab` — new self-contained transition prefab
- `Assets/Editor/CreateTransitionPrefab.cs` — new editor utility for prefab creation
- `Assets/Editor/SceneSetup.cs` — instantiates prefab instead of inline objects
