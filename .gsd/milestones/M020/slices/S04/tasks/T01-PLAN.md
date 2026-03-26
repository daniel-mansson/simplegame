# T01: Audit, Orphan Cleanup, Final Test Pass

**Slice:** S04
**Milestone:** M020

## Goal

Verify all structural invariants are met; fix any orphaned `.meta` files; run final 340-test pass; commit the milestone summary.

## Must-Haves

### Truths

| Check | Command | Expected |
|---|---|---|
| Services/ gone | `ls Assets/Scripts/Game/Services` | No such directory |
| Popup/ has 1 file | `find Assets/Scripts/Game/Popup -name "*.cs"` | Only UnityViewContainer.cs |
| IAP/ count | `find Assets/Scripts/Game/IAP -name "*.cs" \| wc -l` | 15 |
| Ads/ count | `find Assets/Scripts/Game/Ads -name "*.cs" \| wc -l` | 7 |
| ATT/ count | `find Assets/Scripts/Game/ATT -name "*.cs" \| wc -l` | 7 |
| Economy/ count | `find Assets/Scripts/Game/Economy -name "*.cs" \| wc -l` | 6 |
| Save/ count | `find Assets/Scripts/Game/Save -name "*.cs" \| wc -l` | 4 |
| Progression/ count | `find Assets/Scripts/Game/Progression -name "*.cs" \| wc -l` | 4 |
| PlayFab/ count | `find Assets/Scripts/Game/PlayFab -name "*.cs" \| wc -l` | 16 |
| Shop/ count | `find Assets/Scripts/Game/Shop -name "*.cs" \| wc -l` | 3 |
| LevelFlow/ count | `find Assets/Scripts/Game/LevelFlow -name "*.cs" \| wc -l` | 7 |
| ConfirmDialog/ count | `find Assets/Scripts/Game/ConfirmDialog -name "*.cs" \| wc -l` | 3 |
| Meta/ gains | `find Assets/Scripts/Game/Meta -name "*.cs" \| wc -l` | ≥7 (3 original + MetaProgressionService + 3 ObjectRestored) |
| No orphaned .meta | `find Assets/Scripts/Game -name "*.meta" -not -path "*/.*"` | No .meta file without a corresponding source file |
| Tests | EditMode run | 340 passed, 0 failed |

## Steps

1. Run all count checks from the table above
2. Check for orphaned `.meta` files: `find Assets/Scripts/Game -name "*.meta"` — confirm each `.meta` has a matching `.cs` or directory
3. If any orphan found: `git rm <orphan>.meta` and commit
4. Run EditMode tests (K006 stdin method)
5. Verify 340 pass, 0 fail
6. If all clean: `git add -A && git commit -m "chore(S04): verify final structure — feature-cohesion restructure complete"` (only if there were cleanup changes; skip commit if nothing to commit)

## Context

- If a `.meta` file is orphaned (its source was removed but .meta wasn't `git rm`'d), it will show as a tracked file with no counterpart — `git status` will show it as a deleted file. `git rm` it.
- Missing-script warnings in Unity: these appear in Console as yellow warnings mentioning GUID. If any appear after opening Unity, it means a `.meta` GUID was not preserved — check if any move was done without `git mv`. Recovery: find the `.meta` file in git history, restore the GUID.
- Total .cs file count should be unchanged: `find Assets/Scripts/Game -name "*.cs" | wc -l` before == after
