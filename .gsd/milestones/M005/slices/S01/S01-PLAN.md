# S01: Prefab transition player with LitMotion

**Goal:** Replace the hand-rolled `UnityTransitionPlayer` while-loop fade with LitMotion tweening, and extract the transition overlay from inline Boot scene objects into a self-contained prefab.
**Demo:** Screen transitions use a 0.3s fade-to-black driven by LitMotion. The transition prefab is a standalone asset — swapping it changes the transition look without code changes.

## Must-Haves

- `UnityTransitionPlayer.cs` uses `LMotion.Create().BindToAlpha().ToUniTask()` — no manual while loop
- `Assets/Prefabs/TransitionOverlay.prefab` exists as a self-contained prefab (Canvas + CanvasGroup + Image + UnityTransitionPlayer)
- `SimpleGame.Core.asmdef` references `LitMotion` and `LitMotion.Extensions`
- `ITransitionPlayer` interface is unchanged (zero signature changes)
- All 98+ edit-mode tests pass
- SceneSetup editor script instantiates the prefab instead of building inline objects
- Play-mode transitions work (0.3s fade-to-black during screen navigation)

## Verification

- `rg "while" Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` returns empty (no manual loops)
- `rg "LMotion" Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` returns matches (LitMotion in use)
- `diff <(git show HEAD:Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs) Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` returns empty (interface unchanged)
- `test -f Assets/Prefabs/TransitionOverlay.prefab` succeeds
- Unity batchmode test run: 98+ tests pass, 0 failures
- Play-mode UAT: navigate between screens, observe 0.3s fade-to-black

## Tasks

- [x] **T01: Rewrite UnityTransitionPlayer with LitMotion and extract prefab** `est:45m`
  - Why: This is the entire slice — rewrite the MonoBehaviour to use LitMotion, add asmdef references, create the prefab via editor script, update SceneSetup to instantiate from prefab
  - Files: `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs`, `Assets/Scripts/Core/SimpleGame.Core.asmdef`, `Assets/Editor/SceneSetup.cs`
  - Do: Add LitMotion/LitMotion.Extensions refs to Core asmdef → rewrite FadeOutAsync/FadeInAsync to use `LMotion.Create(0f, 1f, _fadeDuration).BindToAlpha(_canvasGroup).ToUniTask(ct)` → create editor script to produce prefab asset → update SceneSetup to instantiate prefab → run tests → verify in play mode
  - Verify: `rg "while" UnityTransitionPlayer.cs` empty; `rg "LMotion" UnityTransitionPlayer.cs` matches; 98+ tests pass; prefab exists
  - Done when: All must-haves in this slice pass, including play-mode fade working

## Files Likely Touched

- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs`
- `Assets/Scripts/Core/SimpleGame.Core.asmdef`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Prefabs/TransitionOverlay.prefab` (new)
