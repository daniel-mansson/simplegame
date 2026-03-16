---
id: S02
milestone: M003
status: complete
tests_passed: 58
tests_total: 58
---

# S02 Summary: SceneController MonoBehaviours + Async Control Flow

## What Was Built

Introduced `ISceneController` interface and three SceneController MonoBehaviours. `GameBootstrapper` was replaced — it now builds infrastructure and drives a navigation loop via `await ctrl.RunAsync()`, containing no navigation logic itself. All production async paths are fully awaited; no `.Forget()` anywhere.

## Files Changed

- `Assets/Scripts/Game/Boot/ISceneController.cs` (new) — `UniTask<ScreenId> RunAsync(CancellationToken ct = default)`
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` (new) — MonoBehaviour; `[SerializeField] SettingsView`; creates SettingsPresenter, awaits `WaitForBack()`, returns `ScreenId.MainMenu`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` (new) — MonoBehaviour; `[SerializeField]` MainMenuView + ConfirmDialogView; loops on `WaitForAction()`; handles popup inline via `HandleConfirmPopupAsync()`; returns `ScreenId.Settings` when settings chosen
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — replaced stub with real navigation loop: builds infra, `ShowScreenAsync(MainMenu)`, switch-on-current-screen loop finding and running SceneControllers
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` (new) — 5 tests: Settings back, Settings re-run, MainMenu settings click, popup-then-settings, popup-cancel-then-settings

## Key Decisions Made

- **View override pattern for testing** — SceneControllers hold `[SerializeField]` concrete view refs for Unity wiring, plus `_viewOverride` interface fields; `SetViewsForTesting()` (public) injects mocks; `ActiveView` property picks override if set
- **`internal` not visible cross-assembly** — made testing helpers `public` instead of `internal` (simpler than `InternalsVisibleTo`)
- **Pure synchronous test execution** — UniTask TCS completions chain synchronously in edit-mode; popup-then-settings test fires all clicks in sequence without any `await` between them

## Verification

- 58/58 edit-mode tests pass (53 S01 + 5 new)
- `grep -rn ".Forget()" Assets/Scripts/` → empty
