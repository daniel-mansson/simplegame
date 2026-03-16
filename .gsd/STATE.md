# GSD State

**Active Milestone:** none
**Active Slice:** none
**Phase:** milestone complete — awaiting next milestone or UAT

## Milestone Registry
- ✅ **M001:** MVP UI Architecture Foundation (pending play-mode UAT)
- ✅ **M002:** Assembly Restructure — Core/Game Separation
- ✅ **M003:** SceneController Architecture — Async Control Flow (pending play-mode UAT)

## Recent Decisions
- SceneController is a MonoBehaviour per scene; RunAsync() loops internally, returns only when navigating away
- Presenters expose domain-specific result tasks (WaitForAction, WaitForBack, WaitForConfirmation) — no outbound callbacks
- UniTask TCS cancellation observable by awaiting (TaskCanceledException) — not via .IsCanceled on AsTask()
- ConfirmDialogView lives in Boot scene; discovered at runtime via FindFirstObjectByType
- BootInjector [RuntimeInitializeOnLoadMethod] loads Boot additively if not present (play-from-any-scene)
- MCP run_tests: use `mcporter call unityMCP.run_tests mode=EditMode` then poll `get_test_job`

## Blockers
- None

## Pending UAT
- M003: enter play mode from MainMenu.unity; confirm Boot loads additively, main menu appears, Settings nav and popup work
- M001: see `.gsd/milestones/M001/slices/S05/S05-UAT.md`

## Verification Signals (current)
- 58/58 edit-mode tests pass
- `grep -rn ".Forget()" Assets/Scripts/` → empty
- Scenes `MainMenu.unity` and `Settings.unity` contain wired SceneControllers
