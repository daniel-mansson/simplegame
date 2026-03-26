# S01: Move IAP, Ads, and ATT Feature Groups — UAT

**Milestone:** M020
**Written:** 2026-03-26

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: This slice is a pure filesystem reorganisation with no runtime behaviour changes. All correctness signals are structural (file locations, git history, directory existence) and test-suite (EditMode pass/fail). No visual or interactive verification is needed.

## Preconditions

- Unity Editor is not mid-compile (wait for editor console to show no errors)
- Working directory: `Assets/Scripts/Game/`
- Git working tree is clean (no unstaged changes that would contaminate the output)

## Smoke Test

Run `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` from the project root — expect **15**. If this returns 15, the slice structure is intact.

## Test Cases

### 1. IAP folder contains exactly 15 source files

1. From project root: `find Assets/Scripts/Game/IAP -name "*.cs" | sort`
2. **Expected:** 15 files listed:
   - IAPMockConfig.cs, IAPOutcome.cs, IAPProductCatalog.cs, IAPProductDefinition.cs, IAPProductInfo.cs
   - IAPPurchasePresenter.cs, IAPPurchaseView.cs, IAPResult.cs
   - IIAPPurchaseView.cs, IIAPService.cs
   - MockIAPService.cs, NullIAPService.cs, NullPlayFabCatalogService.cs
   - PlayFabCatalogService.cs, UnityIAPService.cs

### 2. Ads folder contains exactly 7 source files

1. From project root: `find Assets/Scripts/Game/Ads -name "*.cs" | sort`
2. **Expected:** 7 files listed:
   - AdResult.cs, IAdService.cs, IRewardedAdView.cs
   - NullAdService.cs, RewardedAdPresenter.cs, RewardedAdView.cs, UnityAdService.cs

### 3. ATT folder contains exactly 7 source files

1. From project root: `find Assets/Scripts/Game/ATT -name "*.cs" | sort`
2. **Expected:** 7 files listed:
   - ATTAuthorizationStatus.cs, ConsentGatePresenter.cs, ConsentGateView.cs
   - IATTService.cs, IConsentGateView.cs, NullATTService.cs, UnityATTService.cs

### 4. Services/ directory no longer exists

1. From project root: `ls Assets/Scripts/Game/Services/`
2. **Expected:** Error — "No such file or directory" (exit code 2). The directory was emptied and removed by the moves.

### 5. Popup/ contains only UnityViewContainer

1. From project root: `ls Assets/Scripts/Game/Popup/`
2. **Expected:** Only `UnityViewContainer.cs` and `UnityViewContainer.cs.meta` listed. No IAP, Ads, ATT, or other popup files.

### 6. Moves were tracked as renames (not copy+delete)

1. Run: `git show c45a15f --stat | grep " => "`
2. **Expected:** Lines showing renames into IAP/, Ads/, and ATT/ — e.g.:
   - `Assets/Scripts/Game/{Services => IAP}/IIAPService.cs`
   - `Assets/Scripts/Game/{Services => Ads}/IAdService.cs`
   - `Assets/Scripts/Game/{Services => ATT}/IATTService.cs`
3. These rename records confirm .meta GUIDs were preserved (git tracked the move rather than treating it as delete+add).

### 7. No orphaned .meta files from emptied folders

1. Run: `git status --short | grep "^??"`
2. **Expected:** No `.meta` files appear in the output. Any `??` lines should be non-meta files (e.g. GSD task JSON files) — not stale meta remnants.

### 8. EditMode tests pass

1. Using the K006 stdin workaround: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`
2. Poll until complete: `mcporter call unityMCP.get_test_job job_id=<id>`
3. **Expected:** `status == "succeeded"`, `summary.passed >= 347`, `summary.failed == 0`

## Edge Cases

### IAP file with namespaces from Services/ namespace

1. Open `Assets/Scripts/Game/IAP/IIAPService.cs` in an editor
2. **Expected:** File is in the IAP/ folder but namespace reads `namespace SimpleGame.Game.Services` — this is intentional (D105). The namespace and folder do not need to match. Unity resolves types by GUID, not path.

### Popup enum reference in UnityViewContainer

1. Search: `rg "PopupId.IAPPurchase" Assets/Scripts/Game/Popup/`
2. **Expected:** One match in UnityViewContainer.cs — this is a PopupId enum value reference, not a moved file. It should remain in Popup/ and is correct.

### Confirm no IAP/Ads/ATT files remain in old locations

1. Run: `rg "class IIAPService\|class UnityIAPService\|class IAdService\|class IATTService" Assets/Scripts/Game/Services/ Assets/Scripts/Game/Popup/ 2>/dev/null`
2. **Expected:** No output (both directories either don't exist or contain no matching files).

## Failure Signals

- `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` returns anything other than 15 — a file is missing or extra
- `ls Assets/Scripts/Game/Services/` succeeds instead of erroring — Services/ directory still exists
- `git show c45a15f --stat` shows deletions without corresponding renames — GUIDs were not preserved
- `git status --short | grep "^??"` returns `.meta` files — orphaned meta from emptied folder
- EditMode tests: `summary.failed > 0` — a GUID break caused a compile error or missing-script issue
- Unity Editor Console shows `CS0246` or `CS0103` errors after reload — namespace resolution failure (shouldn't happen since namespaces were unchanged)

## Requirements Proved By This UAT

- none — this slice is a pure structural refactor with no capability changes

## Not Proven By This UAT

- Unity Editor visual inspection for missing-script warnings in Boot scene (requires opening Editor and checking Console)
- Runtime game flow (covered by existing tests and upstream milestone UATs)
- Namespace correctness — files intentionally retain old namespaces; namespace alignment is deferred

## Notes for Tester

The test count baseline is **347**, not 340 — the plan was written before 7 additional tests were added. All 347 should pass.

The Services/ directory being absent is correct and expected — it was completely emptied by these moves and removed. This is ahead of the S02 schedule; S02 can confirm it stays absent.

Popup/ containing only UnityViewContainer.cs is also correct and ahead of schedule — S03's Popup cleanup is already done.
