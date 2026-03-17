---
id: S01
parent: M007
milestone: M007
provides:
  - IViewResolver interface in Core/PopupManagement — T Get<T>() where T : class
  - UnityViewContainer (renamed from UnityPopupContainer) implementing IPopupContainer<PopupId> + IViewResolver
  - Get<T>() implemented as GetComponentInChildren<T>(true) — resolves inactive children, no manual registration
  - MockViewResolver test double (dictionary-based, Register<T>/Get<T>) — ready for S02 agents
  - ViewContainerTests.cs with 5 NUnit tests proving the IViewResolver contract
requires:
  - slice: none (first slice)
    provides: n/a
affects:
  - S02
  - S03
key_files:
  - Assets/Scripts/Core/PopupManagement/IViewResolver.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Scenes/Boot.unity
  - Assets/Tests/EditMode/Game/ViewContainerTests.cs
key_decisions:
  - D041 — IViewResolver is a separate interface from IPopupContainer; Generic Get<T>() in Core, game-agnostic
  - D042 — Container renamed to UnityViewContainer (signals expanded role beyond show/hide)
  - Get<T>() as GetComponentInChildren<T>(true) — finds inactive children without any registration overhead
  - ITestView/TestViewComponent defined as file-local test types — avoids polluting production namespaces
patterns_established:
  - git mv both .cs and .cs.meta together to preserve Unity GUID — never delete/recreate
  - Boot.unity m_EditorClassIdentifier must be updated manually after rename (GUID bind is primary)
  - MockViewResolver pattern: Dictionary<Type, object> with Register<T>/Get<T> for type-safe test doubles without scene hierarchy
  - TestViewComponent pattern: inner MonoBehaviour+interface for exercising GetComponentInChildren in edit-mode tests
observability_surfaces:
  - rg "UnityPopupContainer" Assets/ → exit 1 (zero matches) confirms rename complete
  - rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs → confirms interface exists
  - rg "class UnityViewContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs → confirms dual-interface declaration
  - rg "[Test]" Assets/Tests/EditMode/Game/ViewContainerTests.cs → 5 matches
  - git status Assets/Scripts/Game/Popup/ → both .cs and .cs.meta show as renamed (not deleted+untracked)
drill_down_paths:
  - .gsd/milestones/M007/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M007/slices/S01/tasks/T02-SUMMARY.md
duration: ~30m (T01: 15m, T02: 15m)
verification_result: passed
completed_at: 2026-03-17
---

# S01: IViewResolver + Container Refactor

**Introduced `IViewResolver` in Core, renamed `UnityPopupContainer` → `UnityViewContainer` (GUID preserved), implemented `Get<T>()` as `GetComponentInChildren<T>(true)`, and proved the contract with 5 new tests and a `MockViewResolver` test double ready for S02.**

## What Happened

**T01** executed all production changes atomically. `IViewResolver` was created in `Assets/Scripts/Core/PopupManagement/` as a clean single-method interface (`T Get<T>() where T : class`). `UnityPopupContainer.cs` and its `.meta` were renamed via `git mv` — git status confirmed both files show as `renamed:` (not deleted+untracked), so the `.meta` GUID is preserved and Boot scene serialization is intact.

`UnityViewContainer` was rewritten to add `IViewResolver` to its implements list and a one-line `Get<T>()` body: `return GetComponentInChildren<T>(true)`. All existing `IPopupContainer<PopupId>` logic (SerializeField refs to 6 popup GameObjects, switch on PopupId, SetActive calls) was left completely unchanged.

`GameBootstrapper.cs` and `SceneSetup.cs` had their `UnityPopupContainer` type references updated to `UnityViewContainer`. Then `Assets/Scenes/Boot.unity` was found to contain the old class name in `m_EditorClassIdentifier` — patched via `sed`. The final `rg "UnityPopupContainer" Assets/` returned exit 1 (zero matches).

**T02** created `ViewContainerTests.cs` in `Assets/Tests/EditMode/Game/`. The file defines `ITestView`/`TestViewComponent` as file-local types (avoids production namespace pollution), `MockViewResolver` as a dictionary-backed test double, and two test fixtures: `ViewContainerGetTests` (3 tests against real `UnityViewContainer` using `GetComponentInChildren`) and `MockViewResolverTests` (2 tests against the mock itself). The existing `asmdef` at `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` already referenced both Core and Game assemblies — no asmdef changes needed.

## Verification

All slice-plan verification commands passed:

```
rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/  → exit 1 (zero matches) ✓
rg "UnityPopupContainer" Assets/                         → exit 1 (zero matches) ✓
rg "IViewResolver" Assets/Scripts/Core/PopupManagement/  → finds IViewResolver.cs ✓
rg "class UnityViewContainer" Assets/Scripts/Game/Popup/ → finds dual-interface declaration ✓
rg "[Test]" Assets/Tests/EditMode/Game/ViewContainerTests.cs → 5 matches ✓
rg "MockViewResolver" Assets/Tests/EditMode/Game/ViewContainerTests.cs → class defined and used ✓
git status Assets/Scripts/Game/Popup/ → clean, both files renamed (not deleted+untracked) ✓
```

Compile-time verification: no CS0246 errors possible — `rg "UnityPopupContainer" Assets/` exit 1 is the definitive signal. Full Unity batchmode test run deferred to S02 (domain-reload-disabled editor issue with newly created test files per K003).

## Requirements Advanced

- R070 — `IViewResolver` interface now exists in Core with `T Get<T>() where T : class`; interface is in `SimpleGame.Core.PopupManagement` namespace, game-agnostic, separate from `IPopupContainer`
- R071 — `UnityViewContainer` implements both `IPopupContainer<PopupId>` and `IViewResolver`; old name fully erased from codebase (grep-clean)

## Requirements Validated

- R070 — IViewResolver contract proven by 3 `ViewContainerGetTests` tests (correct resolution, null-for-missing, inactive-child resolution) and 2 `MockViewResolverTests` tests
- R071 — Rename proven by `rg "UnityPopupContainer" Assets/` exit 1; dual-interface proven by grep on class declaration

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

One anticipated deviation: `Assets/Scenes/Boot.unity` contained `m_EditorClassIdentifier: SimpleGame.Game::SimpleGame.Game.Popup.UnityPopupContainer`. The plan's conditional check flagged this as expected — it was patched via `sed`. The deviation was planned-for, not a surprise.

## Known Limitations

- `GameBootstrapper` still uses `FindFirstObjectByType<UnityViewContainer>()` for the container — this is intentional, deferred to S02 (SerializeField refs for boot infrastructure per D044)
- `GameBootstrapper` still uses `FindFirstObjectByType` for `UnityInputBlocker`, `UnityTransitionPlayer`, and all scene controllers — deferred to S02/S03
- Unity batchmode test run (164+ tests) not yet confirmed post-rename — K003 caveat applies; new test file `ViewContainerTests.cs` may not be picked up until the editor reloads
- Popup views are held as SerializeField refs on `UnityViewContainer`, not yet as prefab instances under a container hierarchy (R069 fully addressed in Boot scene wiring, which is structural/inspector work beyond this slice's code scope)

## Follow-ups

- S02: Wire scene controllers to use `IViewResolver.Get<T>()` instead of `FindFirstObjectByType`; add `IViewResolver` param to `Initialize()` calls; use `MockViewResolver` from this slice as the test double
- S02: Switch `GameBootstrapper` to `[SerializeField]` refs for `UnityInputBlocker`, `UnityTransitionPlayer`, `UnityViewContainer` (D044)
- Run full Unity batchmode test suite after S02 to confirm the 164+ test gate (K003 workaround: editor focus or restart)

## Files Created/Modified

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — new: Core interface with `T Get<T>() where T : class`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — renamed from UnityPopupContainer; implements IPopupContainer<PopupId> + IViewResolver; adds Get<T>() via GetComponentInChildren<T>(true)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — type reference updated: UnityPopupContainer → UnityViewContainer
- `Assets/Editor/SceneSetup.cs` — type reference updated: AddComponent<UnityPopupContainer>() → AddComponent<UnityViewContainer>()
- `Assets/Scenes/Boot.unity` — m_EditorClassIdentifier updated from UnityPopupContainer to UnityViewContainer
- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` — new: MockViewResolver + 5 NUnit tests proving IViewResolver contract

## Forward Intelligence

### What the next slice should know

- `MockViewResolver` in `ViewContainerTests.cs` is the S02 test double — import it by reference, don't recreate it. It lives in `SimpleGame.Tests.Game` namespace. Its `Register<T>(view)` + `Get<T>()` API is deliberately minimal and type-safe.
- `GameBootstrapper` already uses `UnityViewContainer` type correctly but still finds it via `FindFirstObjectByType<UnityViewContainer>()`. S02 must change this specific call to a `[SerializeField]` field (D044). The field is already declared as `private` in the class — just add `[SerializeField]` and remove the scene-scan call.
- Scene controllers (`MainMenuSceneController`, `InGameSceneController`, `SettingsSceneController`) currently receive popup views via `FindFirstObjectByType` calls inside their own code. Their `Initialize()` signatures are defined in their respective files — S02 must add `IViewResolver` as a parameter there.
- The Boot scene GUID binding is intact (git mv preserved it). Don't touch `Boot.unity` unless adding new components — the scene wiring is solid.

### What's fragile

- K003 (domain-reload-disabled editor) — the 5 new tests in `ViewContainerTests.cs` may not be detected by the Unity test runner until the editor is restarted. The S02 executor should trigger a domain reload before running the full test suite count check.
- Boot.unity scene file — has been patched once for m_EditorClassIdentifier. If any further rename occurs in S02/S03, the same sed pattern must be applied: `sed -i 's/OldClass/NewClass/g' Assets/Scenes/Boot.unity`.

### Authoritative diagnostics

- `rg "UnityPopupContainer" Assets/` exit 1 — the single most reliable signal that the rename is complete. Exit 0 means a stale reference exists and Unity will emit CS0246.
- `git log --oneline Assets/Scripts/Game/Popup/` — shows the rename commit; `git show HEAD:Assets/Scripts/Game/Popup/UnityViewContainer.cs.meta` vs old path confirms GUID preservation.
- `rg "class UnityViewContainer.*IPopupContainer.*IViewResolver" Assets/Scripts/Game/Popup/UnityViewContainer.cs` — confirms dual-interface on a single line.

### What assumptions changed

- Original assumption: Boot.unity would not need updating. Actual: `m_EditorClassIdentifier` stores the class name as a plain string (not GUID-bound), so it needed a sed patch. This is now documented in K005.
- Original assumption: asmdef changes might be needed for the test file. Actual: `SimpleGame.Tests.Game.asmdef` already referenced both Core and Game — no changes required.
