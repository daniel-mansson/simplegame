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
