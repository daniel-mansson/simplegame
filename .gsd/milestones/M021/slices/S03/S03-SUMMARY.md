---
id: S03
parent: M021
milestone: M021
provides:
  - MainMenuFlowPresenter pure C# class with full navigation loop and popup orchestration
  - MainMenuSceneController slimmed to 82 lines (wiring board only)
  - SetViewsForTesting delegated from controller to flow presenter
  - InSceneScreenManager construction moved from controller to Initialize()
key_files:
  - Assets/Scripts/Game/MainMenu/MainMenuFlowPresenter.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
key_decisions:
  - "SetScreenManagerForTesting removed — not called in any test"
  - "Bee dag cache cleared (K011) after stale compile errors from quick succession edits"
patterns_established:
  - "Same flow presenter pattern as InGame: pure C# class owns RunAsync; controller stores and delegates"
duration: 25min
verification_result: pass
completed_at: 2026-03-26T14:45:00Z
---

# S03: Slim MainMenuSceneController

**Navigation loop, popup flows, environment helpers, and debug ad logic moved to MainMenuFlowPresenter.**

## What Happened

Created `MainMenuFlowPresenter` with the complete `RunAsync` loop previously in `MainMenuSceneController`: `MainMenuPresenter` lifecycle, all action dispatch (Settings/Play/ObjectRestored/ResetProgress/NextEnvironment/OpenShop/DebugAds), `HandleObjectRestoredPopupAsync`, `ShowConfirmDialogAsync`, `HandleShopScreenAsync`, `HandleDebugRewardedAsync`, `HandleDebugInterstitialAsync`, `GetCurrentEnvironment`, `HasNextEnvironment`.

`MainMenuSceneController` builds the `InSceneScreenManager` in `Initialize()`, creates `MainMenuFlowPresenter` with all dependencies, and delegates `RunAsync` and `SetViewsForTesting`.

Encountered Bee dag cache stale state (K011) after editing `InGameFlowPresenter` and `InGameSceneController` in the same compile cycle — cleared dag and rebuilt from scratch.

## Deviations

None.

## Files Created/Modified

- `Assets/Scripts/Game/MainMenu/MainMenuFlowPresenter.cs` — new, full navigation loop
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — delegating wiring board
