# S03: Scene Root Convention + Final Cleanup — UAT

**Milestone:** M007
**Written:** 2026-03-17

## UAT Type

- UAT mode: live-runtime + human-experience
- Why this mode is sufficient: S03 completes a structural refactor. All automated checks (grep + tests) pass. The remaining gap is behavioral correctness across the full game flow — only a human play-through in Unity play mode can confirm transitions, popups, and navigation are functionally identical to pre-M007.

## Preconditions

1. Unity Editor is open with the Boot scene loaded
2. All scripts compile with zero errors (check Console for any red errors before starting)
3. Play mode is not running (start fresh)
4. Confirm setup: `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType" Assets/Scripts/` returns no matches (zero output, exit 1)

## Smoke Test

Press **Play** in the Unity Editor. The Boot scene should initialize and navigate automatically to the MainMenu scene.

**Expected:** MainMenu scene loads additively. Main menu UI is visible. No errors in Console. Specifically, no `[GameBootstrapper] XxSceneController not found in scene.` error.

If this fails: check `GameBootstrapper.cs` lines 99, 113, 126 — `FindSceneController<T>(current.Value.ToString())` must be present. Verify the `MainMenu` scene name exactly matches `ScreenId.MainMenu.ToString()`.

---

## Test Cases

### 1. Boot → MainMenu navigation

1. Open Unity Editor. Ensure Boot scene is open.
2. Press Play.
3. Wait 2–3 seconds for Boot initialization.
4. **Expected:** MainMenu scene loads and the main menu UI is displayed. No `[GameBootstrapper]` error messages in Console. Unity Console shows `[GameBootstrapper] Boot sequence started.` and `[GameBootstrapper] Infrastructure ready. Starting navigation loop.`

### 2. MainMenu → InGame → Win flow

1. From MainMenu, tap/click the **Play** button.
2. **Expected:** Transition animation plays (fade out / fade in). InGame scene loads additively. In-game UI is visible (piece counter, heart indicators, place-correct/place-incorrect buttons).
3. Click **Place Correct** enough times to exhaust the piece counter (win condition).
4. **Expected:** Win dialog popup appears. Popup is displayed over the game UI. Input is blocked while popup is open (clicking the game background does nothing).
5. Dismiss the Win dialog (confirm/close button).
6. **Expected:** Transition plays. InGame scene unloads. MainMenu scene reloads. Main menu UI is visible again. No console errors.

### 3. InGame → Lose → Retry → Win flow

1. From MainMenu, press Play to enter InGame.
2. Click **Place Incorrect** enough times to lose all hearts.
3. **Expected:** Lose dialog popup appears. Input is blocked.
4. In the Lose dialog, click **Retry**.
5. **Expected:** InGame scene reloads (same level). Game state resets (hearts refilled, counter reset). No console errors during reload.
6. Complete the level by clicking Place Correct until win.
7. **Expected:** Win dialog appears. Dismiss it. Returns to MainMenu.

### 4. MainMenu → Settings → Back

1. From MainMenu, click the **Settings** button.
2. **Expected:** Settings scene loads additively via transition. Settings UI visible.
3. Click the **Back** button in Settings.
4. **Expected:** Transition plays. Settings scene unloads. MainMenu is visible again. No errors.

### 5. ConfirmDialog popup in MainMenu (if exposed)

1. From MainMenu, trigger any action that shows a confirmation dialog (if present in UI).
2. **Expected:** ConfirmDialog popup appears, blocks input. Confirm or cancel dismisses it cleanly.
3. After dismissal, normal MainMenu interaction is restored.

---

## Edge Cases

### Scene name mismatch (diagnostic only — do not expect to reproduce)

**If** `FindSceneController` returns null (scene controller not found), Unity Console will show:
```
[GameBootstrapper] MainMenuSceneController not found in scene.
```
or `SettingsSceneController` / `InGameSceneController`.

This would indicate either (a) the scene was not yet loaded when the lookup ran, or (b) the `ScreenId` enum value string does not match the scene name in EditorBuildSettings. This should not occur in a correctly configured project.

### Popup during transition

1. Navigate from MainMenu to InGame (transition running).
2. Do not interact during the fade transition.
3. **Expected:** Transition completes before any popups are shown. Input blocker prevents interaction during transition. No overlapping popup+transition artifacts.

### Back-to-back navigation (MainMenu → Play → Win → MainMenu → Play again)

1. Complete the win flow once (MainMenu → InGame → Win → MainMenu).
2. Immediately press Play again.
3. **Expected:** Second play session starts cleanly. InGame scene loads fresh. No stale state from the previous session.

---

## Failure Signals

- `[GameBootstrapper] XyzSceneController not found in scene.` in Console → `FindSceneController<T>` returned null; scene not loaded or name mismatch
- `[GameBootstrapper] No current screen — cannot find SceneController.` → screen manager lost track of the current screen
- Unity compilation errors (red console entries) → scripts failed to compile; check GameBootstrapper.cs for syntax errors
- Popup not appearing after win/lose → IViewResolver resolution failing; check ViewContainer children in Boot scene Inspector
- Input not blocked during popup → UnityInputBlocker not wired; check GameBootstrapper SerializeField `_inputBlocker` in Boot scene Inspector
- Transition not playing → UnityTransitionPlayer not wired; check GameBootstrapper SerializeField `_transitionPlayer` in Boot scene Inspector
- Test count < 169 in Unity Test Runner → K003 stale domain reload; close and reopen Unity editor

---

## Requirements Proved By This UAT

- **R077** — Game flow, popup behavior, transitions, and all user-visible interactions are identical after the M007 refactor; confirmed by full play-through
- **R074** (supporting) — Scene controllers correctly found at runtime via scene root convention (no `FindSceneController not found` errors)
- **R075** (supporting) — Zero FindObject* in production code; confirmed by observable correct runtime behavior with no implicit scanning errors
- **R009** — Boot scene stays persistent; MainMenu/Settings/InGame load additively (confirmed by scene load behavior)
- **R010** — Navigation via screen manager works end-to-end (confirmed by full flow)

## Not Proven By This UAT

- Test suite correctness (R076) — proved by automated Unity EditMode test run (169/169 passed), not by play-through
- Structural correctness of IViewResolver / container hierarchy (R069–R073) — proved in S01/S02 UAT
- Persistence/save behavior — not in scope for M007

## Notes for Tester

- The game uses stub gameplay (Place Correct / Place Incorrect buttons with a counter). This is intentional — core gameplay mechanics are deferred to a future milestone.
- Hearts start at a fixed count (3). Place Incorrect three times to trigger the Lose dialog.
- Piece counter starts at the level's configured count. Place Correct that many times to trigger the Win dialog.
- If Unity Console shows the Boot sequence messages and no red errors, the scene root convention is working correctly.
- The `mcporter call unityMCP.run_tests` CLI command crashes on Windows (K006). To re-run automated tests, use: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` then poll with `mcporter call unityMCP.get_test_job job_id=<id>`.
