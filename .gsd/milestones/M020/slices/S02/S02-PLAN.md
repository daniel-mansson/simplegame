# S02: Move Economy, Save, Progression, PlayFab, and MetaProgressionService

**Goal:** Create `Economy/`, `Save/`, `Progression/`, and `PlayFab/` feature folders; move MetaProgressionService to `Meta/`; move all remaining `Services/` files; remove the now-empty `Services/` folder and its `.meta`.

**Demo:** `Services/` directory gone; 5 target folders contain their respective files; tests pass.

## Must-Haves

- `Assets/Scripts/Game/Services/` directory does not exist
- `Assets/Scripts/Game/Services.meta` does not exist (git rm'd)
- `Economy/` (6 files), `Save/` (4 files), `Progression/` (4 files), `PlayFab/` (16 files) all exist
- `Meta/` has gained `MetaProgressionService.cs`
- 340 EditMode tests pass
- `git log --diff-filter=R --name-status -1` shows rename entries (not delete+add) confirming blame history preserved

## Tasks

- [x] **T01: Move Economy (6), Save (4), MetaProgressionService to Meta/**
  Economy: ICoinsService, CoinsService, IGoldenPieceService, GoldenPieceService, IHeartService, HeartService.
  Save: IMetaSaveService, MetaSaveData, MetaSaveMerge, PlayerPrefsMetaSaveService.
  Meta: MetaProgressionService.cs (moved to existing Meta/ folder).

- [ ] **T02: Move Progression (4) and PlayFab (16), remove Services/ folder, run tests, commit**
  Progression: ProgressionService, GameService, GameSessionService, GameOutcome.
  PlayFab (14 from Services/): IPlayFabAuthService, PlayFabAuthService, ICloudSaveService, PlayFabCloudSaveService, IAnalyticsService, PlayFabAnalyticsService, IPlatformLinkService, PlayFabPlatformLinkService, IRemoteConfigService, PlayFabRemoteConfigService, GameRemoteConfig, ISingularService, SingularService, NullSingularService.
  PlayFab (2 from Popup/): IPlatformLinkView, PlatformLinkPresenter.
  Then: rmdir Services/ + git rm Services.meta. Run tests; commit.

## Files Likely Touched

- `Assets/Scripts/Game/Services/` — emptied and removed
- `Assets/Scripts/Game/Economy/`, `Save/`, `Progression/`, `PlayFab/` — created
- `Assets/Scripts/Game/Meta/` — gains MetaProgressionService.cs

## Observability / Diagnostics

This slice performs only `git mv` renames — no runtime behaviour changes. Observable signals:

- **File presence:** `ls Assets/Scripts/Game/{Economy,Save,Progression,PlayFab,Meta}/` — confirms files landed in target folders.
- **Source removal:** `ls Assets/Scripts/Game/Services/` should return "No such file or directory" after T02.
- **Git rename tracking:** `git log --diff-filter=R --name-status -1` on the slice commit confirms renames rather than delete+add pairs (preserves blame history).
- **Compiler health:** Unity will auto-reimport moved files; compile errors in `Editor.log` are the failure signal. Use K011 to distinguish stale-cache errors from genuine ones.
- **Test gate:** `run_tests EditMode` returning 340 passed with 0 failed is the go/no-go signal for T02 completion.
- **Failure state:** If a `git mv` fails mid-task, `git status` shows partial rename staging; `git reset HEAD` restores clean state. No runtime state is affected — these are source file moves only.
