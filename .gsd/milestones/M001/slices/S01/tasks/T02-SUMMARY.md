---
id: T02
parent: S01
milestone: M001
provides:
  - IView marker interface (SimpleGame.Core.MVP)
  - Presenter<TView> generic base class with two-phase lifecycle (SimpleGame.Core.MVP)
  - ISampleView sample view interface with event Action and UpdateLabel (SimpleGame.Core.MVP)
  - GameService plain C# domain service (SimpleGame.Core.Services)
  - SamplePresenter concrete presenter demonstrating constructor injection + event wiring (SimpleGame.Core.MVP)
  - UIFactory central factory wiring service into presenter (SimpleGame.Core.MVP)
key_files:
  - Assets/Scripts/Core/MVP/IView.cs
  - Assets/Scripts/Core/MVP/Presenter.cs
  - Assets/Scripts/Core/MVP/ISampleView.cs
  - Assets/Scripts/Core/MVP/SamplePresenter.cs
  - Assets/Scripts/Core/MVP/UIFactory.cs
  - Assets/Scripts/Core/Services/GameService.cs
key_decisions:
  - View interfaces use event Action (not UnityEvent) — keeps interfaces Unity-type-free and mockable in pure C# tests
  - Two-phase presenter lifecycle: constructor for injection, Initialize() for event subscription/async setup
  - Domain services live in SimpleGame.Core.Services namespace, separate from MVP infrastructure
patterns_established:
  - All view interfaces extend IView and expose events as event Action (not UnityEvent)
  - Presenters receive all dependencies via constructor; Initialize() subscribes events; Dispose() unsubscribes
  - UIFactory receives services once at construction, creates one presenter per Create call
  - No using UnityEngine in any Core type — pure C# throughout
observability_surfaces:
  - Compilation: grep "error CS" Logs/T02-compile.log (or Editor.log) — zero hits required
  - Static check: grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void|static class|static readonly|static async|static UniTask"
  - MonoBehaviour check: grep -r "MonoBehaviour" --include="*.cs" Assets/Scripts/Core/ — must return nothing
  - View isolation check: grep -n "Presenter\|GameService" Assets/Scripts/Core/MVP/ISampleView.cs — must return nothing
duration: ~10m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T02: Define MVP base types, sample view interface, and domain service

**6 core MVP C# files written to Assets/Scripts/Core/ — all compile cleanly in Unity batchmode (exit 0, zero error CS lines), all must-have checks pass.**

## What Happened

Created all 6 files specified in the task plan exactly as designed:

1. **IView.cs** — empty marker interface in `SimpleGame.Core.MVP`. Intentionally has no members.
2. **Presenter.cs** — generic abstract base `Presenter<TView> where TView : IView`. `View` is `protected readonly`, set once in constructor. `Initialize()` and `Dispose()` are `virtual` for override in subclasses. No constructor logic beyond field assignment — two-phase pattern is intentional for async-readiness in S02.
3. **ISampleView.cs** — extends `IView` with `event Action OnButtonClicked` and `void UpdateLabel(string text)`. Uses `System.Action` not `UnityEvent` — the foundational convention for all view interfaces.
4. **GameService.cs** — plain C# class in `SimpleGame.Core.Services`. Single method `GetWelcomeMessage()` returning a string. No MonoBehaviour, no static state.
5. **SamplePresenter.cs** — concrete `Presenter<ISampleView>`. Constructor takes `ISampleView` and `GameService`. `Initialize()` subscribes `HandleButtonClicked` to `View.OnButtonClicked` and calls `View.UpdateLabel` with the welcome message. `Dispose()` unsubscribes the handler.
6. **UIFactory.cs** — receives `GameService` in constructor, exposes `CreateSamplePresenter(ISampleView view)` returning a fully wired `SamplePresenter`.

Pre-flight fix: Added `## Observability Impact` section to T02-PLAN.md per the flagged gap — describes how to inspect compilation failures, static state violations, namespace resolution failures, and view interface contamination.

## Verification

**Unity batchmode compilation:**
```
Unity.exe -batchmode -projectPath C:\OtherWork\simplegame -quit -logFile Logs/T02-compile.log
EXIT: 0
grep "error CS" Logs/T02-compile.log → (no output)
```

**Static analysis (all pass — no output means pass):**
```bash
# Static field check
grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void|static class|static readonly|static async|static UniTask"
→ (no output) ✓

# MonoBehaviour check
grep -r "MonoBehaviour" --include="*.cs" Assets/Scripts/Core/
→ (no output) ✓

# UnityEngine using check
grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/
→ (no output) ✓

# View interface isolation
grep -n "Presenter\|GameService" Assets/Scripts/Core/MVP/ISampleView.cs
→ (no output) ✓

# Namespace check — all 6 files in correct namespaces
→ SimpleGame.Core.MVP (5 files), SimpleGame.Core.Services (1 file) ✓
```

**Must-Have checklist:**
- [x] `IView` is an empty marker interface — no methods, no properties
- [x] `Presenter<TView>` constrains `TView : IView`, stores view as `protected readonly`, has virtual `Initialize()` and `Dispose()`
- [x] `ISampleView` extends `IView` with `event Action OnButtonClicked` and `void UpdateLabel(string text)` — no Unity types
- [x] `GameService` is a plain C# class with no MonoBehaviour, no static state
- [x] `SamplePresenter` receives view and service via constructor, subscribes in Initialize, unsubscribes in Dispose
- [x] `UIFactory` receives `GameService` in constructor, has `CreateSamplePresenter(ISampleView)` method
- [x] No `static` fields holding state in any file
- [x] No `using UnityEngine` in any core type
- [x] All files in `SimpleGame.Core.MVP` or `SimpleGame.Core.Services` namespace

**Slice-level verification (partial — T03 not yet run):**
- Static state check: ✅ passes
- Batchmode compilation: ✅ exit 0, zero errors
- Edit-mode tests (`MVPWiringTests.cs`): ⏳ pending T03 — test file not yet written
- `TestResults.xml` zero failures: ⏳ pending T03

## Diagnostics

To inspect this task's outputs later:
```bash
# Check compilation health
grep -i "error CS\|will not be loaded" /c/OtherWork/simplegame/Logs/T02-compile.log

# Static field guard
grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"

# MonoBehaviour contamination guard
grep -r "MonoBehaviour" --include="*.cs" Assets/Scripts/Core/

# View isolation guard
grep -n "Presenter\|GameService\|UIFactory" Assets/Scripts/Core/MVP/ISampleView.cs

# Confirm all 6 files exist
find Assets/Scripts/Core -name "*.cs" | sort
```

## Deviations

None — all files implemented exactly as specified in the task plan.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IView.cs` — empty marker interface
- `Assets/Scripts/Core/MVP/Presenter.cs` — generic abstract base class with two-phase lifecycle
- `Assets/Scripts/Core/MVP/ISampleView.cs` — sample view interface (event Action, UpdateLabel)
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — concrete presenter with constructor injection and event wiring
- `Assets/Scripts/Core/MVP/UIFactory.cs` — central factory
- `Assets/Scripts/Core/Services/GameService.cs` — plain C# domain service
- `.gsd/milestones/M001/slices/S01/tasks/T02-PLAN.md` — added `## Observability Impact` section (pre-flight fix)
- `.gsd/milestones/M001/slices/S01/S01-PLAN.md` — marked T02 `[x]`
- `.gsd/DECISIONS.md` — appended decisions #3, #4, #5
