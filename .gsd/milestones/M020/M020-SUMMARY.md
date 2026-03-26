---
id: M020
provides:
  - Feature-cohesive folder structure for Assets/Scripts/Game/
  - IAP/ (15), Ads/ (7), ATT/ (7), Economy/ (6), Save/ (4), Progression/ (4), PlayFab/ (17), Shop/ (3), LevelFlow/ (7), ConfirmDialog/ (3), Meta/ extended (7 files)
  - Services/ directory removed
  - Popup/ reduced to UnityViewContainer.cs only
  - 347 EditMode tests passing, 0 regressions, 0 orphaned .meta files
key_decisions:
  - "D104: Feature-cohesion folder structure replaces layer folders"
  - "D105: Namespaces unchanged during restructure"
  - "git mv for .cs + filesystem move for .meta + git add -A is the correct Unity file move pattern"
patterns_established:
  - "New features get their own folder under Assets/Scripts/Game/<FeatureName>/ containing interface, implementation(s), and popup pair"
  - "git mv moves .cs; .meta must be moved separately (filesystem) and staged with git add -A"
requirement_outcomes: []
duration: single session
verification_result: passed
completed_at: 2026-03-26
---

# M020: Feature-Cohesion Restructure

**`Services/` (50 files) eliminated, `Popup/` reduced to 1 file — all code now grouped by feature into 11 purpose-named folders; 347/347 tests pass, zero regressions, zero orphaned .meta files.**

## What Happened

Reorganised `Assets/Scripts/Game/` from two layer-based dumping grounds into 11 feature-cohesive folders. Executed across four slices:

**S01** created `IAP/`, `Ads/`, `ATT/` — moved 29 files (service types + popup pairs) from `Services/` and `Popup/`. The IAP feature went from being split across two directories into a single `IAP/` folder containing everything: interface, real impl, mock, config ScriptableObjects, and the purchase popup.

**S02** created `Economy/`, `Save/`, `Progression/`, `PlayFab/` and moved `MetaProgressionService` to `Meta/`. Removed the empty `Services/` directory. One file (`IPlayFabCatalogService.cs`) was omitted from the plan count but discovered during execution and correctly moved to `PlayFab/`.

**S03** moved the remaining popup feature files: `ObjectRestored` trio to `Meta/` (belongs with the meta-world restoration feature), `Shop` trio to `Shop/`, `LevelFlow` group (7 files) to `LevelFlow/`, `ConfirmDialog` trio to `ConfirmDialog/`. Left `UnityViewContainer.cs` alone in `Popup/` as the container infrastructure.

**S04** confirmed all invariants: Services/ gone, Popup/ = 1 file, total `.cs` count unchanged (110), 0 orphaned `.cs.meta` files, 347/347 tests pass.

**Key lesson on Unity file moves:** `git mv file.cs newdir/file.cs` moves only the `.cs`. The paired `.meta` must be moved separately via the filesystem, then both staged together with `git add -A`. This is the correct pattern for all future Unity file reorganisations.

## Cross-Slice Verification

- `find Assets/Scripts/Game/Services -name "*.cs" 2>/dev/null` → no output (directory gone)
- `find Assets/Scripts/Game/Popup -name "*.cs"` → `UnityViewContainer.cs` only
- `find Assets/Scripts/Game -name "*.cs" | wc -l` → 110 (same as before)
- `git ls-files Assets/Scripts/Game/ | grep "\.cs\.meta$" | <orphan check>` → 0 orphans
- EditMode: 347/347 passed, 0 failed

## Requirement Changes

None — structural refactor only, no capability changes.

## Forward Intelligence

### What the next milestone should know
- New features get a folder under `Assets/Scripts/Game/<FeatureName>/` containing the interface, all implementations (real, mock, null), and the popup presenter+view pair if the feature has one
- Namespaces are still `SimpleGame.Game.Services` (service files) and `SimpleGame.Game.Popup` (popup files) — folder and namespace are not aligned. A future namespace-alignment pass would need to update `m_EditorClassIdentifier` strings in scene/asset files and all `using` statements
- `PlayFab/` has 17 files — it's the largest feature folder; if it grows further, consider splitting auth/cloud-save/analytics into sub-groups
- `Popup/` still holds `UnityViewContainer.cs` as infrastructure — this is intentional

### What's fragile
- `.meta` file moves require the two-step pattern (git mv .cs, filesystem mv .meta, git add -A) — a plain `git mv` on just the `.cs` leaves a stale `.meta` in the source folder that will be incorrectly tracked as a new file if not cleaned up

### Authoritative diagnostics
- `find Assets/Scripts/Game -name "*.cs" | wc -l` — should stay at 110 after any future file move within Game/
- `git ls-files Assets/Scripts/Game/ | grep "\.cs\.meta$" | while read m; do cs="${m%.meta}"; [ ! -f "$cs" ] && echo "ORPHAN: $m"; done` — orphan check

## Files Created/Modified

- `Assets/Scripts/Game/IAP/` — created (15 files)
- `Assets/Scripts/Game/Ads/` — created (7 files)
- `Assets/Scripts/Game/ATT/` — created (7 files)
- `Assets/Scripts/Game/Economy/` — created (6 files)
- `Assets/Scripts/Game/Save/` — created (4 files)
- `Assets/Scripts/Game/Progression/` — created (4 files)
- `Assets/Scripts/Game/PlayFab/` — created (17 files)
- `Assets/Scripts/Game/Shop/` — created (3 files)
- `Assets/Scripts/Game/LevelFlow/` — created (7 files)
- `Assets/Scripts/Game/ConfirmDialog/` — created (3 files)
- `Assets/Scripts/Game/Meta/` — extended with MetaProgressionService + ObjectRestored trio (now 7 files)
- `Assets/Scripts/Game/Popup/` — reduced to UnityViewContainer.cs
- `Assets/Scripts/Game/Services/` — removed
