---
estimated_steps: 4
estimated_files: 2
---

# T02: Add SerializeField refs to GameBootstrapper for boot infrastructure and wire in SceneSetup

**Slice:** S02 — Scene Controller View Resolution + Boot SerializeField Refs
**Milestone:** M007

## Description

Replace the 3 `FindFirstObjectByType` calls for boot infrastructure (`UnityInputBlocker`, `UnityTransitionPlayer`, `UnityViewContainer`) in `GameBootstrapper.Start()` with `[SerializeField]` fields. Update `SceneSetup.CreateBootScene()` to wire these fields so the Boot scene has the correct serialized references.

This eliminates 3 more `FindFirstObjectByType` calls (R073), leaving only the 3 scene controller lookups for S03.

Relevant installed skills: none needed — straightforward C# + editor script change.

## Steps

1. **`GameBootstrapper.cs` — Add SerializeField fields.** Add three fields after the existing `[SerializeField] private WorldData _worldData;`:
   ```csharp
   [SerializeField] private UnityInputBlocker _inputBlocker;
   [SerializeField] private UnityTransitionPlayer _transitionPlayer;
   [SerializeField] private UnityViewContainer _viewContainer;
   ```
   Ensure the necessary `using` directives are present: `UnityInputBlocker` needs `using SimpleGame.Core.Unity.PopupManagement;` (already present), `UnityTransitionPlayer` needs `using SimpleGame.Core.Unity.TransitionManagement;` (already present), `UnityViewContainer` needs `using SimpleGame.Game.Popup;` (already present).

2. **`GameBootstrapper.cs` — Replace FindFirstObjectByType calls in Start().** Remove these 3 lines:
   ```csharp
   var inputBlocker = FindFirstObjectByType<UnityInputBlocker>();
   var transitionPlayer = FindFirstObjectByType<UnityTransitionPlayer>(FindObjectsInactive.Include);
   var popupContainer = FindFirstObjectByType<UnityViewContainer>();
   ```
   Replace with local aliases pointing to the fields:
   ```csharp
   var inputBlocker = _inputBlocker;
   var transitionPlayer = _transitionPlayer;
   var popupContainer = _viewContainer;
   ```
   This preserves all downstream code that references `inputBlocker`, `transitionPlayer`, and `popupContainer` as local variables — minimal diff, no risk of breaking the PopupManager/ScreenManager/UIFactory construction or the `Initialize()` calls that T01 modified. An alternative is to use the fields directly throughout `Start()`, but local aliases are safer since they keep the rest of the method unchanged.

3. **`SceneSetup.cs` — Wire the 3 new fields in `CreateBootScene()`.** After the existing `WireSerializedField(bootstrapper, "_worldData", worldData)` block (around line 52), add:
   ```csharp
   WireSerializedField(bootstrapper, "_inputBlocker", inputBlocker);
   ```
   For the transition player, the instance is `transitionInstance` which is created inside an `if (transitionPrefab != null)` block. The `UnityTransitionPlayer` component is on that instance. Add the wiring inside that block, after the `SetActive(false)` call:
   ```csharp
   WireSerializedField(bootstrapper, "_transitionPlayer", transitionInstance.GetComponent<UnityTransitionPlayer>());
   ```
   For the view container, the variable is `popupContainer` (which is the `UnityViewContainer` component added to the PopupCanvas):
   ```csharp
   WireSerializedField(bootstrapper, "_viewContainer", popupContainer);
   ```
   **Important:** The `inputBlocker` variable in SceneSetup is the `UnityInputBlocker` component (declared around line 78). The `transitionInstance` variable is inside an `if` block so the WireSerializedField call must also be inside that block to avoid a null reference. The `popupContainer` variable is declared around line 89.

4. **Verify field count and remaining FindFirstObjectByType.** Run the verification commands to confirm 4 `[SerializeField]` attributes on GameBootstrapper and exactly 3 remaining `FindFirstObjectByType` calls (the scene controller lookups).

## Must-Haves

- [ ] `GameBootstrapper` has `[SerializeField] private UnityInputBlocker _inputBlocker`
- [ ] `GameBootstrapper` has `[SerializeField] private UnityTransitionPlayer _transitionPlayer`
- [ ] `GameBootstrapper` has `[SerializeField] private UnityViewContainer _viewContainer`
- [ ] No `FindFirstObjectByType` for `UnityInputBlocker`, `UnityTransitionPlayer`, or `UnityViewContainer` in `GameBootstrapper.Start()`
- [ ] `SceneSetup.CreateBootScene()` wires `_inputBlocker` on bootstrapper
- [ ] `SceneSetup.CreateBootScene()` wires `_transitionPlayer` on bootstrapper (inside null-guard for prefab)
- [ ] `SceneSetup.CreateBootScene()` wires `_viewContainer` on bootstrapper
- [ ] Exactly 3 `FindFirstObjectByType` calls remain in `GameBootstrapper.cs` (scene controller lookups)

## Verification

```bash
# 4 SerializeField attributes on GameBootstrapper
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → 4 matches (_worldData, _inputBlocker, _transitionPlayer, _viewContainer)

# Exactly 3 FindFirstObjectByType remaining (scene controllers only)
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → 3 matches: MainMenuSceneController, SettingsSceneController, InGameSceneController

# No FindFirstObjectByType for infrastructure types
rg "FindFirstObjectByType<Unity" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → exit 1 (zero matches)

# SceneSetup wires the 3 new fields
rg "WireSerializedField.*bootstrapper.*_inputBlocker\|WireSerializedField.*bootstrapper.*_transitionPlayer\|WireSerializedField.*bootstrapper.*_viewContainer" Assets/Editor/SceneSetup.cs
# → 3 matches

# Overall production FindFirstObjectByType count
rg "FindFirstObjectByType" Assets/Scripts/ --count
# → GameBootstrapper.cs:3 only (scene controllers removed in T01)
```

## Observability Impact

**Signals introduced / changed by this task:**
- `GameBootstrapper.Start()` now reads `_inputBlocker`, `_transitionPlayer`, and `_viewContainer` from serialized fields instead of searching the scene at runtime. If any field is `null` (not wired in the Boot scene), downstream construction of `PopupManager` or `ScreenManager` will receive null and will throw a `NullReferenceException` or silently skip functionality. The Unity Console will show the exception with a `GameBootstrapper` stack frame.
- The existing `Debug.Log("[GameBootstrapper] Infrastructure ready. Starting navigation loop.")` log still fires after infrastructure is built — absence of this message means construction failed before that point.

**How to inspect this task at runtime:**
1. In Unity Console, filter by `[GameBootstrapper]` — if the "Infrastructure ready" message is absent, one of the three serialized fields was null.
2. Check the `GameBootstrapper` Inspector in the Boot scene — all three fields (`_inputBlocker`, `_transitionPlayer`, `_viewContainer`) should show populated references after running `Tools/Setup/Create And Register Scenes`.
3. If `_transitionPlayer` is null (prefab missing), `ScreenManager` will have no transition effect but won't crash (transition is nullable). Log line `[SceneSetup] TransitionOverlay.prefab not found.` in the Console flags the missing asset.

**Failure visibility:**
- `_inputBlocker` null: `PopupManager` constructor receives null; blocking behavior silently absent. No exception unless PopupManager guards against null internally.
- `_transitionPlayer` null: `ScreenManager` treats transitions as no-ops. The SceneSetup already logs a warning for the missing prefab case.
- `_viewContainer` null: `PopupManager` constructor receives null `IPopupContainer`; ShowPopupAsync will throw on first popup attempt — visible as NullReferenceException in Console.

## Inputs

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — After T01, this file already passes `popupContainer` to scene controller `Initialize()` calls. The `Start()` method still has 3 `FindFirstObjectByType` calls for infrastructure (lines 57-59).
- `Assets/Editor/SceneSetup.cs` — `CreateBootScene()` creates `inputBlocker`, `transitionInstance`, and `popupContainer` variables. These are the exact components to wire to the new fields.
- T01 output: `InGameSceneController.cs` and `MainMenuSceneController.cs` have zero `FindFirstObjectByType` calls.

## Expected Output

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — 3 new `[SerializeField]` fields; `Start()` uses fields instead of `FindFirstObjectByType` for infrastructure; 3 scene controller lookups remain unchanged
- `Assets/Editor/SceneSetup.cs` — 3 new `WireSerializedField` calls in `CreateBootScene()` for `_inputBlocker`, `_transitionPlayer`, `_viewContainer`
