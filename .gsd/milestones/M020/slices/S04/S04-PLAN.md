# S04: Final Verification — Compile, Tests, Orphan Cleanup

**Goal:** Confirm clean project state: no orphaned `.meta` files, all target folder file counts match manifest, 340 tests pass, no missing-script warnings in scenes.

**Demo:** `rg -l "." Assets/Scripts/Game/Services/` returns nothing; `find Assets/Scripts/Game/Popup -name "*.cs" | wc -l` returns 1; 340 tests confirmed; Unity opens clean.

## Must-Haves

- Zero `.meta` files pointing to removed directories
- All expected feature folders exist with correct file counts (verified against manifest)
- 340 EditMode tests pass (test count may be higher if tests were added during S01–S03)
- No missing-script warnings in Unity Console

## Tasks

- [x] **T01: Audit file counts, check for orphaned .meta files, run final test pass, commit state**

  **Why:** Acts as the explicit close-gate for M020. All structural invariants from the restructure (S01–S03) must be confirmed here — if any slip through (orphaned .meta, wrong file count, failing test), this is the safety net.

  **Files:** `Assets/Scripts/Game/**/*.cs`, `Assets/Scripts/Game/**/*.meta` (read-only audit). Orphan `.meta` files removed if found.

  **Do:**
  1. Run count checks for all feature folders against the plan manifest
  2. Python orphan scan: every `.meta` must have a matching `.cs` or directory
  3. `git rm` any orphans and note them
  4. Run EditMode tests via K006 stdin method
  5. Confirm ≥340 pass, 0 fail

  **Verify:** All count checks ✅; orphan scan shows 0 orphans; test run shows 0 failures.

## Files Likely Touched

- None — verification only; any cleanup is fixing previous slice gaps

## Observability / Diagnostics

- **Test results:** `mcporter call unityMCP.get_test_job job_id=<id>` — `result.summary.passed/failed` are the authoritative signals
- **Orphan scan:** Python script emits `No orphaned .meta files found.` or a list of paths — surface is stdout, no log file
- **File counts:** `find Assets/Scripts/Game/<folder> -name "*.cs" | wc -l` — run per-folder; deviations from manifest indicate a missed move in S01–S03
- **Failure state:** If tests fail, `result.failures_so_far[]` in the job response names the failing tests. Cross-reference with recent git changes to identify which slice introduced the regression.
- **Compile errors:** Check `Editor.log` using the K011 last-compile-run extraction — only errors *after the last `Starting:` line* are current
- **Redaction:** No secrets or credentials are involved; all signals are safe to log

## Verification

1. `ls Assets/Scripts/Game/Services` → no such directory (Services/ was removed in S02)
2. `find Assets/Scripts/Game/Popup -name "*.cs" | wc -l` → 1
3. Per-folder counts match manifest (IAP=15, Ads=7, ATT=7, Economy=6, Save=4, Progression=4, PlayFab≥16, Shop=3, LevelFlow=7, ConfirmDialog=3, Meta≥7)
4. Python orphan scan → 0 orphans
5. EditMode test run → ≥340 passed, 0 failed
6. `git status` → clean working tree (no uncommitted changes)

## Notes

If S01–S03 were clean, this slice is just a pass/fail check. The value is having an explicit final gate that forces honest verification before marking the milestone complete.
