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
