---
id: SR01
parent: M019
milestone: M019
uat_type: live-runtime
written: 2026-03-25
---

# SR01: Restore MockIAPService in Editor Runtime — UAT

**Milestone:** M019
**Written:** 2026-03-25

## UAT Type

- UAT mode: live-runtime
- Why this mode is sufficient: The only change is which service is constructed at boot. The observable signal is the Unity Console log and the runtime behavior of the mock — both are checkable in a Play mode session inside the Editor. No device required.

## Preconditions

1. Unity Editor open with the project loaded in the `M019` worktree
2. Boot scene is active (the scene loaded at index 0)
3. `Assets/Resources/IAPMockConfig.asset` exists (created by S01; should already be present)
4. `Assets/Resources/IAPProductCatalog.asset` exists with at least one product defined
5. No compile errors in the Unity Console

## Smoke Test

Enter Play mode. In the Unity Console, filter by `[GameBootstrapper]`. Confirm you see:

```
[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.
```

If this line appears, SR01's primary goal is met — the Editor is using MockIAPService.

---

## Test Cases

### 1. Mock service is active in Editor Play mode

1. Open `Assets/Resources/IAPMockConfig.asset` in the Inspector.
2. Confirm `MockOutcome` is set to `Success`.
3. Enter Play mode.
4. Filter the Unity Console by `[GameBootstrapper]`.
5. **Expected:** Log line `[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.` is present. No `UnityIAPService (device)` log is visible.

---

### 2. Success outcome — coin pack purchase completes and grants coins

1. In `IAPMockConfig.asset`, set `MockOutcome` to `Success`.
2. Enter Play mode.
3. Navigate to the Shop (MainMenu → Shop screen).
4. Tap any coin pack.
5. **Expected:** Purchase completes, coins are added to the player's coin balance (visible in the HUD or balance display). No error or "purchase failed" message is shown.

---

### 3. Cancelled outcome — purchase returns without granting coins

1. In `IAPMockConfig.asset`, set `MockOutcome` to `Cancelled`.
2. Enter Play mode.
3. Navigate to the Shop and tap any coin pack.
4. **Expected:** Purchase flow returns with a cancelled/no-action result. Coin balance is unchanged. No coins are granted, no error message is shown (cancellation is not an error).

---

### 4. PaymentFailed outcome — purchase fails with error messaging

1. In `IAPMockConfig.asset`, set `MockOutcome` to `PaymentFailed`.
2. Enter Play mode.
3. Navigate to the Shop and tap any coin pack.
4. **Expected:** Purchase fails. An error/failure message is shown in the Shop UI (e.g., "Purchase failed"). No coins are granted. Coin balance is unchanged.

---

### 5. ValidationFailed outcome — PlayFab validation failure path

1. In `IAPMockConfig.asset`, set `MockOutcome` to `ValidationFailed`.
2. Enter Play mode.
3. Navigate to the Shop and tap any coin pack.
4. **Expected:** Purchase fails at the validation step. An error/failure message is shown. No coins are granted. This exercises the code path that would correspond to a PlayFab receipt rejection on device.

---

### 6. IAPPurchase popup — mock outcomes work from popup entry point

1. In `IAPMockConfig.asset`, set `MockOutcome` to `Success`.
2. Enter Play mode.
3. Navigate to the InGame scene and lose a level (or trigger the LevelFailed popup directly).
4. In the LevelFailed popup, tap the IAP purchase option (if present) or navigate to the IAPPurchase popup via any other entry point.
5. **Expected:** Purchase completes, coins are granted (not golden pieces). Coin balance increases.
6. Repeat with `MockOutcome` set to `Cancelled` — **Expected:** no coins granted, popup dismisses cleanly.

---

### 7. Outcome change is hot — changing IAPMockConfig before entering Play mode takes effect

1. In `IAPMockConfig.asset`, set `MockOutcome` to `PaymentFailed`.
2. Enter Play mode, trigger a shop purchase — confirm failure.
3. Exit Play mode.
4. Change `MockOutcome` to `Success`.
5. Enter Play mode again, trigger a shop purchase.
6. **Expected:** Purchase succeeds in the second run. The mock reads the ScriptableObject at boot, so changing the value before entering Play mode takes effect immediately.

---

## Edge Cases

### IAPMockConfig.asset missing from Resources

1. Temporarily rename `Assets/Resources/IAPMockConfig.asset` to `IAPMockConfig_backup.asset`.
2. Enter Play mode.
3. **Expected:** Boot proceeds without a hard error. Shop purchases default to Success (null config = Success behavior in MockIAPService). Unity Console may not show a warning for the missing asset (this is intentional per the slice plan). No crash.
4. Restore the asset name before continuing other tests.

---

### UnityIAPService is NOT active in the Editor

1. In the Unity Console (in Play mode), search for `[GameBootstrapper] IAP: UnityIAPService`.
2. **Expected:** Zero matches. UnityIAPService is only active in non-Editor builds. Seeing this log in the Editor means the `#if UNITY_EDITOR` guard was compiled out — which would be a regression.

---

### Code guard grep verification (static check, no Play mode needed)

Run from the project root (or in a terminal pointed at the repo):

```bash
rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs
```
**Expected:** 2 matches, both clearly inside the `#if UNITY_EDITOR` block.

```bash
rg "MOCK_IAP" Assets/Scripts/
```
**Expected:** Exit code 1, zero matches. Any match means a stale reference was not removed.

```bash
rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs
```
**Expected:** 2 matches, both inside the `#else` block (lines between `#else` and `#endif`).

---

## Failure Signals

- Console shows `[GameBootstrapper] IAP: UnityIAPService (device).` in the Editor → `#if UNITY_EDITOR` guard is missing or was compiled out
- Shop purchase always succeeds regardless of `MockOutcome` setting → MockIAPService is not reading the config, or `IAPMockConfig.asset` is missing
- Shop purchase always fails or throws a NullReferenceException → possible null `_iapCatalog` or missing product definitions
- `rg "MOCK_IAP" Assets/Scripts/` returns any match → stale comment was not fully removed (regression)
- Unity Console shows compile errors referencing `MockIAPService` in `#else` block → guard placement is incorrect

---

## Requirements Proved By This UAT

- **R166** — Editor mock IAP with selectable outcomes: UAT cases 1–5 exercise all four outcomes (Success, Cancelled, PaymentFailed, ValidationFailed) from the Shop panel. Case 7 proves the ScriptableObject-driven configuration works at boot time. The static grep checks confirm the `#if UNITY_EDITOR` guard is structurally correct.

---

## Not Proven By This UAT

- Device behavior (UnityIAPService + PlayFab path) — the `#else` branch is not exercised in the Editor by definition. Device behavior was proven by S02 and S03.
- Real money flows or sandbox receipts — mock path only; no actual store communication occurs.
- IAPMockConfig.asset null warning UX — per slice plan, null config silently defaults to Success with no warning. This is documented as intentional behavior, not tested as a correctness requirement.

---

## Notes for Tester

- **Change `MockOutcome` before entering Play mode**, not during. The ScriptableObject is read at boot (during `GameBootstrapper.Start()`), so mid-session changes won't take effect until the next Play mode session.
- The IAPMockConfig.asset Inspector field uses an enum dropdown — all four values (Success, Cancelled, PaymentFailed, ValidationFailed) are selectable without typing.
- The coin balance HUD should be visible in the Shop screen. If the balance display is not visible, check the currency overlay is enabled for that context.
- If entering Play mode fails with compile errors, check K011 (Bee pipeline cache staleness) before investigating the IAP guard change.
