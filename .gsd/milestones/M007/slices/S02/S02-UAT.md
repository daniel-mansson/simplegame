---
id: S02
parent: M007
milestone: M007
uat_type: artifact-driven
---

# S02: Scene Controller View Resolution + Boot SerializeField Refs — UAT

**Milestone:** M007
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S02 is a pure structural refactor with no new runtime behavior or UI changes. Correctness is provable by static analysis (grep counts) and compile-time type checking (test parameter signatures). No new runtime logic was introduced — view getters fall back to `_viewResolver?.Get<T>()` instead of `FindFirstObjectByType`, which is identical in behavior when the resolver holds the same views. Unity batchmode test run (164+ tests) is the runtime proof, deferred to S03 which also runs human UAT.

## Preconditions

- Unity project is open (or accessible via CLI)
- S01 is complete: `IViewResolver` interface exists in Core, `UnityViewContainer` implements `IViewResolver`
- `Tools/Setup/Create And Register Scenes` has been run at least once (to populate SerializeField refs in Boot scene)
- No uncommitted merge conflicts in the 6 modified files

## Smoke Test

```bash
rg "FindFirstObjectByType" Assets/Scripts/ --count
```
**Expected:** `Assets/Scripts/Game/Boot/GameBootstrapper.cs:3` — no other files listed. This confirms the slice's primary goal: scene controller scanning eliminated, only scene-controller lookups remain.

---

## Test Cases

### 1. Zero FindFirstObjectByType in InGameSceneController

```bash
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs
```

**Expected:** No output, exit code 1. Any match is a failure — it means the S02 refactor was not applied or was partially reverted.

---

### 2. Zero FindFirstObjectByType in MainMenuSceneController

```bash
rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
```

**Expected:** No output, exit code 1.

---

### 3. Exactly 3 FindFirstObjectByType remaining in GameBootstrapper (scene controller lookups only)

```bash
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs -n
```

**Expected:** Exactly 3 lines, all referencing scene controller types:
- `FindFirstObjectByType<MainMenuSceneController>()`
- `FindFirstObjectByType<SettingsSceneController>()`
- `FindFirstObjectByType<InGameSceneController>()`

Any match on `UnityInputBlocker`, `UnityTransitionPlayer`, or `UnityViewContainer` means the T02 refactor was not applied.

---

### 4. IViewResolver present in both scene controllers

```bash
rg "IViewResolver" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
```

**Expected:** At minimum 2 matches per file — one for the `private IViewResolver _viewResolver` field declaration and one for the `IViewResolver viewResolver = null` parameter in `Initialize()`. LogError strings referencing "IViewResolver" count as bonus matches but do not substitute for the field and parameter.

---

### 5. Exactly 4 SerializeField fields on GameBootstrapper

```bash
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs
```

**Expected:** Exactly 4 lines:
1. `[SerializeField] private WorldData _worldData;` (pre-existing from before M007)
2. `[SerializeField] private UnityInputBlocker _inputBlocker;`
3. `[SerializeField] private UnityTransitionPlayer _transitionPlayer;`
4. `[SerializeField] private UnityViewContainer _viewContainer;`

---

### 6. SceneSetup wires all 3 new fields

```bash
rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs
```

**Expected:** 3 lines, each being a `WireSerializedField(bootstrapper, "...", ...)` call for each field name. If any of the three is missing, running the scene setup tool will leave that field null on the `GameBootstrapper` Inspector.

---

### 7. Test call sites include null IViewResolver parameter

```bash
rg "ctrl\.Initialize" Assets/Tests/EditMode/Game/InGameTests.cs
```

**Expected:** 6 lines, all ending with `, null)`. The null is the `IViewResolver viewResolver = null` parameter — safe because `SetViewsForTesting()` overrides take precedence.

```bash
rg "ctrl\.Initialize.*goldenPieces" Assets/Tests/EditMode/Game/SceneControllerTests.cs
```

**Expected:** 2 lines, both ending with `, null)`.

---

### 8. 4 LogError observability signals intact

```bash
rg "LogError.*not found" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
```

**Expected:** 4 matches — 2 per file. These fire at runtime if `_viewResolver?.Get<T>()` also returns null and no `SetViewsForTesting()` override is set. Their presence confirms the failure-visibility path is intact.

---

### 9. "Infrastructure ready" log present in GameBootstrapper

```bash
rg "Infrastructure ready" Assets/Scripts/Game/Boot/GameBootstrapper.cs
```

**Expected:** 1 match: `Debug.Log("[GameBootstrapper] Infrastructure ready. Starting navigation loop.");`. This confirms the observable boot-success signal remains in place — its absence in the Unity Console would indicate boot infrastructure construction failed.

---

### 10. No other FindObject* variants in production scripts

```bash
rg "FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/
```

**Expected:** No output, exit code 1. These are the deprecated Unity variants that FindFirstObjectByType replaced — none should appear.

---

### 11. SceneSetup _transitionPlayer wired inside prefab guard (Unity Editor visual check)

After running `Tools/Setup/Create And Register Scenes` in the Unity Editor:

1. Open the Boot scene
2. Select the `GameBootstrapper` GameObject
3. Check the Inspector

**Expected:** `_inputBlocker`, `_transitionPlayer`, and `_viewContainer` fields all show populated component references (not "None (...)"). If `TransitionOverlay.prefab` is present in `Assets/Prefabs/`, `_transitionPlayer` should be populated; if absent, a `[SceneSetup] TransitionOverlay.prefab not found.` warning appears in Console and `_transitionPlayer` is null (transitions become instant no-ops, but no crash).

---

## Edge Cases

### SetViewsForTesting still takes precedence over IViewResolver

In any test that calls `ctrl.SetViewsForTesting(mockView)` followed by `ctrl.Initialize(..., null)`:

**Expected:** The mock view set via `SetViewsForTesting()` is returned by the getter, not a view from the resolver (which is null). This confirms the override-first resolution order is intact. If the resolver were consulted first, passing `null` would cause a null-ref; the test would throw rather than passing silently.

---

### _viewResolver null + no SetViewsForTesting override → silent null (no crash)

In an edit-mode test that calls `ctrl.Initialize(..., null)` without `SetViewsForTesting()`:

**Expected:** The view getter returns null. The `Debug.LogError` fires identifying the missing view. No `NullReferenceException` is thrown because all popup handlers guard with `if (view == null) return;`.

---

### FindFirstObjectByType comment line in GameBootstrapper not counted as call

```bash
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs --count
```

**Expected:** Count is 3. Note that one match may appear in a doc comment (describing the old approach) — the `--count` flag counts lines, not call sites. Use `-n` to distinguish comment lines from actual `var ctrl = FindFirstObjectByType<T>()` call sites. All 3 actual call sites are scene controller lookups on lines 99, 113, 126.

---

## Failure Signals

- `rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs` returns any output → T01 not applied or reverted
- `rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs --count` returns anything other than 3 → T02 overcorrected (removed too many or too few calls)
- `rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs` returns fewer than 4 matches → T02 fields missing
- `rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs` returns fewer than 3 matches → SceneSetup wiring incomplete; Boot scene will have null fields after scene setup tool run
- Unity Console shows `[InGameSceneController] LevelCompleteView not found via IViewResolver.` at runtime → `_viewContainer` was null or does not hold a `LevelCompleteView` child; check Boot scene hierarchy and `GameBootstrapper._viewContainer` field
- Unity Console shows `[GameBootstrapper] Infrastructure ready.` is absent after boot → a serialized field was null; check Inspector for null refs on GameBootstrapper; re-run `Tools/Setup/Create And Register Scenes`
- CS7036 compile error (`no argument for parameter 'viewResolver'`) → `Initialize()` call site missing the null parameter; check all test files and GameBootstrapper

## Requirements Proved By This UAT

- **R072** — Scene controllers receive `IViewResolver` in `Initialize()` and use `Get<T>()` for popup views. Proved by: zero `FindFirstObjectByType` in both controllers (grep exit 1), `IViewResolver` field and parameter present (grep matches), 4 LogError signals intact (grep 4 matches).
- **R073** — `GameBootstrapper` has `[SerializeField]` refs for all boot infrastructure. Proved by: 4 `[SerializeField]` in `GameBootstrapper`, 3 remaining `FindFirstObjectByType` are scene controller lookups only, 3 `WireSerializedField` calls in `SceneSetup`.

## Not Proven By This UAT

- **R075** (zero FindFirstObjectByType in all of Assets/Scripts/) — not yet proven; 3 calls remain in `GameBootstrapper` for scene controller lookups, deferred to S03
- **R076** (164+ edit-mode tests pass in Unity batchmode) — deferred to S03; test compilation correctness is verified structurally (null parameter added to all call sites) but batchmode run is S03's responsibility
- **R077** (full game flow identical after refactor) — human UAT play-through deferred to S03
- **Boot scene runtime binding** — the `[SerializeField]` fields being correct in source does not guarantee the Boot scene file has the Inspector references populated; only the Unity Editor visual check (Test Case 11) or a batchmode test run confirms this

## Notes for Tester

- All artifact-driven checks (1–10) can be run from a terminal without opening Unity — they are pure file grep operations.
- Test Case 11 requires the Unity Editor and assumes `Tools/Setup/Create And Register Scenes` has been run successfully.
- The 3 remaining `FindFirstObjectByType` calls in `GameBootstrapper` are intentional — they are the S03 work item, not a bug in S02.
- If `rg "FindFirstObjectByType" Assets/Scripts/ --count` returns `GameBootstrapper.cs:4` instead of 3, check if a doc comment was added that accidentally contains the function name — use `-n` to see line numbers and verify which lines are call sites vs. comments.
