# S04: Final Verification — Compile, Tests, Orphan Cleanup — UAT

**Milestone:** M020
**Written:** 2026-03-26

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S04 produced no user-visible behaviour. All verification was structural — file counts, orphan scans, test runs, and git state. The artifact record from T01 is the complete proof. No live runtime or human UX verification is applicable.

## Preconditions

- Unity Editor is open with the project loaded (for scene/console verification)
- `mcporter` is available in PATH
- Unity MCP server is responsive (`mcporter call unityMCP.read_console --output raw --args '{"count":5}'` returns recent console output, not a ping-timeout)
- Working tree is on branch `milestone/M020` (or the merge commit is complete)

## Smoke Test

Run the file-structure check to confirm the restructure landed:

```bash
ls Assets/Scripts/Game/Services 2>&1  # should print "No such file or directory"
find Assets/Scripts/Game/Popup -name "*.cs"  # should print exactly one line: .../UnityViewContainer.cs
find Assets/Scripts/Game/IAP -name "*.cs" | wc -l  # should print 15
```

All three return the expected values → the structural core of M020 is present.

## Test Cases

### 1. Services/ directory is gone

1. Run: `ls Assets/Scripts/Game/Services`
2. **Expected:** exit code 1, output contains "No such file or directory"

### 2. Popup/ contains exactly one .cs file

1. Run: `find Assets/Scripts/Game/Popup -name "*.cs"`
2. **Expected:** exactly one line of output: `Assets/Scripts/Game/Popup/UnityViewContainer.cs`

### 3. All feature folder file counts match manifest

For each folder, run `find Assets/Scripts/Game/<folder> -name "*.cs" | wc -l`:

| Folder | Expected count |
|--------|---------------|
| IAP/ | 15 |
| Ads/ | 7 |
| ATT/ | 7 |
| Economy/ | 6 |
| Save/ | 4 |
| Progression/ | 4 |
| PlayFab/ | 17 |
| Shop/ | 3 |
| LevelFlow/ | 7 |
| ConfirmDialog/ | 3 |
| Meta/ | 7 |

**Expected:** Each folder returns the count shown. PlayFab/ is 17 (plan said 16 — one-off manifest error; 17 is correct).

### 4. Zero orphaned .meta files

1. Run:
```python
python3 -c "import os,glob; base='Assets/Scripts/Game'; orphans=[m for m in glob.glob(base+'/**/*.meta',recursive=True) if not os.path.exists(m[:-5])]; print('No orphaned .meta files found.' if not orphans else '\n'.join(orphans))"
```
2. **Expected:** output is exactly `No orphaned .meta files found.`

### 5. 347 EditMode tests pass

1. Run: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`
2. Note the `job_id` in the response.
3. Poll: `mcporter call unityMCP.get_test_job job_id=<id>` until `status == "succeeded"`
4. **Expected:** `result.summary.passed == 347`, `result.summary.failed == 0`, `result.summary.skipped == 0`

### 6. Working tree is clean

1. Run: `git status --short`
2. **Expected:** empty output (no uncommitted changes, no untracked files)

## Edge Cases

### Orphan from a missed empty-folder cleanup

1. Run: `git status --short | grep "\.meta"`
2. **Expected:** No `.meta` files appear as untracked or modified. If any appear, they are orphans from emptied folders that were not removed per K008. Fix: `git rm <path>.meta` and commit.

### Compile errors hidden in Editor.log

1. Run:
```python
python3 -c "
log = open('C:/Users/Daniel/AppData/Local/Unity/Editor/Editor.log', errors='replace').read()
last = log.split('Starting: ')[-1]
errors = [l for l in last.split('\n') if 'error CS' in l]
print('\n'.join(errors) if errors else 'No errors in last compile run')
"
```
2. **Expected:** `No errors in last compile run`. If any `error CS` lines appear, the restructure broke a namespace or using-statement reference.

### PlayFab/ count comes back as 16

If the count is 16, `SingularService.cs` is missing. Verify:
1. Run: `find Assets/Scripts/Game/PlayFab -name "SingularService.cs"`
2. **Expected:** `Assets/Scripts/Game/PlayFab/SingularService.cs` (file exists)
3. If missing, check git log for where it was moved or deleted — it should be in PlayFab/ per the S02 boundary map.

## Failure Signals

- `ls Assets/Scripts/Game/Services` exits 0 (directory still exists) → S01 cleanup did not complete; check `git log --oneline` for the S01 commit
- `find .../Popup -name "*.cs" | wc -l` returns more than 1 → popup feature files were not moved in S03
- Any folder count is lower than expected → a file was missed in one of S01–S03; check `git log --diff-filter=R --summary` for renames
- Orphan scan prints paths → empty-folder `.meta` cleanup was skipped; run `git rm <paths>` and commit
- Test run shows failures → a `using` statement or namespace reference broke during moves; check `result.failures_so_far` for test names, then cross-reference with recent `git log` to identify which slice introduced the regression
- `git status --short` shows untracked `.meta` files → same orphan issue as above

## Requirements Proved By This UAT

- None directly — M020 is a structural refactor with no capability changes. The test pass (347/347) confirms no existing capability was broken.

## Not Proven By This UAT

- Missing-script warnings in Unity scenes (requires visual inspection of the Unity Editor Console after opening each scene; not covered by automated checks above)
- Runtime behaviour of any feature — this UAT proves structural integrity only

## Notes for Tester

The PlayFab/ count of 17 is correct — the plan manifest was off by one (`SingularService.cs` pre-existed M020). Do not treat 17 as a failure.

The test count of 347 is the new baseline. If a future slice expects 340, that number is stale.

For the missing-script check: open the Unity Editor, load each of the four scenes (Boot, MainMenu, InGame, Settings), and check the Console for yellow or red missing-script warnings. This is the only check that cannot be done from the shell. It is low-risk given that all moves used `git mv` to preserve GUIDs.
