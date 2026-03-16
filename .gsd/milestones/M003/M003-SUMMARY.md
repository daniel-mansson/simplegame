---
id: M003
status: complete
slices_complete: 3
tests_passed: 58
tests_total: 58
---

# M003 Summary: SceneController Architecture — Async Control Flow

## What Was Delivered

Replaced the callback-based presenter wiring and fire-and-forget async model with a clean SceneController-per-scene pattern. Each scene has a MonoBehaviour SceneController with a `RunAsync()` entry point that loops internally until navigation away is decided. All control flow is linear and readable. No callbacks on presenters. No `.Forget()` in production paths.

## Architecture After M003

```
GameBootstrapper (Boot scene)
  ├── Builds: GameService, ScreenManager, PopupManager, UIFactory
  └── Navigation loop:
        ShowScreenAsync(MainMenu)
        loop:
          MainMenuSceneController.RunAsync() → ScreenId
          ├── MainMenuPresenter.WaitForAction() → MainMenuAction
          │   ├── Settings → return ScreenId.Settings
          │   └── Popup  → HandleConfirmPopupAsync inline → loop
          ShowScreenAsync(result)
          SettingsSceneController.RunAsync() → ScreenId
          ├── SettingsPresenter.WaitForBack() → return ScreenId.MainMenu
          ...

BootInjector [RuntimeInitializeOnLoadMethod]
  └── Additively loads Boot.unity if not already loaded (play-from-any-scene)
```

## Slices

- ✅ S01: Presenter async result API — `WaitForAction()`, `WaitForBack()`, `WaitForConfirmation()`; no outbound callbacks
- ✅ S02: SceneController MonoBehaviours — `ISceneController`, `MainMenuSceneController`, `SettingsSceneController`; `GameBootstrapper` replaced; no `.Forget()`
- ✅ S03: Boot-from-any-scene — `BootInjector`, SceneSetup updated, scenes regenerated

## Key Decisions

- **UniTask TCS cancellation** — only observable by awaiting (throws `TaskCanceledException`); `.IsCanceled` on `.AsTask()` wrapper is async and unreliable
- **ConfirmDialogView runtime discovery** — lives in Boot scene; found via `FindFirstObjectByType<ConfirmDialogView>()` at popup time, not cross-scene SerializeField
- **`internal` not cross-assembly visible** — testing helpers made `public`
- **MCP invocation** — use `mcporter call unityMCP.run_tests mode=EditMode` (pi's `mcp_call` sends wrong param name)

## Verification

- 58/58 edit-mode tests pass
- `grep -rn ".Forget()" Assets/Scripts/` → empty
- Scenes `MainMenu.unity` and `Settings.unity` contain wired SceneControllers
- Pending: play-mode UAT (human verification: start from MainMenu.unity, confirm boot loads, nav works)

## Requirement Coverage

- Covers: R001 (MVP pattern improved), R004 (UIFactory central), R008 (boot flow), R010 (screen navigation)
- Covers: R014 (UniTask async — pattern improved), R015 (edit-mode tests: 53→58)
