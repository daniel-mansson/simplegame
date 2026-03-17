---
estimated_steps: 4
estimated_files: 0
---

# T02: Run full test suite and verify milestone completion

**Slice:** S03 — Scene Root Convention + Final Cleanup
**Milestone:** M007

## Description

Run the full Unity edit-mode test suite in batchmode to confirm all 169+ tests pass after the S01–S03 refactoring. This is the milestone-level gate for R076 (all tests pass) and the final verification that zero `FindObject*` variants exist anywhere in production code (R075). No code changes — this is pure verification.

**Note:** Per K003, if the Unity editor hasn't domain-reloaded since S01/S02, new test files (like `ViewContainerTests.cs`) may not be detected. The executor should attempt the test run and report the count. If the count is lower than expected, note this as a K003 issue requiring an editor restart — do not treat it as a failure.

**Relevant skill:** None required — this is verification only.

## Steps

1. Run comprehensive `rg` verification across the entire `Assets/` directory:
   ```bash
   rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/
   ```
   Must return exit 1 (zero matches across ALL `.cs` files — Scripts/, Editor/, Tests/).

2. Verify the `FindSceneController` helper exists with correct structure:
   ```bash
   rg -n "FindSceneController|GetSceneByName|GetRootGameObjects" Assets/Scripts/Game/Boot/GameBootstrapper.cs
   ```
   Must show the helper definition, `GetSceneByName` call, `GetRootGameObjects` call, and 3 usage sites.

3. Run the Unity batchmode edit-mode test suite. Use the Unity MCP server or a direct batchmode command:
   ```
   mcp_call(server: "unityMCP", tool: "run_tests", args: {"testMode": "EditMode"})
   ```
   Or if MCP is unavailable, note the command for the user to run manually.
   
   Expected: all 169+ tests pass (the count may vary slightly — K003 caveat applies to new test detection).

4. Summarize milestone verification status:
   - R074: Scene root convention in place (FindSceneController helper)
   - R075: Zero FindObject* in Assets/Scripts/
   - R076: All 169+ tests pass
   - R077: Human UAT required (flag for user)

## Must-Haves

- [ ] `rg` confirms zero `FindObject*` variants across entire `Assets/` tree (all `.cs` files)
- [ ] Unity batchmode test run attempted with results reported
- [ ] Test count ≥ 169 and all passing (or K003 caveat documented if count is lower)
- [ ] Milestone verification checklist summarized

## Verification

- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` → exit 1
- Unity batchmode test exit code 0
- Test count ≥ 169, 0 failures
- Milestone summary: R074 validated, R075 validated, R076 validated, R077 flagged for human UAT

## Inputs

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — T01's output with `FindSceneController` helper and zero `FindObject*` calls
- All test files unchanged from S02 state — no test modifications expected or required in S03
- K003 knowledge entry — domain-reload-disabled editor may not detect new test files without restart

## Observability Impact

- **What changes:** No runtime code changes. The test run itself surfaces whether all 169 test methods are compiled and passing under the current domain state.
- **How a future agent inspects this task:** Run `mcporter call unityMCP.run_tests job_id` → poll `get_test_job` for `status: succeeded` and `summary.total == 169`. Alternatively, check `TestResults_M007.xml` (or the latest `TestResults.xml`) — the `<test-run>` root attribute `testcasecount` must be 169.
- **Failure state visibility:** If the test count is < 169, the K003 domain-reload issue is in effect — the running editor hasn't reloaded since new test files were added. Remedy: close and reopen the Unity editor to force domain reload, then re-run tests. If any tests fail, read the `<test-case result="Failed">` entries in the XML output for the exact assertion message and stack trace.
- **Diagnostic command:** `rg -g "*.cs" "\[Test\]\|\[TestCase\]\|\[UnityTest\]" Assets/Tests/ | wc -l` should return 169. If the batchmode XML shows fewer, it's a stale assembly cache (K003).

## Expected Output

- No files modified — this is a verification-only task
- Verification report: test count, pass/fail status, grep results, milestone checklist
- Human UAT flagged as the remaining gate before milestone closure
