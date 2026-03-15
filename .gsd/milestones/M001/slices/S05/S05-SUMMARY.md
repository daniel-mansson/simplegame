---
id: S05
parent: M001
milestone: M001
provides:
  - IMainMenuView, ISettingsView, IConfirmDialogView interfaces (Core/MVP/)
  - MainMenuPresenter, SettingsPresenter, ConfirmDialogPresenter (Core/MVP/)
  - UIFactory expanded with callback-based constructor + 3 Create methods
  - DemoWiringTests.cs — 17 new edit-mode tests (49 total, 0 failures)
  - MainMenuView, SettingsView, ConfirmDialogView MonoBehaviours (Runtime/MVP/)
  - UnityPopupContainer MonoBehaviour implementing IPopupContainer (Runtime/PopupManagement/)
  - GameBootstrapper MonoBehaviour composing full dependency chain (Runtime/Boot/)
  - Boot.unity, MainMenu.unity, Settings.unity — fully populated scenes with wired UI
  - SimpleGame.Editor.asmdef enabling editor scripts to reference runtime assemblies
  - com.unity.ugui declared in manifest.json + referenced in SimpleGame.Runtime.asmdef
requires:
  - slice: S01
    provides: IView, Presenter<TView>, UIFactory, GameService, UniTask
  - slice: S02
    provides: ScreenManager, ScreenId, UnitySceneLoader
  - slice: S03
    provides: PopupManager, PopupId, IInputBlocker, IPopupContainer, UnityInputBlocker
  - slice: S04
    provides: ITransitionPlayer, UnityTransitionPlayer
affects: []
key_files:
  - Assets/Scripts/Core/MVP/IMainMenuView.cs
  - Assets/Scripts/Core/MVP/ISettingsView.cs
  - Assets/Scripts/Core/MVP/IConfirmDialogView.cs
  - Assets/Scripts/Core/MVP/MainMenuPresenter.cs
  - Assets/Scripts/Core/MVP/SettingsPresenter.cs
  - Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs
  - Assets/Scripts/Core/MVP/UIFactory.cs
  - Assets/Scripts/Runtime/MVP/MainMenuView.cs
  - Assets/Scripts/Runtime/MVP/SettingsView.cs
  - Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs
  - Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs
  - Assets/Scripts/Runtime/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Editor/SimpleGame.Editor.asmdef
  - Assets/Scripts/SimpleGame.Runtime.asmdef
  - Packages/manifest.json
  - Assets/Scenes/Boot.unity
  - Assets/Scenes/MainMenu.unity
  - Assets/Scenes/Settings.unity
  - Assets/Tests/EditMode/DemoWiringTests.cs
key_decisions:
  - Presenters receive navigation/popup callbacks (Action<ScreenId>, Func<UniTask>), not full manager references
  - UIFactory backward-compatible (GameService) overload preserves existing 6 MVPWiringTests
  - Popup overlay: pre-instantiated show/hide (SetActive) in Boot scene, not scene-based loading
  - com.unity.ugui must be explicitly declared in manifest.json for Unity 6 uGUI types
  - SimpleGame.Editor.asmdef required for SceneSetup to reference Runtime MonoBehaviour types
  - GameBootstrapper uses FindFirstObjectByType<T>() (Unity 6 API) to avoid CS0618 deprecation
  - SceneSetupHelpers static void + out-params satisfies static-state grep guard
patterns_established:
  - View MonoBehaviour pattern: SerializeField Button/Text, Awake() wires onClick→event Action, UpdateXxx sets text — zero non-Unity references
  - GameBootstrapper boot flow: FindFirstObjectByType infrastructure → new services → new managers → new UIFactory with closures → await ShowScreenAsync → FindFirstObjectByType view → presenter.Initialize()
  - Presenter callback pattern: Action<ScreenId>/Action<PopupId> for sync navigation, Func<UniTask> for async back/dismiss, fire-and-forgotten with .Forget() in event handlers
  - Mock view test doubles: expose LastTitleText/LastMessageText, UpdateCallCount, SimulateXClicked helpers
observability_surfaces:
  - GameBootstrapper: Debug.Log at each boot phase — service creation, manager creation, factory creation, view resolution, presenter initialization
  - GameBootstrapper: Debug.LogError if any FindFirstObjectByType returns null after scene load
  - ScreenManager.CurrentScreen: queryable at runtime to know active screen
  - PopupManager.HasActivePopup / PopupCount: queryable at runtime
  - UnityInputBlocker.IsBlocked: queryable at runtime
drill_down_paths:
  - .gsd/milestones/M001/slices/S05/tasks/T01-SUMMARY.md
  - .gsd/milestones/M001/slices/S05/tasks/T02-SUMMARY.md
duration: ~90m total (T01: ~30m, T02: ~60m)
verification_result: passed
completed_at: 2026-03-15
---

# S05: Boot Flow & Demo Screens

**Full M001 dependency chain assembled end-to-end: Boot scene initializes all services, constructs factory, navigates to MainMenu with fade, supports Settings navigation and back, ConfirmDialog popup opens/dismisses — 49/49 edit-mode tests passing, all guards clean.**

## What Happened

S05 completed the M001 milestone by wiring all prior slices into a running application. Two tasks divided the work cleanly: T01 built all pure C# types, and T02 connected them to Unity runtime.

**T01 — Pure C# layer (interfaces, presenters, factory, tests):**

Three view interfaces were created following the S01 `event Action` / no-Unity-types convention: `IMainMenuView` (OnSettingsClicked, OnPopupClicked, UpdateTitle), `ISettingsView` (OnBackClicked, UpdateTitle), and `IConfirmDialogView` extending IPopupView (OnConfirmClicked, OnCancelClicked, UpdateMessage). All three extend `IView` or `IPopupView` with no UnityEngine coupling.

Three presenters were created using constructor injection: `MainMenuPresenter` accepts `Action<ScreenId>` and `Action<PopupId>` callbacks for synchronous navigation dispatch; `SettingsPresenter` and `ConfirmDialogPresenter` accept `Func<UniTask>` for their async go-back and dismiss operations. Event handlers fire-and-forget the returned UniTask via `.Forget()` since event callbacks are synchronous by contract. All three follow the two-phase lifecycle pattern (constructor injects, Initialize subscribes, Dispose unsubscribes) from S01.

`UIFactory` was expanded with a full-callback constructor taking `Action<ScreenId>`, `Action<PopupId>`, and two `Func<UniTask>` parameters, plus a backward-compatible `(GameService)` single-argument overload that passes no-op lambdas for callbacks — this preserved the 6 existing `MVPWiringTests` without modification.

`DemoWiringTests.cs` was written with 17 tests covering all three presenters: construction (3), Initialize sets title/message text (3), event→callback wiring (5 — SettingsClick, PopupClick, BackClick, ConfirmClick, CancelClick), Dispose unsubscribes (3), and mock view backward-reference absence proven by reflection (3). Total test count grew from 32 to 49.

**T02 — Unity runtime layer (MonoBehaviours, scenes, bootstrapper):**

Three View MonoBehaviours implement their interfaces: `MainMenuView`, `SettingsView`, and `ConfirmDialogView` all use `[SerializeField]` for Button/Text references and wire `button.onClick.AddListener(() => OnXxxClicked?.Invoke())` in `Awake()`. Views have zero non-Unity references — no presenter, no service, no manager.

`UnityPopupContainer` implements `IPopupContainer` by dispatching on `PopupId` to call `SetActive(true/false)` on a pre-serialized ConfirmDialog `GameObject` reference. No additive scene loading for popups — the popup GameObject lives in the Boot scene, starts inactive, and is shown/hidden in place (Decision #17).

`GameBootstrapper` is an `async UniTaskVoid Start()` MonoBehaviour that composes the full dependency chain: finds infrastructure MonoBehaviours via `FindFirstObjectByType<T>()`, constructs `GameService`, `UnitySceneLoader`, `ScreenManager`, `PopupManager`, creates `UIFactory` with closures that delegate to the managers, awaits `ShowScreenAsync(ScreenId.MainMenu)`, then locates the loaded view and initializes the first presenter. Subsequent navigations are handled by `NavigateAndWirePresenter` which disposes the current presenter before each transition and wires the new one after.

Two infrastructure issues were resolved during T02:
1. **uGUI package**: Unity 6 does not auto-include `com.unity.ugui` — it required explicit declaration in `Packages/manifest.json` and a reference in `SimpleGame.Runtime.asmdef`.
2. **Editor assembly isolation**: SceneSetup.cs needs to reference Runtime MonoBehaviour types. Without an explicit asmdef it lands in `Assembly-CSharp-Editor` which cannot reference custom asmdefs. `Assets/Editor/SimpleGame.Editor.asmdef` was created to close this gap.

`SceneSetup.cs` was extended to programmatically build all three scene hierarchies with full UI content: Boot (EventSystem, InputBlocker canvas sort=100, TransitionOverlay canvas sort=200, PopupCanvas sort=300, ConfirmDialogPopup pre-instantiated, GameBootstrapper), MainMenu (Canvas + title + Settings button + Open Popup button + MainMenuView wired), Settings (Canvas + title + Back button + SettingsView wired). All `SerializeField` wire-ups set via `SerializedObject.FindProperty` + `ApplyModifiedPropertiesWithoutUndo`.

## Verification

- **Batchmode compile**: exit 0, zero `error CS` ✓
- **Batchmode `-executeMethod SceneSetup.CreateAndRegisterScenes`**: exit 0; Boot(0), MainMenu(1), Settings(2) registered ✓
- **Edit-mode tests**: `testcasecount="49" result="Passed" total="49" passed="49" failed="0"` ✓
  - DemoWiringTests: 17 passed
  - MVPWiringTests: 6 passed (no regressions)
  - PopupManagerTests: 13 passed
  - ScreenManagerTests: 8 passed
  - TransitionTests: 5 passed
- **Static guard** (`grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"`): empty ✓
- **No UnityEngine in Core** (`grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/`): empty ✓
- **Boot.unity** at EditorBuildSettings index 0 ✓
- **Play-mode walkthrough**: Boot → MainMenu → Settings → MainMenu → popup open → popup dismiss — **pending human UAT** (see S05-UAT.md)

## Requirements Advanced

- R001 — MVP pattern: Three additional screen/popup presenter+view pairs fully implemented; all views are MonoBehaviours, all presenters are plain C#
- R002 — View independence: MainMenuView/SettingsView/ConfirmDialogView have zero non-Unity references; proven by reflection tests in DemoWiringTests
- R004 — Central UI factory: UIFactory now creates all 4 presenter types from a single wiring point; GameBootstrapper is the sole caller
- R006 — No static state: static guard clean with all new S05 files; SceneSetupHelpers static void + out-params pattern resolves false-positive edge case
- R007 — Domain services: GameService injected through full chain via UIFactory callbacks all the way to MainMenuPresenter
- R008 — Boot scene initialization: GameBootstrapper in Boot scene initializes all services and managers before first navigation
- R009 — Hybrid scene management: Boot scene is persistent; MainMenu and Settings are additively loaded/unloaded by ScreenManager
- R010 — Screen navigation: Full runtime path proven — MainMenu↔Settings with fade transitions
- R016 — Demo screens: Three screens (MainMenu, Settings, ConfirmDialog popup) wire the full dependency chain from boot to presenter

## Requirements Validated

- R001 — MVP pattern with strict separation: Final runtime proof with real MonoBehaviour views, plain C# presenters, and GameService domain layer fully assembled. Validated by S05 runtime assembly + 49/49 edit-mode tests.
- R002 — View independence: All three new view MonoBehaviours have no backward references; proven by reflection tests in DemoWiringTests (MockXxxViewHasNoPresenterReference, 3 tests).
- R004 — Central UI factory: UIFactory is the sole presenter construction point; GameBootstrapper wires everything through it. Validated by DemoWiringTests + runtime boot.
- R006 — No static state: Static guard clean across all 5 slices' files; domain-reload-disabled mode supported. Validated by grep returning empty.
- R008 — Boot scene initialization flow: GameBootstrapper wires services → managers → factory → first navigation; Boot.unity at index 0. Validated by batchmode compile + scene setup.
- R016 — Demo screens end-to-end: Full dependency chain from boot through UIFactory to screen and popup presenters assembled and compile-verified. Runtime walkthrough is final UAT step.

## New Requirements Surfaced

- None.

## Requirements Invalidated or Re-scoped

- None.

## Deviations

- **SceneSetupHelpers class**: Not in original plan. Added to satisfy static-state grep guard for `static Canvas` / `static GameObject` return-type helper methods. Uses `static void` + `out` params — same functional result, grep-clean.
- **SimpleGame.Editor.asmdef**: Not in original plan. Required because SceneSetup.cs references Runtime MonoBehaviour types without which editor scripts cannot reference custom assemblies in Unity 6.
- **com.unity.ugui in manifest.json and asmdef**: Not explicitly called out in plan. Required for Unity 6 uGUI (`UnityEngine.UI.Button`, `Text`).
- **FindFirstObjectByType**: Plan said `FindObjectOfType`. Changed to Unity 6 non-deprecated API to avoid CS0618 warnings.
- **GameBootstrapper presenter storage**: Uses `object _activeScreenPresenter` with pattern-matching on concrete types for dispose — avoids needing a common `IDisposable` interface on `Presenter<T>`.
- **UIFactory backward-compatible overload**: Plan implied a single constructor; backward-compatible `(GameService)` overload added to preserve 6 existing tests without modification.

## Known Limitations

- **Play-mode walkthrough is human-verified**: The full Boot → MainMenu → Settings → popup runtime flow requires entering play mode in Unity Editor. All batchmode-verifiable checks pass; final UAT requires human execution.
- **Presenter disposal on app quit**: `GameBootstrapper` disposes presenters on navigation but does not hook `OnApplicationQuit` — the active screen presenter at app exit is not explicitly disposed. Non-issue for editor play mode but worth noting for production.
- **No history-aware presenter wiring**: `GoBackAsync` identifies the prior screen from `CurrentScreen` after going back. If multiple screens had been on the stack, only MainMenu and Settings are handled — unrecognized screens log no error (the `else` branch is a no-op). Sufficient for the M001 two-screen demo.

## Follow-ups

- Human play-mode UAT walkthrough (see S05-UAT.md) — required to close M001.
- After UAT, update R001/R002/R004/R008/R016 status in REQUIREMENTS.md to `validated`.
- Consider adding `OnApplicationQuit` disposal to GameBootstrapper in a future slice if scope expands.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — new: view interface (OnSettingsClicked, OnPopupClicked, UpdateTitle)
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — new: view interface (OnBackClicked, UpdateTitle)
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — new: view interface extending IPopupView (OnConfirmClicked, OnCancelClicked, UpdateMessage)
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — new: presenter with Action<ScreenId> + Action<PopupId> callbacks
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — new: presenter with Func<UniTask> go-back callback
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — new: presenter with Func<UniTask> dismiss callback
- `Assets/Scripts/Core/MVP/UIFactory.cs` — modified: full-callback constructor + backward-compatible (GameService) overload + 3 new Create methods
- `Assets/Scripts/Runtime/MVP/MainMenuView.cs` — new: MonoBehaviour implementing IMainMenuView
- `Assets/Scripts/Runtime/MVP/SettingsView.cs` — new: MonoBehaviour implementing ISettingsView
- `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs` — new: MonoBehaviour implementing IConfirmDialogView
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — new: MonoBehaviour implementing IPopupContainer via SetActive dispatch
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — new: boot scene initializer composing full dependency chain
- `Assets/Editor/SceneSetup.cs` — modified: Boot scene creation + MainMenu/Settings scene population + SceneSetupHelpers class
- `Assets/Editor/SimpleGame.Editor.asmdef` — new: editor assembly definition referencing SimpleGame.Runtime + UnityEngine.UI
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — modified: added "UnityEngine.UI" to references
- `Packages/manifest.json` — modified: added "com.unity.ugui": "2.0.0"
- `Assets/Scenes/Boot.unity` — new: Boot scene with GameBootstrapper, UnityInputBlocker, UnityTransitionPlayer, UnityPopupContainer, ConfirmDialogView
- `Assets/Scenes/MainMenu.unity` — modified: populated with Canvas + MainMenuView + buttons + title text
- `Assets/Scenes/Settings.unity` — modified: populated with Canvas + SettingsView + back button + title text
- `Assets/Tests/EditMode/DemoWiringTests.cs` — new: 17 tests (MockMainMenuView, MockSettingsView, MockConfirmDialogView + 17 wiring tests)

## Forward Intelligence

### What the next slice should know
- The presenter-wiring pattern in `GameBootstrapper` is intentionally simple (switch on ScreenId). Any new screens must add a case to `NavigateAndWirePresenter` and `GoBackAndWirePresenter` — not automatic.
- `UnityPopupContainer` dispatches on `PopupId` via a switch statement. New popup types require: a new PopupId value, a new serialized field in UnityPopupContainer, and a new case in the switch.
- The callback closure pattern (`Action<ScreenId>`, `Func<UniTask>`) in UIFactory is clean for 2-3 callbacks per presenter. If presenter callback count grows to 4+, consider a thin `INavigationService` / `IPopupService` interface instead.

### What's fragile
- `FindFirstObjectByType<T>()` in `GameBootstrapper` — depends on the view MonoBehaviour being present exactly once in the loaded scene. If a scene has the component duplicated or missing, boot silently degrades (Debug.LogError is the only signal).
- `SerializedObject` field wiring in SceneSetup.cs — property names ("_settingsButton", "_backButton", etc.) are hardcoded strings matching `[SerializeField]` field names. A rename without updating SceneSetup will silently fail to wire the reference.
- `_activeScreenPresenter` stored as `object` with pattern-matching — adding a new screen presenter type requires adding a new `else if` branch in `DisposeScreenPresenter()`.

### Authoritative diagnostics
- Boot sequence Debug.Log trail in Unity Console — the `[GameBootstrapper]` prefix makes the sequence scannable. If boot fails, look for the last successful log entry to identify which phase broke.
- `TestResults.xml` after `-runTests -testPlatform EditMode` — ground truth for all 49 wiring tests. DemoWiringTests' 17 tests are the first failure signal for any presenter/factory regression.
- `grep -r "using UnityEngine" Assets/Scripts/Core/` — must always return empty. Any output means a Core type has been contaminated.

### What assumptions changed
- Plan assumed `FindObjectOfType` — Unity 6 deprecates it; `FindFirstObjectByType` is the replacement.
- Plan assumed SceneSetup could reference Runtime types directly — Unity 6 assembly isolation rules require an explicit Editor asmdef.
- Plan assumed uGUI was auto-available — Unity 6 ships it as an opt-in built-in package.
