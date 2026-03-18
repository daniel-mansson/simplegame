# S01: Animation Plumbing — UAT

## What to Test

Open the game in Play mode and observe popup open/close behavior.

## Test Steps

1. **Start play mode** in the Unity Editor from the Boot scene
2. **Navigate to InGame** — click Play from MainMenu
3. **Trigger any popup** — e.g. place a correct piece to win, or click Quit-equivalent if exposed
4. **Observe open:** dim overlay should fade in (semi-transparent dark), popup should slide up from slightly below and settle with a slight bounce
5. **Observe close:** popup should scale down and fade out; the dim overlay should fade simultaneously; immediately after clicking the close button you should be able to interact with the UI underneath (don't wait for animation to finish)
6. **Repeat for at least 2 popup types** (e.g. LevelComplete and LevelFailed)

## Expected Results

- Dim overlay is visible during popup open (semi-transparent dark backdrop)
- Popup bounces up on enter (not an instant appear)
- Input is unblocked immediately on dismiss (can tap UI before fade-out animation ends)
- PopupViewBase fields null-warning logs ARE expected until S03 wires `_canvasGroup` and `_panel`
- No crashes or NullReferenceExceptions (only the intentional "animation skipped" warnings)

## Known Incomplete

- Animations will log warnings and skip (refs not yet wired) — this is expected until S03
- Blocker alpha fade will work (UnityInputBlocker refs are wired via SceneSetup)
