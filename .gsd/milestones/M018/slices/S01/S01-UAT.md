# S01: Consent Gate Popup — UAT

**When to run:** After creating the `ConsentGatePopup` prefab in the Unity Editor and running `Tools/Setup/Create And Register Scenes`.

## Pre-conditions
- `ConsentGatePopup` prefab created in `Assets/Prefabs/Game/Popups/` with:
  - `ConsentGateView` component attached
  - `_acceptButton` wired to a Button
  - `_tosLinkButton` wired to a Button labeled "Terms of Service"
  - `_privacyLinkButton` wired to a Button labeled "Privacy Policy"
  - `_canvasGroup` and `_panel` wired (for PopupViewBase animations)
- `Tools/Setup/Create And Register Scenes` run after prefab creation
- PlayerPrefs cleared (`PlayerPrefs.DeleteAll()` or fresh install simulation)

## Test Script

### T1 — First launch shows consent popup
1. Clear PlayerPrefs (Edit → Clear All PlayerPrefs, or delete the key `ConsentGate_Accepted`)
2. Enter Play mode
3. **Expected:** Consent popup appears blocking the screen. No X button, no close gesture.
4. **Expected:** There is exactly one actionable button labeled "Accept" (plus ToS and Privacy links)

### T2 — ToS link opens browser
1. With consent popup visible, tap "Terms of Service"
2. **Expected:** Device browser opens to `https://simplemagicstudios.com/play`

### T3 — Privacy Policy link opens browser
1. With consent popup visible, tap "Privacy Policy"
2. **Expected:** Device browser opens to `https://simplemagicstudios.com/play`

### T4 — Accept proceeds to main menu
1. With consent popup visible, tap "Accept"
2. **Expected:** Popup animates out, main menu loads normally

### T5 — Second launch skips consent popup
1. After T4 completed (flag set), exit Play mode and re-enter Play mode
2. **Expected:** No consent popup — goes straight to main menu

### T6 — Consent persists across Editor restarts
1. Close and reopen Unity
2. Enter Play mode
3. **Expected:** No consent popup (PlayerPrefs key persists)
