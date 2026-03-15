---
estimated_steps: 6
estimated_files: 8
---

# T01: Core view interfaces, presenters, UIFactory expansion, and edit-mode tests

**Slice:** S05 — Boot Flow & Demo Screens
**Milestone:** M001

## Description

Create all pure C# types needed for the three demo screens (MainMenu, Settings, ConfirmDialog): view interfaces, presenters, and UIFactory expansion. Then write edit-mode tests proving the wiring contract for all three. This follows the exact patterns established in S01 (ISampleView/SamplePresenter/MockSampleView) and extends them to three real screens.

Key design: presenters receive navigation/popup actions as callbacks (`Action<ScreenId>`, `Action<PopupId>`, `Func<UniTask>`) rather than full manager references. This keeps presenters testable — mocks can supply simple lambda callbacks without constructing real ScreenManager/PopupManager instances.

## Steps

1. **Create `IMainMenuView`** in `Assets/Scripts/Core/MVP/IMainMenuView.cs` — extends `IView`; declares `event Action OnSettingsClicked`, `event Action OnPopupClicked`, `void UpdateTitle(string text)`. No `using UnityEngine`.

2. **Create `ISettingsView`** in `Assets/Scripts/Core/MVP/ISettingsView.cs` — extends `IView`; declares `event Action OnBackClicked`, `void UpdateTitle(string text)`. No `using UnityEngine`.

3. **Create `IConfirmDialogView`** in `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — extends `IPopupView`; declares `event Action OnConfirmClicked`, `event Action OnCancelClicked`, `void UpdateMessage(string text)`. No `using UnityEngine`.

4. **Create presenters** — `MainMenuPresenter : Presenter<IMainMenuView>` (constructor: view + `Action<ScreenId>` navigateCallback + `Action<PopupId>` showPopupCallback; Initialize: subscribes OnSettingsClicked → `navigateCallback(ScreenId.Settings)`, OnPopupClicked → `showPopupCallback(PopupId.ConfirmDialog)`, sets title; Dispose: unsubscribes). `SettingsPresenter : Presenter<ISettingsView>` (constructor: view + `Func<UniTask>` goBackCallback; Initialize: subscribes OnBackClicked → invokes goBackCallback, sets title; Dispose: unsubscribes). `ConfirmDialogPresenter : Presenter<IConfirmDialogView>` (constructor: view + `Func<UniTask>` dismissCallback; Initialize: subscribes OnConfirmClicked + OnCancelClicked → both invoke dismissCallback, sets message; Dispose: unsubscribes).

5. **Expand `UIFactory`** — Add `Action<ScreenId> _navigateCallback`, `Action<PopupId> _showPopupCallback`, `Func<UniTask> _goBackCallback`, `Func<UniTask> _dismissPopupCallback` to constructor. Add `CreateMainMenuPresenter(IMainMenuView)`, `CreateSettingsPresenter(ISettingsView)`, `CreateConfirmDialogPresenter(IConfirmDialogView)` methods.

6. **Write `DemoWiringTests.cs`** — Create `MockMainMenuView`, `MockSettingsView`, `MockConfirmDialogView` test doubles (follow MockSampleView pattern: expose LastTitleText/LastMessageText, UpdateCallCount, SimulateXClicked methods). Write tests: (a) UIFactory creates each presenter correctly, (b) Initialize sets initial title/message, (c) view events trigger correct callbacks, (d) Dispose unsubscribes, (e) mock views have no backward references.

## Must-Haves

- [ ] IMainMenuView, ISettingsView, IConfirmDialogView — all in Core, no UnityEngine, event Action convention
- [ ] MainMenuPresenter, SettingsPresenter, ConfirmDialogPresenter — constructor injection, two-phase lifecycle
- [ ] UIFactory expanded with 3 Create methods and callback-based constructor
- [ ] DemoWiringTests.cs with ≥9 tests covering construction, event wiring, disposal for all 3 presenters
- [ ] No static state fields in any new file
- [ ] No `using UnityEngine` in any Core/ file
- [ ] All existing 32 tests still pass (no regressions)

## Verification

- Batchmode test run: all tests pass (32 existing + ≥9 new), 0 failures
- `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns empty
- `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns empty
- Batchmode compile: exit 0, zero `error CS`

## Inputs

- `Assets/Scripts/Core/MVP/ISampleView.cs` — pattern for view interface (event Action + update method)
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — pattern for presenter (constructor injection, Initialize/Dispose)
- `Assets/Scripts/Core/MVP/UIFactory.cs` — existing factory to expand (currently only GameService + CreateSamplePresenter)
- `Assets/Scripts/Core/MVP/IPopupView.cs` — marker interface that IConfirmDialogView extends
- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — enum used in navigation callbacks
- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — enum used in popup callbacks
- `Assets/Tests/EditMode/MVPWiringTests.cs` — MockSampleView pattern to replicate for new mock views

## Observability Impact

**What signals change:** UIFactory now accepts four callbacks at construction time. When wired to real managers, `navigateCallback` and `showPopupCallback` are the entry points for all presenter-driven navigation — any failure to route a screen transition or popup will surface as a missing invocation of these callbacks. `goBackCallback` and `dismissPopupCallback` wrap async UniTask chains, so failures there appear as unresolved tasks or missing fade transitions.

**How a future agent inspects this task:** 
- `Assets/Tests/EditMode/DemoWiringTests.cs` contains 17 tests covering the full wiring contract. Run Unity batchmode `-runTests -testPlatform EditMode` to re-verify at any time.
- All three presenter types log no runtime output themselves; inspection is via the callback lambdas supplied at factory construction in `GameBootstrapper`.
- The MockXxxView test doubles use `UpdateCallCount` and `LastTitleText`/`LastMessageText` properties — usable in future tests to assert initialization state.

**Failure visibility:** If a presenter fails to call `UpdateTitle`/`UpdateMessage` during `Initialize()`, the view's text fields remain empty — visually obvious in Play mode. If `Dispose()` is not called, leaked event subscriptions will fire callbacks after screen transitions, producing NullReferenceExceptions or duplicate navigation calls.

## Expected Output

- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — view interface with OnSettingsClicked, OnPopupClicked, UpdateTitle
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — view interface with OnBackClicked, UpdateTitle
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — view interface with OnConfirmClicked, OnCancelClicked, UpdateMessage
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — presenter with navigation + popup callbacks
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — presenter with go-back callback
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — presenter with dismiss callback
- `Assets/Scripts/Core/MVP/UIFactory.cs` — modified: expanded constructor + 3 new Create methods
- `Assets/Tests/EditMode/DemoWiringTests.cs` — ≥9 tests with MockMainMenuView, MockSettingsView, MockConfirmDialogView
