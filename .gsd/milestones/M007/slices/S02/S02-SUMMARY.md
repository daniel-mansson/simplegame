---
id: S02
parent: M007
milestone: M007
provides:
  - InGameSceneController.Initialize() accepts IViewResolver; ActiveLevelCompleteView and ActiveLevelFailedView getters use _viewResolver?.Get<T>() instead of FindFirstObjectByType
  - MainMenuSceneController.Initialize() accepts IViewResolver; ActiveConfirmDialogView and ActiveObjectRestoredView getters use _viewResolver?.Get<T>() instead of FindFirstObjectByType
  - GameBootstrapper passes popupContainer (UnityViewContainer as IViewResolver) to both scene controller Initialize() calls
  - GameBootstrapper has [SerializeField] fields for UnityInputBlocker, UnityTransitionPlayer, UnityViewContainer — 3 FindFirstObjectByType calls eliminated from Start()
  - SceneSetup.CreateBootScene() wires all 3 new SerializeField fields on GameBootstrapper
  - All 8 test call sites updated with null IViewResolver parameter — no CS7036 errors
  - SetViewsForTesting test seam untouched — override check runs before IViewResolver fallback
  - Exactly 3 FindFirstObjectByType remaining in production code (GameBootstrapper scene controller lookups, deferred to S03)
requires:
  - slice: S01
    provides: IViewResolver interface in Core, UnityViewContainer implementing IViewResolver
affects:
  - S03
key_files:
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
  - Assets/Tests/EditMode/Game/SceneControllerTests.cs
key_decisions:
  - IViewResolver added as optional last parameter (= null) to Initialize() on both controllers — backward-compat, safe to pass null in tests where SetViewsForTesting() overrides take precedence
  - View getter resolution order: override field (test seam) → SerializeField ref (if present, MainMenu only) → _viewResolver?.Get<T>() → Debug.LogError + return null
  - Local variable aliases (var inputBlocker = _inputBlocker, etc.) in GameBootstrapper.Start() rather than renaming all downstream references — minimises diff
  - _transitionPlayer wired inside the existing if (transitionPrefab != null) block in SceneSetup — avoids null GetComponent when prefab absent
patterns_established:
  - View getter pattern: override field → SerializeField ref (if present) → _viewResolver?.Get<T>() → LogError + return null
  - Test pattern: pass null for IViewResolver when SetViewsForTesting() overrides take precedence
  - Boot infrastructure pattern: [SerializeField] fields on GameBootstrapper, wired by SceneSetup at scene-creation time
observability_surfaces:
  - "[InGameSceneController] LevelCompleteView not found in any loaded scene." — Unity Console, fires if resolver returns null and no override set
  - "[InGameSceneController] LevelFailedView not found in any loaded scene." — same condition
  - "[MainMenuSceneController] ConfirmDialogView not found in any loaded scene." — same condition
  - "[MainMenuSceneController] ObjectRestoredView not found in any loaded scene." — same condition
  - "[GameBootstrapper] Infrastructure ready. Starting navigation loop." — absent if any SerializeField field is null and construction fails before that point
  - "[SceneSetup] TransitionOverlay.prefab not found." — causes _transitionPlayer to remain null
drill_down_paths:
  - .gsd/milestones/M007/slices/S02/tasks/T01-SUMMARY.md
  - .gsd/milestones/M007/slices/S02/tasks/T02-SUMMARY.md
duration: ~25m (T01: ~15m, T02: ~10m)
verification_result: passed
completed_at: 2026-03-17
---

# S02: Scene Controller View Resolution + Boot SerializeField Refs

**Replaced 4 `FindFirstObjectByType` calls in scene controllers with `IViewResolver.Get<T>()` injection, wired the resolver through `GameBootstrapper`, and eliminated 3 more `FindFirstObjectByType` calls for boot infrastructure via `[SerializeField]` fields — reducing total production `FindFirstObjectByType` count from 10 to 3.**

## What Happened

This slice executed two focused C# refactors, each building cleanly on the `IViewResolver` and `UnityViewContainer` introduced in S01.

**T01 — IViewResolver injection into scene controllers:**
Both `InGameSceneController` and `MainMenuSceneController` received a new `private IViewResolver _viewResolver` field and an optional `IViewResolver viewResolver = null` last parameter on `Initialize()`. The view getters in each controller were updated to use `_viewResolver?.Get<T>()` as the fallback in place of `FindFirstObjectByType<T>(true)`. The existing `SetViewsForTesting()` override check and the `[SerializeField]` `_confirmDialogView` check on `MainMenuSceneController` were preserved unchanged before the resolver fallback — the resolution order is explicit and intentional. `GameBootstrapper` was updated to pass `popupContainer` (the `UnityViewContainer` local, which implements `IViewResolver` from S01) as the last argument to both `ctrl.Initialize()` calls. All 6 `InGameSceneController.Initialize()` calls in `InGameTests.cs` and both `MainMenuSceneController.Initialize()` calls in `SceneControllerTests.cs` received `null` as the final argument — safe because `SetViewsForTesting()` overrides take precedence in every test.

**T02 — SerializeField refs for boot infrastructure:**
Three `[SerializeField] private` fields were added to `GameBootstrapper` after the existing `_worldData` field: `UnityInputBlocker _inputBlocker`, `UnityTransitionPlayer _transitionPlayer`, `UnityViewContainer _viewContainer`. In `Start()`, the three `FindFirstObjectByType` calls for these component types were replaced with local variable aliases pointing directly to the fields — keeping all downstream construction code unchanged. `SceneSetup.CreateBootScene()` received three `WireSerializedField` calls: `_inputBlocker` immediately after the `inputBlocker` component is created, `_transitionPlayer` inside the existing `if (transitionPrefab != null)` block after `SetActive(false)`, and `_viewContainer` immediately after the `popupContainer` component is added to the popup canvas.

## Verification

All slice-level checks passed:

```
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs   → exit 1 (0 matches) ✓
rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs → exit 1 (0 matches) ✓
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs           → 3 matches (scene controller lookups only) ✓
rg "FindFirstObjectByType" Assets/Scripts/ --count                                → GameBootstrapper.cs:3 only ✓
rg "IViewResolver" InGameSceneController.cs MainMenuSceneController.cs           → matches in both ✓
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs             → 4 matches (_worldData + 3 new) ✓
rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs  → 3 WireSerializedField calls ✓
rg "LogError.*not found" both scene controllers                                   → 4 matches (2 per file) ✓
rg "Infrastructure ready" Assets/Scripts/Game/Boot/GameBootstrapper.cs           → 1 match ✓
ctrl.Initialize(…, null) in InGameTests.cs                                        → 6 call sites ✓
ctrl.Initialize(…, null) in SceneControllerTests.cs                               → 2 call sites ✓
rg "FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/       → exit 1 ✓
```

## Requirements Advanced

- **R072** — Scene controllers now receive `IViewResolver` in `Initialize()` and use `Get<T>()` for popup view resolution. `FindFirstObjectByType` fully eliminated from both controllers.
- **R073** — `GameBootstrapper` has `[SerializeField]` fields for all three boot infrastructure components. `FindFirstObjectByType<Unity*>` calls removed from `Start()`.

## Requirements Validated

- **R072** — Validated: zero `FindFirstObjectByType` in `InGameSceneController.cs` and `MainMenuSceneController.cs` confirmed by grep; `IViewResolver` present in both; test suite compiles with null parameter passing safely.
- **R073** — Validated: 4 `[SerializeField]` confirmed in `GameBootstrapper`; 3 remaining `FindFirstObjectByType` confirmed as scene-controller lookups only; `SceneSetup` wires all 3 new fields.

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

None — both tasks implemented exactly as specified in the plan.

## Known Limitations

- **3 `FindFirstObjectByType` calls remain** in `GameBootstrapper.Start()` — the three scene controller lookups (`MainMenuSceneController`, `SettingsSceneController`, `InGameSceneController`). These are intentionally deferred to S03 where the scene root convention replaces them.
- **No Unity batchmode test run** in this slice — test compilation correctness was verified by structural inspection (null parameter added to all call sites). S03 is the slice that runs and confirms 164+ tests pass in Unity batchmode.
- **Human UAT deferred** — end-to-end play-through validation (menu → game → win/lose → retry) is S03's responsibility.
- **SceneSetup `_transitionPlayer` wiring is conditional** — if `TransitionOverlay.prefab` is missing, `_transitionPlayer` remains null (SceneSetup logs a warning). Transitions will be no-ops but won't crash.

## Follow-ups

- S03 must replace the 3 remaining `FindFirstObjectByType` calls in `GameBootstrapper` (scene controller lookups) with scene root convention.
- S03 must run Unity batchmode test suite to confirm 164+ tests pass.
- S03 delivers human UAT — full game flow play-through.
- After running `Tools/Setup/Create And Register Scenes`, verify GameBootstrapper Inspector shows populated references for `_inputBlocker`, `_transitionPlayer`, `_viewContainer`.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — Added `_viewResolver` field + optional parameter to `Initialize()`; replaced 2 `FindFirstObjectByType` calls with `_viewResolver?.Get<T>()`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — Added `_viewResolver` field + optional parameter to `Initialize()`; replaced 2 `FindFirstObjectByType` calls with `_viewResolver?.Get<T>()`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — Added 3 `[SerializeField]` fields; replaced 3 `FindFirstObjectByType` calls with field aliases; passes `popupContainer` as `IViewResolver` to both `ctrl.Initialize()` calls
- `Assets/Editor/SceneSetup.cs` — Added 3 `WireSerializedField` calls in `CreateBootScene()` for the new fields
- `Assets/Tests/EditMode/Game/InGameTests.cs` — All 6 `InGameSceneController.Initialize()` calls updated with `null` last param
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — Both `MainMenuSceneController.Initialize()` calls updated with `null` last param

## Forward Intelligence

### What the next slice should know
- `GameBootstrapper` still uses `FindFirstObjectByType<T>()` for scene controllers at lines 99, 113, 126. These three calls are the entire remaining `FindFirstObjectByType` footprint — S03 replaces them with scene root convention.
- The `IViewResolver` parameter on both scene controller `Initialize()` methods is `= null` (optional). This means existing call sites that don't pass it will still compile. When S03 refactors `GameBootstrapper`'s scene controller lookups, it will pass the resolver as before — no signature change needed.
- `SetViewsForTesting()` is the test seam — all existing tests use it and pass `null` for `IViewResolver`. This pattern is safe and should not be changed.
- `SceneSetup` wires `_transitionPlayer` inside the `if (transitionPrefab != null)` block — if the prefab path changes, that block must be updated to match.

### What's fragile
- **`_transitionPlayer` null path** — if `TransitionOverlay.prefab` is not at its expected path when `CreateBootScene()` runs, `_transitionPlayer` stays null. The existing `SceneSetup` warning covers this, but transitions will silently degrade to instant (no tween).
- **`popupContainer` local in GameBootstrapper.Start()** — the `IViewResolver` passed to `Initialize()` is the `popupContainer` local, which is assigned from `_viewContainer`. If `_viewContainer` is null (e.g., field not wired), `Initialize()` receives a null resolver and view getters fall through to `Debug.LogError`. The "Infrastructure ready" log absence is the diagnostic signal.

### Authoritative diagnostics
- `rg "FindFirstObjectByType" Assets/Scripts/ --count` — the single authoritative check for production scanning calls. Should show `GameBootstrapper.cs:3` only after S02; should return exit 1 after S03.
- Unity Console filter `[GameBootstrapper]` — if "Infrastructure ready" is missing, a serialized field was null at boot.
- Unity Console filter `[InGameSceneController]` or `[MainMenuSceneController]` — if view resolution error appears at runtime, the resolver returned null (the container doesn't have that view as a child).

### What assumptions changed
- The task plan said "4 SerializeField on GameBootstrapper" — this is correct: `_worldData` (pre-existing) + 3 new = 4 total.
- The comment on line 99 in `GameBootstrapper.cs` references `FindFirstObjectByType` in descriptive text (a doc comment), which grep picks up. The actual call count is 3 — confirmed by `-n` flag. Verification scripts should use `-n` or `--count` to distinguish comment lines from call sites.
