# S03: Move Remaining Popup Feature Files into Feature Folders

**Goal:** Move all remaining popup feature files from `Popup/` into `Shop/`, `LevelFlow/`, and `ConfirmDialog/`. Move ObjectRestored popup trio to `Meta/`. Leave only `UnityViewContainer.cs` in `Popup/`.

**Demo:** `Popup/` contains only `UnityViewContainer.cs`; feature popup code lives alongside its feature service; tests pass.

## Must-Haves

- `Popup/` contains exactly 1 `.cs` file: `UnityViewContainer.cs`
- `Meta/` has: IObjectRestoredView.cs, ObjectRestoredPresenter.cs, ObjectRestoredView.cs
- `Shop/` (3 files), `LevelFlow/` (7 files), `ConfirmDialog/` (3 files) exist
- 340 EditMode tests pass

## Tasks

- [ ] **T01: Move ObjectRestored popup trio to Meta/, create Shop/ (3 files)**
  Meta/: IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView (from Popup/).
  Shop/: IShopView, ShopPresenter, ShopView (from Popup/).

- [ ] **T02: Create LevelFlow/ (7 files) and ConfirmDialog/ (3 files); run tests; commit**
  LevelFlow/: ILevelCompleteView, LevelCompletePresenter, LevelCompleteView, ILevelFailedView, LevelFailedPresenter, LevelFailedView, LevelFailedChoice.
  ConfirmDialog/: IConfirmDialogView, ConfirmDialogPresenter, ConfirmDialogView.
  Verify Popup/ contains only UnityViewContainer.cs. Run tests; commit.

## Files Likely Touched

- `Assets/Scripts/Game/Popup/` — most files moved out (UnityViewContainer.cs stays)
- `Assets/Scripts/Game/Meta/` — gains 3 ObjectRestored files
- `Assets/Scripts/Game/Shop/`, `LevelFlow/`, `ConfirmDialog/` — created
