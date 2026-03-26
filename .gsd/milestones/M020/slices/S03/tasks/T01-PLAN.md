# T01: Move ObjectRestored to Meta/, Create Shop/

**Slice:** S03
**Milestone:** M020

## Goal

Move ObjectRestored popup trio to `Meta/` and create `Shop/` with its 3 files.

## Must-Haves

### Artifacts
- `Meta/`: IObjectRestoredView.cs, ObjectRestoredPresenter.cs, ObjectRestoredView.cs (added to existing folder)
- `Shop/`: IShopView.cs, ShopPresenter.cs, ShopView.cs

## Steps

1. `git mv` IObjectRestoredView.cs, ObjectRestoredPresenter.cs, ObjectRestoredView.cs: Popup/ → Meta/
2. `git mv` IShopView.cs, ShopPresenter.cs, ShopView.cs: Popup/ → Shop/
3. `git status` — confirm 6 renames staged

## Context

- Do NOT commit yet — T02 completes the slice
- ObjectRestored belongs with Meta because it fires when a meta-world object is fully restored

## Observability Impact

This task is a pure file-move — no runtime signals change.

**Inspection:**
- `git status --short` after the moves: expect 6 renames staged (R → new path)
- If working tree is clean and files are at destination, a prior run already completed the moves — treat as success
- `ls Assets/Scripts/Game/Meta/` should list the 3 ObjectRestored files alongside existing Meta content
- `ls Assets/Scripts/Game/Shop/` should list exactly 3 files

**Failure state:** If files are absent from destination and `git status` is clean, check `git log --oneline -5` — the move may have been committed already. If files are absent and `git status` shows no staged renames, the source files may have been renamed differently in an earlier milestone; use `find Assets/ -name "IObjectRestoredView.cs"` to locate them.
