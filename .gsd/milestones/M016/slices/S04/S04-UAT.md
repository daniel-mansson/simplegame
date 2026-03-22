# S04 UAT — Analytics Events

**Slice:** S04 — Analytics Events
**Milestone:** M016

**Prerequisite:** S01 UAT passed (PlayFab login working, Title ID configured).

---

## Test 1: Session Events in PlayFab Game Manager

1. Enter Unity Play mode (ensures session_start fires)
2. Wait 5–10 seconds
3. Exit Play mode (triggers session_end)
4. Open PlayFab Game Manager → Analytics → Event Explorer
5. Search for events from the test player

**Expected:** `session_start` and `session_end` events appear in the event list with recent timestamps.

**Pass condition:** Both events visible in Event Explorer.

---

## Test 2: Level Events

1. Enter Play mode and start a puzzle
2. Complete or fail the puzzle
3. Check PlayFab Event Explorer

**Expected:** `level_started` event with the current level ID. Either `level_completed` or `level_failed` depending on outcome.

**Pass condition:** Level events appear with correct level_id property.

---

## Test 3: Currency Events

1. Enter Play mode and complete a level (earns golden pieces)
2. Navigate to a screen that allows spending coins
3. Check PlayFab Event Explorer

**Expected:** `currency_earned` event for golden_pieces with amount=5 (or configured value). `currency_spent` if coins were spent.

**Pass condition:** Currency events appear with correct currency and amount properties.

---

## Test 4: Platform Account Linked Event

1. Open Settings and link Game Center (iOS device required) or manually call link in editor if mocked
2. Check PlayFab Event Explorer

**Expected:** `platform_account_linked` event appears with platform property set to "GameCenter" or "GooglePlayGames".

**Pass condition:** Event appears with correct platform property.
