---
id: S05
parent: M001
milestone: M001
written: 2026-03-15
---

# S05: Boot Flow & Demo Screens — UAT

**Milestone:** M001: MVP UI Architecture Foundation
**Written:** 2026-03-15

## UAT Type

- UAT mode: live-runtime (play-mode walkthrough in Unity Editor)
- Why this mode is sufficient: S05 is the final-assembly slice. All pure C# logic was verified by 49 edit-mode tests. The UAT proves the full runtime dependency chain — GameBootstrapper wiring, scene loading with fade transitions, view MonoBehaviour event dispatch, presenter response, and popup stack behavior — which is only observable in play mode.

## Preconditions

1. Unity 6000.3.x editor is open with the `C:/OtherWork/simplegame` project loaded.
2. Batchmode `-executeMethod SceneSetup.CreateAndRegisterScenes` has been run and completed with exit 0 (all three scenes exist and are registered in EditorBuildSettings).
3. Boot.unity is at EditorBuildSettings index 0 (File → Build Settings — verify Boot is first).
4. Open Boot.unity in the editor (File → Open Scene → Assets/Scenes/Boot.unity).
5. No compilation errors in the Console window.

## Smoke Test

Open Boot.unity, enter play mode. Within 2 seconds the MainMenu scene should load additively, the screen should display a title ("Main Menu") and two buttons ("Settings", "Open Popup"), and the Unity Console should show the `[GameBootstrapper] Boot sequence complete. Ready.` log line. If this passes, basic wiring is functional.

## Test Cases

### 1. Boot initializes and navigates to MainMenu

1. Open Boot.unity (ensure it is the active scene).
2. Enter play mode (Ctrl+P / ▶).
3. Watch the Console window during startup.
4. **Expected:** The following Debug.Log lines appear in order:
   ```
   [GameBootstrapper] Boot sequence started.
   [GameBootstrapper] GameService created.
   [GameBootstrapper] UnityInputBlocker found: True
   [GameBootstrapper] UnityTransitionPlayer found: True
   [GameBootstrapper] UnityPopupContainer found: True
   [GameBootstrapper] UnitySceneLoader created.
   [GameBootstrapper] ScreenManager created.
   [GameBootstrapper] PopupManager created.
   [GameBootstrapper] UIFactory created.
   [GameBootstrapper] Navigating to MainMenu...
   [GameBootstrapper] MainMenu scene loaded.
   [GameBootstrapper] MainMenuPresenter initialized.
   [GameBootstrapper] Boot sequence complete. Ready.
   ```
5. **Expected:** The Game view shows the MainMenu canvas with a "Main Menu" title, a "Settings" button, and an "Open Popup" button. No pink/missing objects.
6. **Expected:** No errors or exceptions in the Console (warnings about deprecated APIs are acceptable if not breaking).

### 2. Fade transition plays during navigation

1. (Continue from Test 1 — still in play mode, MainMenu showing.)
2. Watch the Game view carefully while clicking the "Settings" button.
3. **Expected:** A brief fade-to-black transition plays before the Settings scene appears — the screen fades out, the old scene unloads, the Settings scene loads, then fades back in.
4. **Expected:** The Settings canvas appears with a "Settings" title and a "Back" button.
5. **Expected:** Input is blocked (no buttons respond) during the fade duration.
6. **Expected:** Console shows no errors; no `ScreenManager` concurrency-guard warnings.

### 3. Settings presenter initializes after navigation

1. (Continue from Test 2 — Settings screen showing.)
2. Inspect the Console.
3. **Expected:** `[GameBootstrapper] SettingsPresenter initialized.` log line appears after navigation completes.
4. **Expected:** Settings title text is displayed (updated by SettingsPresenter.Initialize via UpdateTitle).

### 4. Back navigation returns to MainMenu with fade

1. (Continue from Test 3 — Settings screen showing.)
2. Click the "Back" button.
3. **Expected:** Fade transition plays (same as forward navigation).
4. **Expected:** MainMenu canvas reappears with "Main Menu" title and both buttons.
5. **Expected:** Console shows `[GameBootstrapper] MainMenuPresenter initialized.` after return.
6. **Expected:** No duplicate presenter instances — old SettingsPresenter was disposed before navigation.

### 5. Popup opens over MainMenu and blocks input

1. (Continue from Test 4 — MainMenu showing.)
2. Click the "Open Popup" button.
3. **Expected:** The ConfirmDialog popup appears over the MainMenu (rendered on PopupCanvas, sort order 300).
4. **Expected:** The popup shows a message (e.g. "Are you sure?") and two buttons: "OK" / "Confirm" and "Cancel".
5. **Expected:** Clicking the MainMenu canvas buttons behind the popup does NOT trigger navigation (input is blocked by UnityInputBlocker).
6. **Expected:** Console shows `[GameBootstrapper] ConfirmDialogPresenter initialized.`

### 6. Popup dismisses cleanly on Confirm

1. (Continue from Test 5 — ConfirmDialog popup showing.)
2. Click the "OK" / "Confirm" button.
3. **Expected:** The popup disappears (SetActive(false)).
4. **Expected:** The MainMenu canvas is fully interactive again (input unblocked).
5. **Expected:** No errors in Console.
6. **Expected:** `PopupManager.HasActivePopup` is false (can verify via the Hierarchy or by attempting another popup open — no stacking issue).

### 7. Popup dismisses cleanly on Cancel

1. (Reopen the popup by clicking "Open Popup" from MainMenu.)
2. Click the "Cancel" button.
3. **Expected:** Same dismissal behavior as Test 6 — popup disappears, input unblocked, no errors.

### 8. Full round-trip sequence

1. Boot → MainMenu (fade) → Settings (fade) → MainMenu (fade) → Open Popup → Confirm dismiss.
2. **Expected:** Every step works cleanly in sequence. No "navigation already in progress" errors. No frozen input state. No null reference exceptions.
3. **Expected:** All Debug.Log boot lines appeared at start; no `[GameBootstrapper] ... found: False` lines.

## Edge Cases

### Boot scene is not index 0

1. Open File → Build Settings. Verify Boot.unity is at index 0.
2. If not: press "Run SceneSetup" or re-run batchmode `-executeMethod SceneSetup.CreateAndRegisterScenes`, then verify.
3. **Expected after fix:** Boot.unity is first in the list. Entering play mode from Boot scene works.

### Missing component on Boot scene objects

1. If `UnityInputBlocker found: False`, `UnityTransitionPlayer found: False`, or `UnityPopupContainer found: False` appears in Console during boot:
2. Re-run `SceneSetup.CreateAndRegisterScenes` via batchmode (or `Tools → Create/Register Scenes` if a menu item exists) to regenerate the Boot scene with all components.
3. **Expected after fix:** All three `found: True` lines appear on next play mode entry.

### Rapid button spam during transition

1. While a fade transition is playing (click Settings to start fade), immediately click other buttons.
2. **Expected:** No second navigation starts. The ScreenManager concurrency guard (`_isNavigating` flag) prevents a second `ShowScreenAsync` from starting.
3. **Expected:** No Console errors from double-navigation.

### Exit play mode and re-enter

1. Exit play mode (Ctrl+P), then immediately re-enter.
2. **Expected:** Boot sequence runs cleanly from scratch — no stale static state from the previous session. All `[GameBootstrapper]` log lines appear fresh.
3. **Expected:** This is the domain-reload-disabled mode test — no static fields means re-entry always starts clean.

## Failure Signals

- `[GameBootstrapper] UnityInputBlocker found: False` → InputBlocker MonoBehaviour missing from Boot scene. Re-run SceneSetup.
- `[GameBootstrapper] MainMenuView not found after scene load` → MainMenuView component not on the canvas in MainMenu.unity. Re-run SceneSetup.
- NullReferenceException on first fade → CanvasGroup not wired on TransitionOverlay. Check Boot scene hierarchy.
- Screen flicker without fade → UnityTransitionPlayer not found or CanvasGroup alpha not animating. Check TransitionOverlay > CanvasGroup component.
- Input stays blocked after popup dismiss → UnityInputBlocker reference-counting imbalance. Check PopupManager.DismissAllAsync Unblock() call pattern (Decision #13).
- Pink / missing meshes on UI → Canvas components not created correctly. Rebuild scenes via SceneSetup.
- "Compilation failed" in Console → Check for missing asmdef references. Verify SimpleGame.Editor.asmdef references SimpleGame.Runtime.

## Requirements Proved By This UAT

- R001 — MVP pattern with strict separation: Runtime proof — view MonoBehaviours dispatch events, plain C# presenters respond, GameService used as domain layer, no layer cross-references.
- R002 — View independence: View MonoBehaviours work in isolation (no backward refs); interacting with buttons fires only `event Action`, not direct presenter calls.
- R004 — Central UI factory: GameBootstrapper is the only place that calls `_uiFactory.CreateXxxPresenter()`. All 3 presenter types constructed through the single factory.
- R008 — Boot scene initialization: Boot.unity at index 0; GameBootstrapper initializes full dependency chain before first screen is shown.
- R009 — Hybrid scene management: Boot scene persists; MainMenu and Settings are additively loaded/unloaded by ScreenManager on each navigation.
- R010 — Screen navigation: MainMenu → Settings → MainMenu round-trip verified with fade transitions.
- R011 — Stack-based popup system: ConfirmDialog popup opens over MainMenu, blocks input, and dismisses cleanly.
- R013 — Fade transitions: Fade-to-black transition plays on every screen navigation with input blocked during transition.
- R016 — Demo screens end-to-end: Full dependency chain from boot to presenter to view, including popup, verified in play mode.

## Not Proven By This UAT

- Deep popup stacking (2+ popups simultaneously) — proven by PopupManagerTests edit-mode tests (S03), not runtime UAT.
- Play-mode test automation — deferred (R019). All view testing in M001 is manual.
- Domain reload disabled mode behavior — partially proven by re-enter play mode edge case above, but not automated.
- Error recovery paths (e.g. scene load failure, missing scenes) — not covered; out of scope for M001.

## Notes for Tester

- The fade transition is intentionally brief (~0.3s default). If it appears to not play, slow it down by increasing the fade duration in the TransitionOverlay's UnityTransitionPlayer component in the Boot scene Inspector.
- The "Open Popup" button and ConfirmDialog's "OK"/"Cancel" buttons may have generic labels if SceneSetup used default text. The important thing is that clicking "OK"/"Cancel" dismisses the popup — exact label text is cosmetic.
- If the Game view shows two overlapping MainMenu canvases, it means the previous scene wasn't unloaded before the new one loaded — check ScreenManager's `ShowScreenAsync` for the unload-before-load sequence (proven by `ShowScreenAsync_UnloadsPreviousBeforeLoadingNext` test in S02).
- After a successful walkthrough, you can confirm zero static state by exiting and re-entering play mode — the entire boot sequence must replay cleanly with no residual state from the first session.
