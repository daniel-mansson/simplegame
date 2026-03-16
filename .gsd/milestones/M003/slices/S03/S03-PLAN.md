# S03: Boot-from-Any-Scene + Editor Tooling

**Goal:** Entering play mode from `MainMenu.unity` or `Settings.unity` directly works correctly: a `[RuntimeInitializeOnLoadMethod]` detects missing boot infrastructure, additively loads `Boot.unity`, and the correct SceneController runs normally. SceneSetup is updated so newly created scenes include SceneController MonoBehaviours wired to their views.

**Demo:** Open `MainMenu.unity`, press Play. Boot.unity loads additively. MainMenuSceneController.RunAsync() starts. Settings and popup work. UAT: verified by human in S03.

## Must-Haves

- `BootInjector` class with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` that detects whether boot infrastructure (GameBootstrapper) is already loaded; if not, additively loads `Boot.unity` before proceeding
- `SceneSetup.CreateMainMenuScene()` adds `MainMenuSceneController` component with SerializeField view refs wired
- `SceneSetup.CreateSettingsScene()` adds `SettingsSceneController` component with SerializeField view ref wired
- All 58 edit-mode tests still pass
- No `.Forget()` in production paths

## Proof Level

- This slice proves: operational + UAT
- Real runtime required: yes (play-mode boot-from-any-scene)
- Human/UAT required: yes — enter play mode from `MainMenu.unity`, confirm boot loads, navigation works

## Verification

- `mcporter call unityMCP.run_tests mode=EditMode` → 58+ pass
- `grep -rn ".Forget()" Assets/Scripts/` returns empty
- UAT: user enters play from MainMenu.unity; no null refs; menu appears; Settings and popup work

## Integration Closure

- Upstream: `GameBootstrapper`, `MainMenuSceneController`, `SettingsSceneController`, `SceneSetup.cs`
- New wiring: `BootInjector` hooks `[RuntimeInitializeOnLoadMethod]`; SceneSetup creates scenes with controller refs wired
- What remains: nothing — M003 milestone complete after this slice

## Tasks

- [ ] **T01: Create BootInjector with RuntimeInitializeOnLoadMethod** `est:30m`
  - Why: Enables play-from-any-scene — if Boot isn't loaded, load it before scene controllers start
  - Files: `Assets/Scripts/Game/Boot/BootInjector.cs` (new)
  - Do: Class in `SimpleGame.Game.Boot`. Static method marked `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`. Check if `GameBootstrapper` component exists in any loaded scene via `FindFirstObjectByType<GameBootstrapper>()`; if null, load Boot additively via `SceneManager.LoadScene("Boot", LoadSceneMode.Additive)`. Use `#if UNITY_EDITOR` guard if this should only run in editor (for MVP: always run it, it's harmless in real build if Boot is already first in build settings). Handle edge case: if Boot scene isn't in build settings, log error and skip.
  - Verify: Compile clean; play-mode UAT confirms it works
  - Done when: file exists, compiles, detects missing boot and loads it

- [ ] **T02: Update SceneSetup to wire SceneControllers** `est:30m`
  - Why: New scene creation must include SceneController MonoBehaviours so scenes work out of the box
  - Files: `Assets/Editor/SceneSetup.cs`
  - Do: In `CreateMainMenuScene()`: add `MainMenuSceneController` to the canvas root (or a dedicated GO); wire `_mainMenuView` and `_confirmDialogView` serialized fields via `WireSerializedField`. In `CreateSettingsScene()`: add `SettingsSceneController`; wire `_settingsView`. Note: ConfirmDialogView lives in Boot.unity, not MainMenu.unity — so `_confirmDialogView` SerializeField in MainMenuSceneController cannot be wired at scene creation time (cross-scene reference). Revised approach: `_confirmDialogView` SerializeField in MainMenuSceneController should be nullable; the controller finds it at runtime via `FindFirstObjectByType<ConfirmDialogView>()` when handling the popup. Update MainMenuSceneController accordingly.
  - Verify: Run `Tools/Setup/Create And Register Scenes`; open resulting MainMenu.unity; verify MainMenuSceneController is present
  - Done when: SceneSetup wires controllers; scenes contain SceneControllers after regeneration

- [ ] **T03: Update MainMenuSceneController for runtime view discovery** `est:20m`
  - Why: ConfirmDialogView is in Boot scene, not MainMenu — cross-scene SerializeField refs don't persist; must find at runtime
  - Files: `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
  - Do: Replace `[SerializeField] ConfirmDialogView _confirmDialogView` with runtime lookup: in `HandleConfirmPopupAsync`, use `FindFirstObjectByType<ConfirmDialogView>()` if `_confirmDialogView` is null. Keep `_confirmDialogViewOverride` for tests (unchanged). This makes the component robust whether or not the SerializeField is wired in the editor.
  - Verify: Existing SceneController tests still pass; new scenes wire correctly
  - Done when: field is optional; runtime fallback works; tests pass

- [ ] **T04: Regenerate scenes + run final verification** `est:20m`
  - Why: Apply SceneSetup changes to actual scene files so they reflect the new SceneController wiring
  - Do: Call `mcporter call unityMCP.execute_menu_item menu_path="Tools/Setup/Create And Register Scenes"`. Then `run_tests mode=EditMode` — expect 58+ pass. Check `grep -rn ".Forget()" Assets/Scripts/` returns empty.
  - Verify: All tests pass; scenes contain SceneControllers
  - Done when: `mcporter call unityMCP.run_tests mode=EditMode` → all pass; scenes regenerated

## Files Likely Touched

- `Assets/Scripts/Game/Boot/BootInjector.cs` (new)
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Editor/SceneSetup.cs`
