---
id: S01
milestone: M003
status: complete
tests_passed: 53
tests_total: 53
---

# S01 Summary: Presenter Async Result API

## What Was Built

Replaced all outbound callbacks on presenter constructors with `UniTaskCompletionSource<T>`-backed result methods. The pattern is now: SceneController calls `WaitForAction()` / `WaitForBack()` / `WaitForConfirmation()` and awaits the result; the presenter completes the TCS when the user acts.

## Files Changed

- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — removed `Func<UniTask> dismissCallback`; added `UniTaskCompletionSource<bool> _confirmTcs`; added `WaitForConfirmation() → UniTask<bool>`
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` (new) — `{ Settings, Popup }` enum
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — removed callback params; added `WaitForAction() → UniTask<MainMenuAction>`
- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — removed callback param; added `WaitForBack() → UniTask`
- `Assets/Scripts/Game/Boot/UIFactory.cs` — stripped to `(GameService)` ctor; three single-line `Create*` methods
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — stubbed compilable version (replaced in S02)
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — rewritten for async result API; 20 tests total

## Key Decisions Made

- **TCS cancellation is only observable by awaiting** — `TaskCanceledException` (subclass of `OperationCanceledException`) thrown when awaited after `TrySetCanceled()`; `IsCanceled` on a `Task` wrapper is not reliable synchronously
- **NUnit `Assert.ThrowsAsync<T>` requires exact type** — use `TaskCanceledException` not `OperationCanceledException`
- **`mcporter call unityMCP.run_tests mode=EditMode`** is the correct invocation; pi's `mcp_call` sends wrong param name (`testPlatform` vs `mode`)

## Verification

- 53/53 edit-mode tests pass
- `grep -rn ".Forget()" Assets/Scripts/` → empty
- `grep -rn "Action<ScreenId>|Func<UniTask>" Assets/Scripts/Game/` → empty
