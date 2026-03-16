---
estimated_steps: 7
estimated_files: 4
---

# T01: Rewrite UnityTransitionPlayer with LitMotion and extract prefab

**Slice:** S01 — Prefab transition player with LitMotion
**Milestone:** M005

## Description

Replace the manual while-loop alpha interpolation in `UnityTransitionPlayer` with LitMotion's `LMotion.Create().BindToAlpha().ToUniTask()`. Add LitMotion assembly references to `SimpleGame.Core.asmdef`. Create a self-contained transition prefab asset. Update the `SceneSetup` editor script to instantiate the prefab instead of building the transition overlay inline.

## Steps

1. Add `"LitMotion"` and `"LitMotion.Extensions"` to `SimpleGame.Core.asmdef` references array
2. Rewrite `UnityTransitionPlayer.FadeOutAsync` to use `LMotion.Create(0f, 1f, _fadeDuration).BindToAlpha(_canvasGroup).ToUniTask(ct)` — set initial state (alpha=0, active, blocksRaycasts=false), run the tween, set final state (alpha=1)
3. Rewrite `UnityTransitionPlayer.FadeInAsync` to use `LMotion.Create(1f, 0f, _fadeDuration).BindToAlpha(_canvasGroup).ToUniTask(ct)` — set initial state (alpha=1, blocksRaycasts=false), run the tween, set final state (alpha=0, inactive)
4. Create an editor script (`CreateTransitionPrefab.cs`) that builds the prefab programmatically: Canvas (sort order 200) + CanvasGroup (blocksRaycasts=false, alpha=0) + black Image + UnityTransitionPlayer (wired to CanvasGroup) → save as `Assets/Prefabs/TransitionOverlay.prefab`, starts inactive
5. Run the prefab creation script via batchmode `-executeMethod`
6. Update `SceneSetup.cs` to instantiate the transition prefab instead of building inline objects — load from `AssetDatabase.LoadAssetAtPath`, instantiate, wire into Boot scene
7. Run all 98+ edit-mode tests to verify nothing broke

## Must-Haves

- [ ] `UnityTransitionPlayer.cs` contains no `while` loops — uses `LMotion.Create().BindToAlpha().ToUniTask()` instead
- [ ] `UnityTransitionPlayer.cs` imports `LitMotion` and `LitMotion.Extensions`
- [ ] `SimpleGame.Core.asmdef` references `LitMotion` and `LitMotion.Extensions`
- [ ] `Assets/Prefabs/TransitionOverlay.prefab` exists with Canvas + CanvasGroup + Image + UnityTransitionPlayer
- [ ] `ITransitionPlayer.cs` is unchanged (no diff from HEAD)
- [ ] `SceneSetup.cs` instantiates the prefab instead of building transition objects inline
- [ ] 98+ edit-mode tests pass

## Verification

- `rg "while" Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` → empty
- `rg "LMotion" Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` → matches found
- `rg "LitMotion" Assets/Scripts/Core/SimpleGame.Core.asmdef` → matches found
- `test -f Assets/Prefabs/TransitionOverlay.prefab` → success
- Unity batchmode test run → 98+ passed, 0 failed

## Inputs

- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — current while-loop implementation to rewrite
- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — needs LitMotion references
- `Assets/Editor/SceneSetup.cs` — lines 92-106 build transition overlay inline; needs to instantiate prefab instead
- `Library/PackageCache/com.annulusgames.lit-motion@*/Runtime/Extensions/uGUI/LitMotionUGUIExtensions.cs` — `BindToAlpha` API reference
- `Library/PackageCache/com.annulusgames.lit-motion@*/Runtime/External/UniTask/LitMotionUniTaskExtensions.cs` — `ToUniTask()` API reference
- D033: Transition visuals owned by prefab
- D034: LitMotion for transition tweening
- D002: Assembly references use string names, not GUIDs

## Expected Output

- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — rewritten with LitMotion, no while loops
- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — updated with LitMotion references
- `Assets/Prefabs/TransitionOverlay.prefab` — new self-contained transition prefab
- `Assets/Editor/SceneSetup.cs` — updated to instantiate prefab
- `Assets/Editor/CreateTransitionPrefab.cs` — new editor utility to create the prefab asset
