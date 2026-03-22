# S03 UAT — Platform Linking & First-Launch Prompt

**Slice:** S03 — Platform Linking & First-Launch Prompt
**Milestone:** M016

These tests can be run whenever convenient. Device tests require a physical device.

---

## Test 1: Settings Screen Shows Link Status

1. Open Unity Play mode
2. Navigate to Settings
3. **Expected:** Settings screen shows "Game Center: Not Linked" and "Google Play: Not Linked" (or linked status if already linked)

**Pass condition:** Status text reflects actual link state.

---

## Test 2: First-Launch Prompt Appears Once

1. Delete PlayerPrefs key `PlayFab_HasSeenLinkPrompt` (or clear all PlayerPrefs)
2. Enter Play mode
3. **Expected:** After PlayFab login, a platform link popup appears before the main menu

**Pass condition:** Popup is shown. Pressing "Skip" dismisses it and the player reaches the main menu.

---

## Test 3: First-Launch Prompt Not Shown Again After Skip

1. Complete Test 2 (skip the prompt)
2. Exit and re-enter Play mode
3. **Expected:** No link prompt appears — the player goes directly to the main menu

**Pass condition:** No popup on second launch.

---

## Test 4: Game Center Linking on iOS Device (device required)

1. Build for iOS and install on device
2. Ensure Game Center is signed in (Settings → Game Center on device)
3. Navigate to Settings in the game
4. Tap "Link Game Center"
5. **Expected:** Status changes to "Game Center: Linked"
6. Open PlayFab Game Manager → Players → select player → Linked Accounts
7. **Expected:** Game Center account appears in the linked accounts list

**Pass condition:** Link status updates in-game and confirmed in PlayFab dashboard.

---

## Test 5: Unlink Game Center on iOS Device

1. After Test 4, tap "Unlink Game Center" in Settings
2. **Expected:** Status reverts to "Game Center: Not Linked"

**Pass condition:** Unlink confirmed in-game.
