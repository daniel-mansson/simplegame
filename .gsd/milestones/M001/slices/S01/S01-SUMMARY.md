---
id: S01
parent: M001
milestone: M001
provides:
  - Unity 6000.3.4f1 project at C:\OtherWork\simplegame, compiling cleanly with UniTask
  - IView marker interface (SimpleGame.Core.MVP)
  - Presenter<TView> abstract base with two-phase lifecycle: ctor injection + Initialize()/Dispose()
  - ISampleView view interface with event Action and UpdateLabel — no Unity types
  - SamplePresenter concrete presenter demonstrating constructor injection + event subscribe/unsubscribe
  - UIFactory central factory wiring GameService into SamplePresenter
  - GameService plain C# domain service
  - SimpleGame.Runtime.asmdef + SimpleGame.Tests.EditMode.asmdef
  - 6 passing NUnit edit-mode tests in MVPWiringTests.cs
  - TestResults.xml: result="Passed", total="6", failed="0"
requires: []
affects:
  - S02
  - S03
  - S04
  - S05
key_files:
  - Packages/manifest.json
  - Packages/packages-lock.json
  - Assets/Scripts/SimpleGame.Runtime.asmdef
  - Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef
  - Assets/Scripts/Core/MVP/IView.cs
  - Assets/Scripts/Core/MVP/Presenter.cs
  - Assets/Scripts/Core/MVP/ISampleView.cs
  - Assets/Scripts/Core/MVP/SamplePresenter.cs
  - Assets/Scripts/Core/MVP/UIFactory.cs
  - Assets/Scripts/Core/Services/GameService.cs
  - Assets/Tests/EditMode/MVPWiringTests.cs
  - TestResults.xml
key_decisions:
  - UniTask installed via git URL — portable, resolves cleanly in batchmode without OpenUPM
  - Assembly references use string names (not GUIDs) — portable for CLI-only workflows
  - View interfaces use event Action (not UnityEvent) — keeps interfaces Unity-type-free, mockable in pure C#
  - Two-phase presenter lifecycle: constructor for injection, Initialize() for event subscription
  - GameService in SimpleGame.Core.Services namespace — separates domain logic from MVP infrastructure
  - com.unity.test-framework added to manifest (was absent from default project)
  - -quit must NOT be used with -runTests batchmode (races the async test runner)
  - MockSampleView uses UpdateLabelCallCount int for disposal testing (string equality gives false pass)
patterns_established:
  - All view interfaces extend IView; expose events as event Action (no Unity types)
  - Presenters: constructor sets fields only; Initialize() subscribes events; Dispose() unsubscribes
  - UIFactory receives services once at construction; creates one presenter per Create call
  - No using UnityEngine in any Core type — pure C# throughout
  - MockSampleView pattern: LastLabelText + UpdateLabelCallCount + SimulateButtonClick()
observability_surfaces:
  - Packages/packages-lock.json — com.cysharp.unitask entry at commit ad5ed25e82a3 confirms UniTask resolved
  - Logs/Editor.log / Logs/T02-compile.log — grep "error CS" must return zero hits
  - TestResults.xml — result="Passed" failed="0" total="6" at project root
  - Static guard: grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"
drill_down_paths:
  - .gsd/milestones/M001/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M001/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M001/slices/S01/tasks/T03-SUMMARY.md
duration: ~50m total (T01: ~10m, T02: ~10m, T03: ~30m)
verification_result: passed
completed_at: 2026-03-15
---

# S01: Core MVP Infrastructure & Project Setup

**Unity 6000.3.4f1 project compiling with UniTask, 6 MVP core types defined, 6 edit-mode tests passing — TestResults.xml: result="Passed", failed="0", total="6".**

## What Happened

Three tasks ran sequentially to establish the full MVP infrastructure foundation:

**T01 — Project, UniTask, folder structure, assembly definitions**
Created the Unity project via `Unity.exe -batchmode -createProject`, then added UniTask as a git URL dependency in `Packages/manifest.json` before the first full project open. Created the folder hierarchy (`Assets/Scripts/Core/MVP/`, `Assets/Scripts/Core/Services/`, `Assets/Tests/EditMode/`) and two assembly definitions: `SimpleGame.Runtime.asmdef` (references UniTask, autoReferenced) and `SimpleGame.Tests.EditMode.asmdef` (references runtime, UniTask, and test framework; editor-only). A second batchmode pass confirmed compilation — exit code 0, zero `error CS` hits, UniTask resolved at commit `ad5ed25e82a3`. `.gitignore` was updated with Unity-standard entries.

**T02 — MVP base types, sample view interface, and domain service**
Six pure C# files were created with no `using UnityEngine` anywhere in `Assets/Scripts/Core/`:
- `IView.cs` — empty marker interface, no members
- `Presenter<TView>.cs` — abstract base storing `protected readonly TView View`; `Initialize()` and `Dispose()` are virtual
- `ISampleView.cs` — extends `IView`; declares `event Action OnButtonClicked` and `void UpdateLabel(string text)`
- `SamplePresenter.cs` — concrete presenter; constructor injects `ISampleView` + `GameService`; `Initialize()` subscribes to `OnButtonClicked` and sets the welcome label; `Dispose()` unsubscribes
- `UIFactory.cs` — receives `GameService` at construction; `CreateSamplePresenter(ISampleView)` returns a fully-wired `SamplePresenter`
- `GameService.cs` — plain C# class with `GetWelcomeMessage()` returning a string

All static analysis checks passed (no static state, no MonoBehaviour in Core, no `using UnityEngine` in Core, ISampleView has no Presenter/Service references). Batchmode compilation: exit 0, zero errors.

**T03 — Edit-mode tests proving MVP wiring**
Two blockers were found and resolved during this task:
1. `com.unity.test-framework` was absent from the default project manifest — added version `1.4.5` (Unity resolved it to `1.6.0`)
2. The `-quit` flag in the batchmode test run races the async test runner, preventing `TestResults.xml` from being written — removed `-quit`; the test runner exits on its own

`MVPWiringTests.cs` contains a `MockSampleView` test double (tracks `LastLabelText`, `UpdateLabelCallCount`, exposes `SimulateButtonClick()`) and 6 test methods:
1. `PresenterCanBeConstructedWithMockView` — direct construction with mock view and service
2. `UIFactoryCreatesSamplePresenterWithService` — factory creates correctly-typed presenter
3. `PresenterInitializeSetsWelcomeLabel` — Initialize() calls UpdateLabel via injected service
4. `PresenterRespondsToViewEvents` — SimulateButtonClick() causes UpdateLabel to be called again
5. `PresenterDisposeUnsubscribesFromViewEvents` — UpdateLabelCallCount does not increase after Dispose
6. `MockViewHasNoPresenterReference` — reflection confirms MockSampleView fields reference no Presenter/Service/UIFactory types

All 6 pass. `TestResults.xml`: `result="Passed" total="6" passed="6" failed="0"`.

## Verification

**Static state guard — returns nothing (no violations):**
```
grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
→ (no output, exit 1 = no matches)
```

**UniTask resolved:**
```
Packages/packages-lock.json — com.cysharp.unitask@ad5ed25e82a3 present
```

**Compilation clean:**
```
Unity.exe -batchmode -projectPath ... -quit -logFile Logs/T02-compile.log
Exit code: 0
grep "error CS" Logs/T02-compile.log → (no output)
```

**Edit-mode tests:**
```
TestResults.xml: result="Passed" total="6" passed="6" failed="0"

Tests:
- MockViewHasNoPresenterReference        ✓ Passed
- PresenterCanBeConstructedWithMockView  ✓ Passed
- PresenterDisposeUnsubscribesFromViewEvents ✓ Passed
- PresenterInitializeSetsWelcomeLabel    ✓ Passed
- PresenterRespondsToViewEvents         ✓ Passed
- UIFactoryCreatesSamplePresenterWithService ✓ Passed
```

## Requirements Advanced

- R001 — MVP pattern with strict separation: IView, Presenter<TView>, ISampleView, SamplePresenter and UIFactory establish the separation contract in code
- R002 — View independence: ISampleView has zero references to presenter, service, or factory types; verified by grep and reflection test
- R003 — Interface-per-view: ISampleView is the first concrete instance of this pattern; presenter depends only on the interface, never the concrete view
- R004 — Central UI factory: UIFactory is the single wiring point — receives GameService once, creates SamplePresenter via CreateSamplePresenter()
- R005 — Constructor/init injection only: all dependencies passed via constructors; no service locator, no DI framework
- R006 — No static state: grep check passes with no output; two-phase lifecycle pattern explicitly avoids static fields
- R007 — Domain services: GameService established as the pattern for plain C# domain services injected into presenters
- R014 — UniTask installed: com.cysharp.unitask resolved and compiling in the project
- R015 — Edit-mode unit tests: 6 tests established the pattern and toolchain; all pass in batchmode
- R017 — Each layer testable in isolation: mock view implements ISampleView in pure C# with no Unity runtime needed

## Requirements Validated

- R014 — UniTask async/await: UniTask compiles and is available in the project (packages-lock.json confirms resolution; zero compile errors)
- R015 — Edit-mode unit tests: 6 edit-mode tests pass in Unity batchmode CLI — pattern and toolchain proven
- R003 — Interface-per-view: ISampleView + SamplePresenter demonstrates the interface dependency pattern end-to-end, confirmed by passing tests
- R005 — Constructor/init injection only: UIFactory + SamplePresenter wiring verified by test — no DI framework, no service locator
- R017 — Each layer testable in isolation: MockSampleView provides view isolation without Unity runtime; all presenter tests pass in edit-mode

## New Requirements Surfaced

- None

## Requirements Invalidated or Re-scoped

- None

## Deviations

- **`com.unity.test-framework` not in original T01 plan** — the package was absent from the project's default manifest; adding it was necessary for NUnit support. The T01 asmdef already referenced `UnityEngine.TestRunner` and `UnityEditor.TestRunner`, correctly anticipating this — so adding the package itself is additive, not contradictory.
- **`-quit` removed from batchmode test run command** — the T03 plan's verification step included `-quit` which races the async test runner. Removed for correct operation. Future test run documentation omits this flag.
- **`com.unity.test-framework` resolved to 1.6.0 (not 1.4.5)** — Unity's package resolver upgraded from the specified version. This is expected behavior and 1.6.0 is compatible.

## Known Limitations

- `GameService.GetWelcomeMessage()` is a stub returning a hardcoded string — intentional for S01; real domain logic will be added in S05
- `UIFactory.CreateSamplePresenter()` is the only factory method — factory will grow in S02/S03/S05 as screen and popup presenters are added
- The two-phase lifecycle (Initialize separate from constructor) is established but not exercised asynchronously yet — async usage begins in S02

## Follow-ups

- S02 should add `IScreenView : IView` and `ScreenManager` using the established patterns
- S03 should add `IPopupView : IView` and `PopupManager`
- UIFactory will need additional `Create*` methods for each screen/popup presenter introduced in S02–S05
- The `com.unity.test-framework` package version should be pinned explicitly in manifest.json if future Unity resolver upgrades cause instability

## Files Created/Modified

- `Packages/manifest.json` — added UniTask git URL and com.unity.test-framework
- `Packages/packages-lock.json` — auto-generated; confirms UniTask at ad5ed25e82a3
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime assembly definition referencing UniTask
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — test assembly referencing runtime, UniTask, test framework
- `Assets/Scripts/Core/MVP/IView.cs` — empty marker interface
- `Assets/Scripts/Core/MVP/Presenter.cs` — generic abstract base with two-phase lifecycle
- `Assets/Scripts/Core/MVP/ISampleView.cs` — sample view interface (event Action + UpdateLabel)
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — concrete presenter with constructor injection and event wiring
- `Assets/Scripts/Core/MVP/UIFactory.cs` — central factory
- `Assets/Scripts/Core/Services/GameService.cs` — plain C# domain service
- `Assets/Tests/EditMode/MVPWiringTests.cs` — 6 NUnit edit-mode tests with MockSampleView
- `TestResults.xml` — Unity test runner output: 6/6 passed, 0 failures
- `.gitignore` — appended Unity-standard ignore entries
- `.gsd/DECISIONS.md` — created with decisions 1–8

## Forward Intelligence

### What the next slice should know
- The two-phase lifecycle (constructor injects, Initialize subscribes) is load-bearing for S02's async scene lifecycle — Initialize() is where async tasks should start, not in the constructor
- `event Action` on view interfaces (not UnityEvent) is the pattern — MockSampleView must not use any Unity types; keep this consistent in IScreenView and IPopupView
- `com.unity.test-framework` is installed at 1.6.0; the test runner exits on its own — do NOT add `-quit` to `-runTests` batchmode commands
- UIFactory receives all services at construction and creates presenters on demand — when S02 adds ScreenManager, it should be a dependency of UIFactory (passed at construction), not obtained globally

### What's fragile
- UniTask is installed via git URL (not a fixed tag) — if the HEAD of the UniTask repo changes incompatibly, future project opens may resolve a different commit. Low risk for now but worth pinning a tag in the git URL for production projects.
- The `autoReferenced: true` in SimpleGame.Runtime.asmdef means all Unity assemblies can reference it — fine for the demo scope but should be removed once the project grows to avoid accidental coupling.

### Authoritative diagnostics
- `TestResults.xml` at project root — single source of truth for test pass/fail; `result="Passed" failed="0"` is the green bar
- `Logs/Editor.log` (or any `-logFile` target) — grep `"error CS"` for compilation failures; grep `"com.cysharp.unitask"` for package resolution confirmation
- `Packages/packages-lock.json` — confirms what actually resolved (commit hash, resolved version) vs. what was requested in manifest

### What assumptions changed
- Original assumption: `com.unity.test-framework` would be present by default — it was not. Must be added manually to manifest for any Unity project that needs NUnit tests.
- Original assumption: `-quit` is compatible with `-runTests` — it is not. The test runner is async and honors `-quit` before completing. Remove `-quit` from all `-runTests` invocations.
