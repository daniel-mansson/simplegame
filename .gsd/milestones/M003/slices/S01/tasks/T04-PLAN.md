---
estimated_steps: 7
estimated_files: 2
---

# T04: Update UIFactory + rewrite DemoWiringTests for async result API

**Slice:** S01 — Presenter Async Result API
**Milestone:** M003

## Description

Strip all callback params from `UIFactory` — `Create*` methods just construct `new Presenter(view)`. Remove the legacy `(GameService)` overload. Rewrite `DemoWiringTests` to test the new await-based API: simulate view events, await result tasks, assert resolved values. `GameBootstrapper` will fail to compile at this point (it still passes callbacks) — that's expected and will be fixed in S02.

## Steps

1. Read `UIFactory.cs` in full; remove all callback ctor params and the legacy overload; `Create*` methods become single-line `return new XxxPresenter(view)`; `GameService` field removed if no longer needed (check if `CreateMainMenuPresenter` still needs it — after S01 it does not)
2. Read `DemoWiringTests.cs` in full to understand what to replace
3. Remove `SetUp` UIFactory construction with callbacks; update construction: `_factory = new UIFactory()` (or `new UIFactory(_gameService)` if GameService kept — but prefer removing it); update mock fields — remove `_lastNavigatedScreen`, `_lastShownPopup`, `_goBackCallCount`, `_dismissCallCount`
4. Rewrite callback-assertion tests as async result tests using `[UnityTest]` or NUnit async pattern with UniTask: `SimulateSettingsClicked()` fires while `WaitForAction()` is pending → assert result is `MainMenuAction.Settings`; same for Popup, Back, Confirm, Cancel
5. Rewrite Dispose tests: after `Dispose()`, TCS is cancelled; awaiting `WaitForBack()` after dispose either throws OperationCanceledException or never resolves — test that the presenter is inert (no side-effects on subsequent event fires)
6. Keep view-independence reflection tests unchanged
7. Run tests via Unity MCP: `run_tests` (EditMode), poll `get_test_job` until complete; assert all pass

## Must-Haves

- [ ] `UIFactory` has no callback ctor params; `Create*` methods are single-line constructors
- [ ] All DemoWiringTests pass with the new async API
- [ ] Total test count ≥ 49 (rewritten tests replace old ones; net count may be same or higher)
- [ ] `grep -rn "Action<ScreenId>\|Action<PopupId>\|Func<UniTask>" Assets/Scripts/Game/` returns empty
- [ ] `grep -rn "\.Forget()" Assets/Scripts/` returns empty

## Observability Impact

- Signals added/changed: test failures in DemoWiringTests directly identify which presenter result task is broken
- How a future agent inspects this: `run_tests` via MCP or batchmode; `TestResults.xml`
- Failure state exposed: NUnit assertion messages name the presenter and the expected vs actual result value

## Verification

- Unity MCP: `run_tests` with testPlatform=EditMode → `get_test_job` until done → assert passed count matches total
- Fallback batchmode: `"C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" -batchmode -runTests -projectPath . -testPlatform EditMode -testResults TestResults.xml -logFile test.log`
- `grep -rn "\.Forget()" Assets/Scripts/` → empty
- `grep -rn "Action<ScreenId>\|Action<PopupId>\|Func<UniTask>" Assets/Scripts/Game/` → empty

## Inputs

- `Assets/Scripts/Game/Boot/UIFactory.cs` — needs callback params stripped
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — needs full rewrite for async API
- T01/T02/T03 outputs — presenter files with new async API (must be done first)

## Expected Output

- `Assets/Scripts/Game/Boot/UIFactory.cs` — simplified; no callbacks
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — rewritten; async result tests; all passing
