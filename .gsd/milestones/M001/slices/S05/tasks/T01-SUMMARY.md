---
id: T01
parent: S05
milestone: M001
provides:
  - IMainMenuView, ISettingsView, IConfirmDialogView interfaces
  - MainMenuPresenter, SettingsPresenter, ConfirmDialogPresenter
  - UIFactory expanded with 3 Create methods and callback-based constructor
  - DemoWiringTests.cs with 17 passing edit-mode tests
key_files:
  - Assets/Scripts/Core/MVP/IMainMenuView.cs
  - Assets/Scripts/Core/MVP/ISettingsView.cs
  - Assets/Scripts/Core/MVP/IConfirmDialogView.cs
  - Assets/Scripts/Core/MVP/MainMenuPresenter.cs
  - Assets/Scripts/Core/MVP/SettingsPresenter.cs
  - Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs
  - Assets/Scripts/Core/MVP/UIFactory.cs
  - Assets/Tests/EditMode/DemoWiringTests.cs
key_decisions:
  - UIFactory keeps a backward-compatible `(GameService)` overload so the 6 existing MVPWiringTests compile unchanged
  - SettingsPresenter and ConfirmDialogPresenter call `.Forget()` on the returned UniTask (fire-and-forget) since event handlers are synchronous
  - ConfirmDialogPresenter handles both OnConfirmClicked and OnCancelClicked via the same `HandleDismiss` private method
patterns_established:
  - Mock view test doubles: expose LastTitleText/LastMessageText, UpdateCallCount, SimulateXClicked helpers
  - Presenter callback pattern: Action<ScreenId>/Action<PopupId> for sync navigation, Func<UniTask> for async back/dismiss
  - DemoWiringTests SetUp uses a shared UIFactory instance with counter-tracking lambdas
observability_surfaces:
  - DemoWiringTests.cs — 17 edit-mode tests; run `Unity -batchmode -runTests -testPlatform EditMode` to verify wiring
  - Presenter Initialize/Dispose traces visible via event subscription side-effects (UpdateCallCount in tests)
duration: ~30m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T01: Core view interfaces, presenters, UIFactory expansion, and edit-mode tests

**3 view interfaces, 3 presenters, expanded UIFactory, and 17 passing DemoWiringTests (49 total, 0 failures).**

## What Happened

Created all 6 pure C# types for the three demo screens (IMainMenuView, ISettingsView, IConfirmDialogView + their matching presenters) following the ISampleView/SamplePresenter patterns established in S01.

View interfaces follow the `event Action` convention (no UnityEngine) and extend IView or IPopupView. Presenters use constructor injection with callbacks: MainMenuPresenter takes `Action<ScreenId>` + `Action<PopupId>`, while SettingsPresenter and ConfirmDialogPresenter take `Func<UniTask>` for their async back/dismiss operations. The `Func<UniTask>` return value is fire-and-forgotten (`.Forget()`) since event handlers are synchronous — the async chain continues independently.

UIFactory was expanded to accept all four callbacks plus a backward-compatible `(GameService)` single-argument constructor (using default no-op lambdas for callbacks) so the 6 existing MVPWiringTests continue to compile and pass without modification.

DemoWiringTests.cs was written with 17 tests covering: construction (3), Initialize sets title/message (3), event→callback wiring (5 — SettingsClick, PopupClick, BackClick, ConfirmClick, CancelClick), Dispose unsubscribes (3), and mock views have no backward references (3). All mock views use the same pattern as MockSampleView.

A Unity batchmode recompile issue was encountered: the first `-runTests -quit` run compiled scripts but exited before the test runner wrote results (decision #7 in DECISIONS.md — omit `-quit` flag). The correct pattern without `-quit` yielded 49/49 passing.

## Verification

- **Batchmode test run**: 49 tests, 49 passed, 0 failed, 0 skipped (32 existing + 17 new)
  - `DemoWiringTests` fixture: 17 tests, all Passed
  - `MVPWiringTests` fixture: 6 tests, all Passed (no regressions)
- **No UnityEngine in Core**: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` → empty ✓
- **Static guard**: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"` → empty ✓
- **Compile**: exit 0, zero `error CS` hits ✓

## Diagnostics

Re-verify at any time with:
```
Unity -batchmode -nographics -projectPath <path> -runTests -testPlatform EditMode -testResults TestResults.xml -logFile test.log
```
Note: omit `-quit` — the test runner exits Unity automatically after writing results.

Presenter wiring inspection: check `MockXxxView.UpdateTitleCallCount > 0` after `Initialize()` and `== callsBefore` after `Dispose()` + event fire.

## Deviations

- UIFactory constructor: added a backward-compatible single-argument `(GameService)` overload not explicitly listed in the plan. Required to avoid breaking the 6 existing MVPWiringTests that construct `new UIFactory(_gameService)`.
- SettingsPresenter/ConfirmDialogPresenter: used `.Forget()` on UniTask returns from event handlers — plan described "invokes goBackCallback" without specifying synchronous vs async handling. `.Forget()` is the correct fire-and-forget pattern for UniTask in sync event handlers.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — new: view interface with OnSettingsClicked, OnPopupClicked, UpdateTitle
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — new: view interface with OnBackClicked, UpdateTitle
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — new: view interface extending IPopupView with OnConfirmClicked, OnCancelClicked, UpdateMessage
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — new: presenter with Action<ScreenId> + Action<PopupId> callbacks
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — new: presenter with Func<UniTask> go-back callback
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — new: presenter with Func<UniTask> dismiss callback (handles both confirm + cancel)
- `Assets/Scripts/Core/MVP/UIFactory.cs` — modified: full-callback constructor + backward-compatible (GameService) overload + 3 new Create methods
- `Assets/Tests/EditMode/DemoWiringTests.cs` — new: 17 tests with MockMainMenuView, MockSettingsView, MockConfirmDialogView
- `.gsd/milestones/M001/slices/S05/tasks/T01-PLAN.md` — modified: added Observability Impact section (pre-flight fix)
