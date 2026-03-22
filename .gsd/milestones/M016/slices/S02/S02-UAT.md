# S02 UAT — Cloud Save Sync

**Slice:** S02 — Cloud Save Sync
**Milestone:** M016

These tests can be run whenever convenient. They do not block the next slice.

**Prerequisite:** S01 UAT passed (PlayFab Title ID configured, anonymous login working).

---

## Test 1: Cloud Push on Session End

1. Open Unity Play mode
2. Play a puzzle to completion (level complete popup)
3. Click through the level complete popup to return to main menu
4. In PlayFab Game Manager → Players → select the test player → Player Data
5. **Expected:** A key `MetaSave` exists with a JSON value containing current coins, goldenPieces, and objectProgress

**Pass condition:** `MetaSave` key exists and JSON is parseable.

---

## Test 2: Cloud Pull on Boot (Simulated Reinstall)

1. In Unity: call `PlayerPrefs.DeleteAll()` from a debug menu or a temporary test script
2. Enter Play mode (simulates fresh install)
3. Watch the Console for `[CloudSave] Pull succeeded`

**Expected:** Progress from the previous session (Test 1) is restored. Coin balance and object steps match the cloud values.

**Pass condition:** Log shows pull succeeded; local state matches cloud data.

---

## Test 3: Take-Max Merge — Cloud Wins

1. Manually edit the `MetaSave` JSON in PlayFab Game Manager to have `coins: 9999`
2. Ensure local PlayerPrefs has lower coin value
3. Enter Play mode

**Expected:** After boot, the in-game coin display shows 9999 (cloud value wins).

**Pass condition:** Merged value reflects the higher cloud value.

---

## Test 4: Push on App Pause

1. Enter Play mode
2. Pause the Editor (simulates app backgrounding)
3. Check PlayFab Game Manager → Player Data after a few seconds

**Expected:** `MetaSave` key is updated with the latest save state.

**Pass condition:** `MetaSave` timestamp (savedAt field) is recent.
