# T01: Move Economy (6), Save (4), MetaProgressionService to Meta/

**Slice:** S02
**Milestone:** M020

## Goal

Create `Economy/` and `Save/` feature folders; move their files and MetaProgressionService to their target locations.

## Must-Haves

### Artifacts
- `Economy/`: ICoinsService.cs, CoinsService.cs, IGoldenPieceService.cs, GoldenPieceService.cs, IHeartService.cs, HeartService.cs
- `Save/`: IMetaSaveService.cs, MetaSaveData.cs, MetaSaveMerge.cs, PlayerPrefsMetaSaveService.cs
- `Meta/MetaProgressionService.cs` exists (moved from Services/)

## Steps

1. `git mv` 6 Economy files: Services/ → Economy/
2. `git mv` 4 Save files: Services/ → Save/
3. `git mv Assets/Scripts/Game/Services/MetaProgressionService.cs Assets/Scripts/Game/Meta/MetaProgressionService.cs`
4. `git status` — confirm 11 renames staged

## Context

- Do NOT commit yet — T02 completes the slice
- After this task, Services/ still has Progression + PlayFab files (moved in T02)

## Observability Impact

This task performs only `git mv` renames — no runtime code changes. Inspection surfaces:

- **File presence verification:** `ls Assets/Scripts/Game/Economy/` (expect 6 .cs files), `ls Assets/Scripts/Game/Save/` (expect 4 .cs files), `ls Assets/Scripts/Game/Meta/ | grep MetaProgression` (expect 1 file).
- **Staged renames:** `git status --short` should show 11 `R` entries after all moves.
- **Failure state:** If any `git mv` call fails (e.g. source file missing), `git status` reveals partial staging. Run `git reset HEAD` to restore clean state and investigate with `ls Services/` to find the actual filename.
- **No runtime signals change** — these moves affect compiler input paths only; Unity will re-resolve namespaces on next import. Compile errors post-move would surface in `Editor.log` (see K011 for distinguishing stale-cache vs genuine errors).
