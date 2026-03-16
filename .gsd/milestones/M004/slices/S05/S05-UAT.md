# S05 UAT — Full Loop Integration & Polish

## Prerequisites
- Unity Editor open with SimpleGame project

## Steps

1. **Trigger recompilation** — Click into the Unity Editor window (focus it) to trigger domain reload and recompilation
2. **Run scene setup** — Menu: Tools → Setup → Create And Register Scenes
   - This creates the InGame scene and updates Boot/MainMenu with win/lose popups and Play button
3. **Verify scenes exist** — Check Assets/Scenes/: Boot.unity, MainMenu.unity, Settings.unity, InGame.unity
4. **Enter play mode** — Open Boot.unity (or MainMenu.unity), press Play
5. **Test the full loop:**
   - Main menu shows "Level 1" and has a Play button
   - Click Play → InGame scene loads
   - Click +1 Score several times
   - Click Win → Win popup shows score and "Level 1 Complete!" → Click Continue
   - Main menu shows "Level 2"
   - Click Play again → InGame scene with Level 2
   - Click +1 Score a few times, then click Lose
   - Lose popup shows score and "Level 2" → Click Retry
   - Score resets, still Level 2 → Click Win → Win popup → Continue
   - Main menu shows "Level 3"
6. **Run all edit-mode tests** — Should be 98 tests, all green (may show 89 until editor restart resolves the test file detection)

## Expected Results
- Full game loop works: menu → play → score → win/lose → popup → menu reflects progress
- InGame scene works when started directly from editor (uses default level 1)
- 98/98 tests pass (or 89/89 if PopupTests.cs not yet detected — restart Unity to fix)
