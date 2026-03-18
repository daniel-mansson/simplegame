# S04: Integration Verification — UAT

## What to Test

Full integration verification — open and close multiple popups in play mode and confirm all visual and input behaviors work end-to-end.

## Prerequisites

- Unity Editor open with Boot scene
- Game not in play mode

## Test Steps

### 1. Blocker overlay fade
1. Enter Play mode
2. Navigate MainMenu → click Play → InGame loads
3. Trigger a popup (e.g. win a level to open LevelComplete)
4. **Observe:** A semi-transparent dark overlay fades in behind the popup (should see it appear before popup fully bounces up)
5. Click Continue / dismiss the popup
6. **Observe:** Overlay starts fading out; you can immediately click other UI elements (don't wait for fade to complete)

### 2. Popup animate-in
1. Open any popup
2. **Observe:** Popup panel slides in from ~80px below its final position with a slight bounce (OutBounce ease)
3. Alpha should be 1 throughout (no fade on popup enter, only overlay fades)

### 3. Popup animate-out
1. Dismiss any popup
2. **Observe:** Popup scales down slightly (0.85x) and fades to transparent simultaneously
3. **Observe:** Popup disappears (SetActive false) after animation completes

### 4. Input timing on dismiss
1. Open a popup
2. Click dismiss button
3. **Immediately** click any button behind the popup (before animation finishes)
4. **Observe:** The click registers — input is unblocked at dismiss start, not animation end

### 5. Repeat for at least 2 popup types

## Expected Results

| Behavior | Expected |
|---|---|
| Overlay on open | Fades in (alpha 0 → 0.5) |
| Overlay on close | Fades out (alpha 0.5 → 0), starts immediately on dismiss |
| Input on close | Unblocked immediately when dismiss starts |
| Popup enter | Slides up from below with bounce |
| Popup exit | Scales down + fades out |
| Boot scene | No NullReferenceExceptions in console |

## Known Limitations

- Visual polish (actual colors, sizes, fonts) is deferred — placeholder layout only
- Per-popup custom animation overrides not implemented this milestone (infrastructure is ready)
