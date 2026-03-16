# S01: Presenter Async Result API

**Goal:** Remove all outbound callbacks from presenter constructors. Replace with domain-specific `UniTaskCompletionSource<T>`-backed result methods that SceneControllers will await in S02. View→Presenter `event Action` subscriptions are unchanged.
**Demo:** `ConfirmDialogPresenter.WaitForConfirmation()` returns `UniTask<bool>` that resolves true/false when the mock view fires Confirm/Cancel. `MainMenuPresenter.WaitForAction()` returns `UniTask<MainMenuAction>` that resolves when Settings or Popup is clicked. `SettingsPresenter.WaitForBack()` returns `UniTask` that resolves on back. All three constructors take only `TView`. Updated DemoWiringTests prove the new API in edit-mode. All 49 tests pass.

## Must-Haves

- No presenter constructor accepts a callback (`Action`, `Func<UniTask>`, etc.)
- `ConfirmDialogPresenter.WaitForConfirmation()` → `UniTask<bool>`: true = confirm, false = cancel; resets on each call
- `MainMenuPresenter.WaitForAction()` → `UniTask<MainMenuAction>` where `MainMenuAction` is `{ Settings, Popup }`
- `SettingsPresenter.WaitForBack()` → `UniTask`; resolves when back is pressed
- `UIFactory` constructor updated — no callback params; `GameBootstrapper` compilation verified
- All 49 edit-mode tests pass (DemoWiringTests rewritten for new API)
- `grep -rn "\.Forget()" Assets/Scripts/` returns empty — no production `.Forget()` introduced

## Proof Level

- This slice proves: contract
- Real runtime required: no
- Human/UAT required: no

## Verification

- Run via Unity MCP: `run_tests` with EditMode filter — check `get_test_job` until complete; assert total=49 passed=49
- Fallback: `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- `grep -rn "Action<ScreenId>\|Action<PopupId>\|Func<UniTask>" Assets/Scripts/Game/` returns empty (no callbacks on presenter ctors)

## Integration Closure

- Upstream surfaces consumed: existing presenter files, `UIFactory`, `DemoWiringTests`
- New wiring introduced: none — pure presenter API change; `GameBootstrapper` will fail to compile if callbacks remain (compile error is the integration signal)
- What remains before milestone is usable end-to-end: S02 (SceneControllers), S03 (boot-from-any-scene)

## Tasks

- [ ] **T01: Rewrite ConfirmDialogPresenter with WaitForConfirmation()** `est:30m`
  - Why: Establishes the UniTaskCompletionSource result-task pattern for the simplest case (bool result, two trigger events)
  - Files: `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs`
  - Do: Remove `Func<UniTask> dismissCallback` ctor param. Add `UniTaskCompletionSource<bool> _confirmTcs` field. `WaitForConfirmation()` creates a new TCS, stores it, returns the task. `HandleConfirm` completes TCS with true; `HandleCancel` with false. `Initialize()` subscribes both events; `Dispose()` unsubscribes. Guard: if TCS already exists when `WaitForConfirmation()` is called again, cancel the old one first.
  - Verify: Compile check via `validate_script` MCP tool; T02 tests will prove behaviour
  - Done when: file compiles, no callback param, `WaitForConfirmation()` exists returning `UniTask<bool>`

- [ ] **T02: Add MainMenuAction enum + rewrite MainMenuPresenter with WaitForAction()** `est:30m`
  - Why: Establishes the discriminated-result pattern for screens with multiple possible user actions
  - Files: `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` (new), `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs`
  - Do: Create `MainMenuAction.cs` enum with `Settings` and `Popup` values in `SimpleGame.Game.MainMenu` namespace. Remove callback ctor params from `MainMenuPresenter`. Add `UniTaskCompletionSource<MainMenuAction> _actionTcs`. `WaitForAction()` creates/returns TCS task. `HandleSettingsClicked` completes with `MainMenuAction.Settings`; `HandlePopupClicked` with `MainMenuAction.Popup`. Reset TCS on each `WaitForAction()` call.
  - Verify: Compile check via `validate_script`; T04 tests will prove behaviour
  - Done when: `MainMenuAction.cs` exists, presenter compiles with no callbacks, `WaitForAction()` returns `UniTask<MainMenuAction>`

- [ ] **T03: Rewrite SettingsPresenter with WaitForBack()** `est:20m`
  - Why: Completes the presenter API — simplest case, no result value
  - Files: `Assets/Scripts/Game/Settings/SettingsPresenter.cs`
  - Do: Remove `Func<UniTask> goBackCallback` ctor param. Add `UniTaskCompletionSource _backTcs`. `WaitForBack()` creates TCS, returns task. `HandleBackClicked` calls `_backTcs.TrySetResult()`. Reset on each call.
  - Verify: Compile check via `validate_script`; T04 tests will prove behaviour
  - Done when: presenter compiles with no callbacks, `WaitForBack()` returns `UniTask`

- [ ] **T04: Update UIFactory + rewrite DemoWiringTests** `est:45m`
  - Why: UIFactory must stop passing callbacks; tests must prove the new await-based API end-to-end
  - Files: `Assets/Scripts/Game/Boot/UIFactory.cs`, `Assets/Tests/EditMode/Game/DemoWiringTests.cs`
  - Do: Strip all callback params from `UIFactory` constructor — `Create*` methods just `new Presenter(view)`. Remove legacy `(GameService)` overload (no longer needed without callback complexity). Update `GameBootstrapper` imports if needed (it will fail to compile until UIFactory is fixed — that's expected and is the integration signal). Rewrite `DemoWiringTests`: construction tests unchanged; replace callback-assertion tests with async result tests — `SimulateConfirmClicked()` → `await WaitForConfirmation()` resolves true; `SimulateCancelClicked()` → resolves false; `SimulateSettingsClicked()` → `WaitForAction()` resolves `MainMenuAction.Settings`; etc. Dispose tests: after `Dispose()`, completing TCS does nothing / presenter is inert.
  - Verify: Run tests via Unity MCP `run_tests` → `get_test_job`; assert passed=49 (or higher if new tests added). Fallback: batchmode `-runTests -testPlatform EditMode`
  - Done when: all tests pass, `grep -rn "Action<ScreenId>\|Action<PopupId>\|Func<UniTask>" Assets/Scripts/Game/` returns empty

## Files Likely Touched

- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` (new)
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs`
- `Assets/Scripts/Game/Settings/SettingsPresenter.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs`
