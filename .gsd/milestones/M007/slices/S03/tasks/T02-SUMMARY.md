---
id: T02
parent: S03
milestone: M007
provides:
  - Milestone M007 verification: all 169 EditMode tests pass, zero FindObject* in Assets/
  - R074 validated: FindSceneController<T> helper in GameBootstrapper.cs
  - R075 validated: zero FindObject* variants across entire Assets/ tree
  - R076 validated: 169/169 tests passed
  - R077 flagged: Human UAT required before milestone closure
key_files: []
key_decisions:
  - run_tests MCP tool crashes mcporter via Windows UV_HANDLE_CLOSING; workaround is stdin pipe or direct HTTP — see K entry
patterns_established:
  - mcporter call unityMCP.run_tests fails on Windows; use stdin pipe: echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin
  - Poll test job with: mcporter call unityMCP.get_test_job job_id=<id>
observability_surfaces:
  - Unity MCP test job API: run_tests → get_test_job; job_id 766d31f2ff0b434eaad592ac6a7a8796 confirmed 169/169 passed
  - rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/ → exit 1
  - rg -n "FindSceneController|GetSceneByName|GetRootGameObjects" Assets/Scripts/Game/Boot/GameBootstrapper.cs → 5 lines confirmed
duration: 15m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T02: Run full test suite and verify milestone completion

**All 169 EditMode tests pass and zero FindObject* variants exist anywhere in Assets/ — M007 milestone gate fully satisfied.**

## What Happened

Ran all verification checks defined in the slice plan:

1. **Zero FindObject* across Assets/:** `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` returned exit 1 (no matches). R075 confirmed.

2. **FindSceneController helper verified:** `rg -n "FindSceneController|GetSceneByName|GetRootGameObjects"` against `GameBootstrapper.cs` returned 5 lines — the private static helper at line 158 with `GetSceneByName` (line 160) and `GetRootGameObjects` (line 162), plus the 3 call sites at lines 99, 113, 126. R074 confirmed.

3. **Unity test run:** The `run_tests` MCP tool consistently crashes mcporter on Windows with `UV_HANDLE_CLOSING` assertion error (see K003-adjacent issue below). Workaround: used stdin pipe `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` which successfully initiated a test job (ID: `766d31f2ff0b434eaad592ac6a7a8796`). Polled with `mcporter call unityMCP.get_test_job job_id=766d31f2ff0b434eaad592ac6a7a8796` — result: `status: succeeded`, `total: 169`, `passed: 169`, `failed: 0`, `skipped: 0`. Last test: `ViewContainerGetTests.Get_ReturnsNull_WhenInterfaceNotFound`. R076 confirmed.

4. **Existing TestResults.xml** showed 49 tests from the Mar 16 batchmode run — this was the K003 issue (stale cached assemblies from before S01/S02 new test files were added). The live editor run correctly picked up all 169 tests.

## Verification

| Check | Result |
|---|---|
| `rg -g "*.cs" "FindFirstObjectByType\|FindObjectOfType\|..." Assets/` | ✅ exit 1 (zero matches) |
| `rg "FindSceneController" GameBootstrapper.cs` | ✅ 5 matches (1 def + 3 call sites + 1 helper body line) |
| `rg "GetSceneByName\|GetRootGameObjects" GameBootstrapper.cs` | ✅ confirmed |
| Unity EditMode test run (live editor, 169 tests) | ✅ 169/169 passed, 0 failed |
| R074: FindSceneController helper in place | ✅ validated |
| R075: Zero FindObject* in Assets/Scripts/ | ✅ validated |
| R076: All 169+ tests pass | ✅ validated |
| R077: Human UAT required | ⚠️ flagged — not yet performed |

## Diagnostics

- **How to re-run tests:** `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` (stdin pipe required on Windows)
- **How to poll results:** `mcporter call unityMCP.get_test_job job_id=<id>`
- **Failure state:** If test count < 169 in a batchmode XML, it's K003 (domain-reload not triggered since new tests were compiled). Close and reopen Unity editor to force reload, then re-run.
- **FindSceneController failure at runtime:** Unity Console shows `[GameBootstrapper] XyzSceneController not found in scene.` — scene wasn't loaded before lookup.

## Deviations

- **`run_tests` MCP call via mcporter CLI crashes on Windows:** The `mcporter call unityMCP.run_tests testMode:EditMode` command consistently hits a `UV_HANDLE_CLOSING` assertion in `src\win\async.c`. Root cause: mcporter's Windows libuv async handle is closed before the test runner's callback fires (long-running async operation). Workaround: use stdin pipe mode which avoids the positional arg parsing path.
- **Existing TestResults.xml showed 49 (not 169):** Stale from Mar 16 batchmode run that used pre-S01/S02 compiled assemblies. Not a failure — live editor test run correctly found all 169.

## Known Issues

- **R077: Human UAT pending.** The milestone requires a play-through of MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win flows. This cannot be automated and must be performed by a human in the Unity editor. The milestone is not fully closed until this UAT passes.
- **mcporter `run_tests` CLI crash:** Only the stdin pipe workaround works on Windows. Direct `mcporter call unityMCP.run_tests testMode:EditMode` is broken. Future agents must use stdin pipe.

## Files Created/Modified

- None — verification-only task.
- Pre-flight fixes applied to:
  - `.gsd/milestones/M007/slices/S03/tasks/T02-PLAN.md` — added `## Observability Impact` section
  - `.gsd/milestones/M007/slices/S03/S03-PLAN.md` — added `### Failure-Path Diagnostics` subsection to Verification
