# S02: Scene Controller View Resolution + Boot SerializeField Refs — UAT

## Prerequisites
- Unity Editor open with the project
- Scenes rebuilt via Tools/Setup/Create And Register Scenes

## Test Steps

1. **Verify no FindFirstObjectByType in scene controllers**
   - Search `Assets/Scripts/Game/InGame/` and `Assets/Scripts/Game/MainMenu/` for "FindFirstObjectByType"
   - Should find zero results

2. **Verify GameBootstrapper SerializeField refs**
   - Open `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
   - Confirm `[SerializeField]` fields for `_inputBlocker`, `_transitionPlayer`, `_viewContainer`
   - Only scene controller lookups should remain as FindFirstObjectByType (3 in nav loop)

3. **Run tests**
   - Run Edit Mode tests — all 169 should pass

4. **Play test**
   - Rebuild scenes: Tools/Setup/Create And Register Scenes
   - Play from Boot scene
   - Navigate MainMenu → InGame → Win → MainMenu
   - All popups should display correctly

## Expected Result
All checks pass. Scene controllers resolved, boot infra wired explicitly.
