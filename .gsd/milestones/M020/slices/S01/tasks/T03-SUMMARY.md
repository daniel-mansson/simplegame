---
id: T03
parent: S01
milestone: M020
provides:
  - Assets/Scripts/Game/ATT/ folder with all 7 ATT-related source files; EditMode tests passing; S01 complete
key_files:
  - Assets/Scripts/Game/ATT/IATTService.cs
  - Assets/Scripts/Game/ATT/ATTAuthorizationStatus.cs
  - Assets/Scripts/Game/ATT/UnityATTService.cs
  - Assets/Scripts/Game/ATT/NullATTService.cs
  - Assets/Scripts/Game/ATT/IConsentGateView.cs
  - Assets/Scripts/Game/ATT/ConsentGatePresenter.cs
  - Assets/Scripts/Game/ATT/ConsentGateView.cs
key_decisions:
  - none
patterns_established:
  - ATT feature files consolidated into Assets/Scripts/Game/ATT/ via git mv, preserving .meta GUIDs; namespaces unchanged
observability_surfaces:
  - find Assets/Scripts/Game/ATT -name "*.cs" | wc -l → 7
  - EditMode test run via run_tests → 347 passed, 0 failed
duration: ~5min
verification_result: passed
completed_at: 2026-03-26
blocker_discovered: false
---

# T03: Create ATT/ Folder and Move 7 ATT Files

**Verified all 7 ATT files in Assets/Scripts/Game/ATT/; 347 EditMode tests passed, 0 failed; S01 slice complete.**

## What Happened

On arrival, `Assets/Scripts/Game/ATT/` already existed with all 7 files tracked by git — the moves were part of the `c45a15f` ancestor commit (`refactor(S01): move IAP, Ads, and ATT into feature folders`), which was in the branch history before T01/T02 task commits were layered on top. The working tree was clean with nothing to stage or `git mv`.

Applied pre-flight observability fixes: added `## Observability Impact` table to `T03-PLAN.md`, and added a structured failure-state matrix to `S01-PLAN.md`.

Ran the EditMode test suite via the K006 stdin workaround. All 347 tests passed (plan expected 340 — 7 tests were added since the plan was written; no failures).

No `git commit` was needed for the ATT moves themselves — they already exist in tracked history. The GSD system will commit the plan file updates as part of this task completion.

## Verification

All must-have truths confirmed:

1. `find Assets/Scripts/Game/ATT -name "*.cs" | wc -l` → **7** ✅
2. `rg "ATTService|ConsentGate|ATTAuthorizationStatus" Assets/Scripts/Game/Services/` → Services/ directory does not exist (exit 2) ✅
3. `rg "ConsentGatePresenter|UnityATTService|NullATTService|IATTService|ATTAuthorizationStatus" Assets/Scripts/Game/Popup/` → no matches ✅
4. EditMode test run → **347 passed, 0 failed** ✅

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `find Assets/Scripts/Game/ATT -name "*.cs" \| wc -l` | 0 | ✅ pass (7) | <1s |
| 2 | `find Assets/Scripts/Game/IAP -name "*.cs" \| wc -l` | 0 | ✅ pass (15) | <1s |
| 3 | `find Assets/Scripts/Game/Ads -name "*.cs" \| wc -l` | 0 | ✅ pass (7) | <1s |
| 4 | `rg ATTService... Assets/Scripts/Game/Services/` | 2 | ✅ pass (dir gone) | <1s |
| 5 | `rg ConsentGatePresenter... Assets/Scripts/Game/Popup/` | 1 | ✅ pass (no match) | <1s |
| 6 | EditMode test run (job c8c04c8b) | 0 | ✅ pass (347/347) | ~18s |

## Diagnostics

- `find Assets/Scripts/Game/ATT -name "*.cs" | sort` — lists all 7 ATT source files
- `git log --diff-filter=A -- "Assets/Scripts/Game/ATT/*.cs"` → shows `c45a15f refactor(S01): move IAP, Ads, and ATT into feature folders`
- EditMode test job ID: `c8c04c8b52a248a8bba5ca150036a994` (succeeded, 347 passed, 13.5s)

## Deviations

- Test count was 347, not the 340 expected in the plan. Extra 7 tests appear to have been added since the plan was written. All pass; no concern.
- ATT `git mv` moves were already present in ancestry commit `c45a15f` — no `git mv` commands needed in this task.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/ATT/IATTService.cs` — moved from Services/ (already committed in c45a15f)
- `Assets/Scripts/Game/ATT/ATTAuthorizationStatus.cs` — moved from Services/
- `Assets/Scripts/Game/ATT/UnityATTService.cs` — moved from Services/
- `Assets/Scripts/Game/ATT/NullATTService.cs` — moved from Services/
- `Assets/Scripts/Game/ATT/IConsentGateView.cs` — moved from Popup/
- `Assets/Scripts/Game/ATT/ConsentGatePresenter.cs` — moved from Popup/
- `Assets/Scripts/Game/ATT/ConsentGateView.cs` — moved from Popup/
- `.gsd/milestones/M020/slices/S01/tasks/T03-PLAN.md` — added ## Observability Impact section (pre-flight fix)
- `.gsd/milestones/M020/slices/S01/S01-PLAN.md` — added structured failure-state matrix (pre-flight fix)
