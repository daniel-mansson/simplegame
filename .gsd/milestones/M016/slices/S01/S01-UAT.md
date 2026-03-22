# S01 UAT — SDK + Anonymous Login

**Slice:** S01 — SDK + Anonymous Login
**Milestone:** M016

These tests can be run whenever convenient. They do not block the next slice.

## Prerequisites

- Unity Editor open with the simplegame project loaded
- A PlayFab account and Title created at https://developer.playfab.com
- Title ID configured: in Unity, navigate to `Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings` asset and enter your Title ID in the Inspector

---

## Test 1: First Launch — Anonymous Account Created

1. Open Unity Editor and enter Play mode
2. Watch the Console
3. **Expected:** Log line `[PlayFabAuth] Logged in. PlayFabId: XXXXXXXXX` appears within a few seconds of boot
4. Note the PlayFab ID shown

**Pass condition:** A non-empty PlayFab ID is logged. No error lines prefixed with `[PlayFabAuth]`.

---

## Test 2: Second Launch — Same Account Recovered

1. Exit Play mode
2. Enter Play mode again
3. Watch the Console

**Pass condition:** The same PlayFab ID from Test 1 appears in the log. The ID does not change between sessions.

---

## Test 3: Verify Account in PlayFab Game Manager

1. Log into https://developer.playfab.com
2. Navigate to your title → Players
3. **Expected:** At least one player entry exists with a recent "Last Login" timestamp

**Pass condition:** A player record is visible in Game Manager.

---

## Test 4: Offline Mode Graceful Degradation

1. Disconnect from the internet (disable WiFi/ethernet)
2. Enter Play mode
3. Watch the Console

**Pass condition:** Log line `[GameBootstrapper] PlayFab login failed — continuing offline` (or similar warning) appears. The game continues to the main menu without crashing.
