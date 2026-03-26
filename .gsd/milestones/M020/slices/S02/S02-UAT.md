# S02: Move Economy, Save, Progression, and PlayFab feature groups — UAT

**Milestone:** M020
**Written:** 2026-03-26

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: This slice is a pure file reorganisation — no runtime behaviour changed. The UAT confirms that the correct files are in the correct locations, the old location is gone, and the Unity test suite still passes after the move. No play-mode verification is required because no game logic or wiring was touched.

## Preconditions

- Unity Editor is open on this project
- The `milestone/M020` branch is checked out
- Commit `52be57d` (or later) is present in git log

## Smoke Test

Run `ls Assets/Scripts/Game/Services/` in the project root. It must return "No such file or directory". If it returns a listing, S02 is not complete.

## Test Cases

### 1. Economy/ contains exactly 6 service files

1. Open a terminal at the project root.
2. Run: `ls Assets/Scripts/Game/Economy/ | grep -v .meta`
3. **Expected:** Exactly 6 files: `CoinsService.cs`, `GoldenPieceService.cs`, `HeartService.cs`, `ICoinsService.cs`, `IGoldenPieceService.cs`, `IHeartService.cs`

### 2. Save/ contains exactly 4 service files

1. Run: `ls Assets/Scripts/Game/Save/ | grep -v .meta`
2. **Expected:** Exactly 4 files: `IMetaSaveService.cs`, `MetaSaveData.cs`, `MetaSaveMerge.cs`, `PlayerPrefsMetaSaveService.cs`

### 3. Progression/ contains exactly 4 service files

1. Run: `ls Assets/Scripts/Game/Progression/ | grep -v .meta`
2. **Expected:** Exactly 4 files: `GameOutcome.cs`, `GameService.cs`, `GameSessionService.cs`, `ProgressionService.cs`

### 4. PlayFab/ contains all platform files

1. Run: `ls Assets/Scripts/Game/PlayFab/ | grep -v .meta`
2. **Expected:** 17 files including `IPlayFabAuthService.cs`, `PlayFabAuthService.cs`, `IPlatformLinkView.cs`, `PlatformLinkPresenter.cs`, `ICloudSaveService.cs`, `PlayFabCloudSaveService.cs`, `IAnalyticsService.cs`, `PlayFabAnalyticsService.cs`, `IPlatformLinkService.cs`, `PlayFabPlatformLinkService.cs`, `IRemoteConfigService.cs`, `PlayFabRemoteConfigService.cs`, `GameRemoteConfig.cs`, `ISingularService.cs`, `SingularService.cs`, `NullSingularService.cs`, `IPlayFabCatalogService.cs`

### 5. MetaProgressionService relocated to Meta/

1. Run: `ls Assets/Scripts/Game/Meta/ | grep MetaProgressionService`
2. **Expected:** `MetaProgressionService.cs` is present in Meta/; it is NOT present in `Assets/Scripts/Game/Services/` (which no longer exists)

### 6. Services/ directory fully removed

1. Run: `ls Assets/Scripts/Game/Services/ 2>&1; echo "exit: $?"`
2. **Expected:** Output is "ls: cannot access ...: No such file or directory" and exit code is 2
3. Run: `ls Assets/Scripts/Game/Services.meta 2>&1; echo "exit: $?"`
4. **Expected:** Same "No such file or directory" response — no orphan meta file

### 7. Git rename tracking confirms blame history preserved

1. Run: `git log --diff-filter=R --name-status 52be57d | grep -c "^R100"`
2. **Expected:** Count of 29 or higher (all Economy, Save, Progression, PlayFab, and Meta moves recorded as R100 pure renames, not delete+add pairs)

### 8. All 347 EditMode tests pass

1. Run via K006 stdin pipe method:
   ```bash
   echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin
   ```
2. Note the returned `job_id`.
3. Poll until complete: `mcporter call unityMCP.get_test_job job_id=<job_id>`
4. **Expected:** `status: succeeded`, `total: 347`, `passed: 347`, `failed: 0`

## Edge Cases

### No .meta orphans from Services/

1. Run: `git status --short | grep Services`
2. **Expected:** No output — no untracked or modified files mentioning `Services`

### Compiler is clean after moves

1. In the Unity Editor, open the Console (Window → General → Console).
2. **Expected:** No error CS* messages relating to missing namespaces or unresolved types. If errors appear, apply the K011 diagnostic: check `Editor.log` for errors appearing after the last `Starting:` line.

## Failure Signals

- `ls Assets/Scripts/Game/Services/` returns a file listing — moves incomplete or Services/ was not removed
- Any `git log --diff-filter=D` showing a deleted Services/ file with no corresponding R entry — indicates a destructive move that broke blame history
- `IPlayFabAuthService.cs` or `PlatformLinkPresenter.cs` not found anywhere in `Assets/Scripts/Game/` — indicates a missing file from the Popup/ → PlayFab/ pair
- EditMode test count below 340, or any failures — indicates a compile or API breakage introduced by the moves

## Requirements Proved By This UAT

- none — this slice is a structural refactor only; no capability requirements are proved or changed

## Not Proven By This UAT

- Runtime behaviour (game loop, popup flow, cloud save, IAP) — not affected by this slice; verified by prior milestones (M016–M019)
- Missing-script warnings in scenes — not directly checked here; covered by S04

## Notes for Tester

The test count is 347, not 340 as the slice plan originally estimated. Seven additional tests from S03 are already committed on this branch. This is expected — not a failure signal.

`IPlayFabCatalogService.cs` appears in `PlayFab/` but was not in the original S02 manifest. This is correct; it belongs there and was swept in from Services/ as part of the cleanup. Count 17 files in PlayFab/, not 16.
