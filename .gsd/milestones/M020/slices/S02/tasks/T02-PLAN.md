# T02: Move Progression (4) and PlayFab (16), Remove Services/, Run Tests, Commit

**Slice:** S02
**Milestone:** M020

## Goal

Move Progression and PlayFab files, fully empty and remove `Services/`, run tests, commit.

## Must-Haves

### Artifacts
- `Progression/`: ProgressionService.cs, GameService.cs, GameSessionService.cs, GameOutcome.cs
- `PlayFab/` (14 from Services/): IPlayFabAuthService.cs, PlayFabAuthService.cs, ICloudSaveService.cs, PlayFabCloudSaveService.cs, IAnalyticsService.cs, PlayFabAnalyticsService.cs, IPlatformLinkService.cs, PlayFabPlatformLinkService.cs, IRemoteConfigService.cs, PlayFabRemoteConfigService.cs, GameRemoteConfig.cs, ISingularService.cs, SingularService.cs, NullSingularService.cs
- `PlayFab/` (2 from Popup/): IPlatformLinkView.cs, PlatformLinkPresenter.cs
- `Services/` directory does not exist; `Services.meta` removed from git

### Truths
- `ls Assets/Scripts/Game/Services/` — "No such file or directory"
- `git status` — no untracked Services.meta
- 340 tests pass

## Steps

1. `git mv` 4 Progression files: Services/ → Progression/
2. `git mv` 14 PlayFab service files: Services/ → PlayFab/
3. `git mv` IPlatformLinkView.cs + PlatformLinkPresenter.cs: Popup/ → PlayFab/
4. Verify Services/ is now empty: `ls Assets/Scripts/Game/Services/` returns nothing
5. `rmdir Assets/Scripts/Game/Services` (Windows cmd) — or on bash: `rmdir 'Assets/Scripts/Game/Services'`
6. `git rm Assets/Scripts/Game/Services.meta`
7. Run EditMode tests (K006: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`)
8. Verify 340 pass, 0 fail
9. `git add -A && git commit -m "refactor(S02): move Economy, Save, Progression, PlayFab, MetaProgressionService; remove Services/"`

## Observability Impact

This task performs only `git mv` renames and directory removal — no runtime behaviour changes.

**What changes are visible to a future agent:**
- `ls Assets/Scripts/Game/Services/` → "No such file or directory" (directory removed)
- `ls Assets/Scripts/Game/{Progression,PlayFab}/` → lists expected files in each folder
- `git status` → no untracked `Services.meta` after `git rm`
- `git log --diff-filter=R --name-status -1` → all moved files show `R100` (rename 100%) entries, confirming blame history is intact rather than delete+add

**Failure state surfaces:**
- Mid-task failure: `git status` shows partial rename staging (files in both old and new locations); `git reset HEAD` restores clean state
- rmdir failure ("directory not empty"): `ls Assets/Scripts/Game/Services/` reveals unmoved files; fix by moving them then retry
- Compiler errors after moves: check `Editor.log` using K011 pattern to distinguish stale-cache errors from genuine ones (look for errors after the last `Starting:` line)
- Test failure: `get_test_job` returns `failed > 0`; check test runner output for which tests broke and which files they reference

## Context

- K006 test run method: stdin pipe to avoid UV_HANDLE_CLOSING crash on Windows
- K008: rmdir + git rm .meta in same commit
- If Services/ rmdir fails ("directory not empty"), run `ls Assets/Scripts/Game/Services/` to find any remaining files, move them, then retry
