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
