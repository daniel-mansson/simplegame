---
id: T02
parent: S02
milestone: M007
provides:
  - GameBootstrapper has [SerializeField] fields for UnityInputBlocker, UnityTransitionPlayer, UnityViewContainer
  - Start() uses field aliases instead of FindFirstObjectByType for boot infrastructure (3 calls eliminated)
  - SceneSetup.CreateBootScene() wires all 3 new fields on the bootstrapper GameObject
  - Exactly 3 FindFirstObjectByType remain in GameBootstrapper (scene controller lookups, deferred to S03)
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - Used local variable aliases (var inputBlocker = _inputBlocker) rather than replacing field names throughout Start() — minimises diff and keeps all downstream construction code unchanged
  - Wired _transitionPlayer inside the existing if (transitionPrefab != null) block in SceneSetup — avoids a null GetComponent call when the prefab is absent (SceneSetup already logs a warning in that branch)
patterns_established:
  - Boot infrastructure components are serialized fields on GameBootstrapper; SceneSetup wires them at scene-creation time — eliminates all FindFirstObjectByType for infrastructure types
observability_surfaces:
  - "[GameBootstrapper] Infrastructure ready. Starting navigation loop." — absent if any serialized field is null and construction fails before that point
  - GameBootstrapper Inspector in Boot scene: _inputBlocker, _transitionPlayer, _viewContainer must show populated references after running Tools/Setup/Create And Register Scenes
  - "[SceneSetup] TransitionOverlay.prefab not found." warning in Console flags missing transition prefab (causes _transitionPlayer to remain null)
duration: 10m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T02: Add SerializeField refs to GameBootstrapper for boot infrastructure and wire in SceneSetup

**Added 3 `[SerializeField]` fields to `GameBootstrapper` for boot infrastructure and wired them in `SceneSetup.CreateBootScene()`, eliminating the last 3 `FindFirstObjectByType` calls for Unity component types.**

## What Happened

Added `[SerializeField] private UnityInputBlocker _inputBlocker`, `[SerializeField] private UnityTransitionPlayer _transitionPlayer`, and `[SerializeField] private UnityViewContainer _viewContainer` to `GameBootstrapper` after the existing `_worldData` field.

In `Start()`, replaced the three `FindFirstObjectByType` calls with local variable aliases pointing to the fields:
```csharp
var inputBlocker = _inputBlocker;
var transitionPlayer = _transitionPlayer;
var popupContainer = _viewContainer;
```
All downstream code referencing these locals is unchanged.

In `SceneSetup.CreateBootScene()`, added three `WireSerializedField` calls:
- `_inputBlocker` wired immediately after `inputBlocker` component is created (after setting `_canvasGroup`)
- `_transitionPlayer` wired inside the `if (transitionPrefab != null)` block after `SetActive(false)` — gets the `UnityTransitionPlayer` component via `GetComponent`
- `_viewContainer` wired immediately after `popupContainer` component is added to the popup canvas

Also applied pre-flight fixes:
- Added `## Observability Impact` section to T02-PLAN.md
- Added a diagnostic/failure-path check to the S02-PLAN.md Verification section

## Verification

```
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs
→ 4 matches: _worldData, _inputBlocker, _transitionPlayer, _viewContainer ✓

rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs
→ 3 matches: MainMenuSceneController, SettingsSceneController, InGameSceneController ✓

rg "FindFirstObjectByType<Unity" Assets/Scripts/Game/Boot/GameBootstrapper.cs
→ exit 1 (zero matches) ✓

rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs
→ 3 WireSerializedField calls ✓

rg "FindFirstObjectByType" Assets/Scripts/ --count
→ Assets/Scripts/Game/Boot/GameBootstrapper.cs:3 only ✓
```

Slice verification checks also confirmed:
- Zero FindFirstObjectByType in InGameSceneController and MainMenuSceneController ✓
- IViewResolver present in both scene controllers ✓
- 4 LogError failure-path messages intact (2 per controller) ✓
- Test Initialize() calls all include null IViewResolver parameter ✓

## Diagnostics

- Filter Unity Console by `[GameBootstrapper]` — if "Infrastructure ready" message is missing, a serialized field was null and construction failed
- Check GameBootstrapper Inspector after running `Tools/Setup/Create And Register Scenes`: all three fields should show component references
- If `_transitionPlayer` is null: SceneSetup logs `[SceneSetup] TransitionOverlay.prefab not found.` — transitions will be no-ops but won't crash
- If `_viewContainer` is null: `PopupManager` receives null; first `ShowPopupAsync` call will throw NullReferenceException

## Deviations

None. Implemented exactly as the task plan specified.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — Added 3 SerializeField fields; replaced 3 FindFirstObjectByType calls with field aliases
- `Assets/Editor/SceneSetup.cs` — Added 3 WireSerializedField calls in CreateBootScene() for the new fields
- `.gsd/milestones/M007/slices/S02/tasks/T02-PLAN.md` — Added Observability Impact section (pre-flight fix)
- `.gsd/milestones/M007/slices/S02/S02-PLAN.md` — Added diagnostic failure-path check to Verification section (pre-flight fix)
