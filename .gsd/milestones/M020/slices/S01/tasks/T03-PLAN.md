# T03: Create ATT/ Folder, Move 7 ATT Files, Run Tests, Commit

**Slice:** S01
**Milestone:** M020

## Goal

Move ATT-related source files into `Assets/Scripts/Game/ATT/`, run EditMode tests, then commit the full S01 batch (IAP + Ads + ATT moves together).

## Must-Haves

### Artifacts
- `Assets/Scripts/Game/ATT/` exists with these 7 files:
  - From `Services/`: `IATTService.cs`, `ATTAuthorizationStatus.cs`, `UnityATTService.cs`, `NullATTService.cs`
  - From `Popup/`: `IConsentGateView.cs`, `ConsentGatePresenter.cs`, `ConsentGateView.cs`

### Truths
- `find Assets/Scripts/Game/ATT -name "*.cs" | wc -l` returns 7
- `rg "ATTService\|ConsentGate\|ATTAuthorization" Assets/Scripts/Game/Services/ Assets/Scripts/Game/Popup/` returns no matches
- EditMode test run: 340 passed, 0 failed

## Steps

1. `git mv` the 4 ATT service files from `Services/` to `ATT/`
2. `git mv` the 3 ConsentGate popup files from `Popup/` to `ATT/`
3. `git status` — confirm all 29 renames staged (15 IAP + 7 Ads + 7 ATT)
4. Run EditMode tests via Unity MCP (K006 stdin workaround)
5. Verify 340 pass, 0 fail
6. `git add -A && git commit -m "refactor(S01): move IAP, Ads, and ATT into feature folders"`

## Context

- This commit covers all T01+T02+T03 moves in one atomic commit — cleaner than 3 separate commits for what is logically one slice of work
- K006: use `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` for test run; poll with `mcporter call unityMCP.get_test_job job_id=<id>`
- Unity must have focus for recompilation to trigger before test run — if MCP shows stale errors, trigger a refresh via Assets/Refresh menu item

## Observability Impact

This task is a pure filesystem reorganisation. No runtime behaviour changes.

**Inspectable signals after this task:**

| Signal | How to check | Expected value |
|---|---|---|
| ATT folder exists | `find Assets/Scripts/Game/ATT -name "*.cs" \| sort` | 7 files listed |
| ATT files gone from Services/ | `ls Assets/Scripts/Game/Services/ 2>&1` | directory does not exist (or no ATT files) |
| ATT files gone from Popup/ | `rg "ConsentGatePresenter\|UnityATTService\|NullATTService\|IATTService\|ATTAuthorizationStatus" Assets/Scripts/Game/Popup/` | no matches |
| Git tracks renames (not copy+delete) | `git log --diff-filter=R --summary HEAD~3..HEAD \| grep "rename.*ATT"` | 14 rename lines (7 .cs + 7 .meta) |
| Unity compile clean | K011 python3 snippet on Editor.log after last `Starting:` line | zero `error CS` lines |
| EditMode tests | `get_test_job` result | 340 passed, 0 failed |

**Failure state detection:**

- If a `git mv` was skipped, `git status` shows the old file as deleted and the new path as untracked. Fix: re-run the specific `git mv` for the missing pair.
- If Unity shows `error CS0246: The type or namespace name 'IATTService' could not be found`, a `.meta` GUID mismatch occurred — check that the `.meta` file moved alongside the `.cs` file and the old path has no lingering `.meta`.
- If test count drops below 340, a Bee dag cache issue is likely (K011) — delete the active dag files and trigger an `Assets/Refresh`.
