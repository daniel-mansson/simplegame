# S03: Scene Root Convention + Final Cleanup — UAT

## Prerequisites
- Unity Editor open with the project
- Scenes rebuilt via Tools/Setup/Create And Register Scenes

## Test Steps

1. **Verify zero FindFirstObjectByType in production**
   - Search `Assets/Scripts/` for "FindFirstObjectByType" — should find zero actual calls (comment in XML doc is OK)
   - Also search for "FindObjectOfType", "FindObjectsOfType", "FindAnyObjectByType" — all zero

2. **Run full test suite**
   - Run Edit Mode tests — all 169 should pass

3. **Full game loop play test**
   - Rebuild scenes: Tools/Setup/Create And Register Scenes
   - Play from Boot scene
   - MainMenu → Play → InGame
   - Place correct pieces → Win → LevelComplete popup → Continue → MainMenu
   - Play again → Place incorrect → Lose → LevelFailed popup → Retry → Win → MainMenu
   - Tap object in MainMenu (if golden pieces available) → ObjectRestored popup
   - Settings button → Settings screen → Back → MainMenu
   - All popups, transitions, and navigation should work identically to M006

## Expected Result
Zero FindObject* calls, all tests pass, full game loop works identically.
