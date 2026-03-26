# S03: Move Remaining Popup Feature Files into Feature Folders

**Goal:** Move all remaining popup feature files from `Popup/` into `Shop/`, `LevelFlow/`, and `ConfirmDialog/`. Move ObjectRestored popup trio to `Meta/`. Leave only `UnityViewContainer.cs` in `Popup/`.

**Demo:** `Popup/` contains only `UnityViewContainer.cs`; feature popup code lives alongside its feature service; tests pass.

## Must-Haves

- `Popup/` contains exactly 1 `.cs` file: `UnityViewContainer.cs`
- `Meta/` has: IObjectRestoredView.cs, ObjectRestoredPresenter.cs, ObjectRestoredView.cs
- `Shop/` (3 files), `LevelFlow/` (7 files), `ConfirmDialog/` (3 files) exist
- 340 EditMode tests pass

## Tasks

- [x] **T01: Move ObjectRestored popup trio to Meta/, create Shop/ (3 files)**
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

## Observability / Diagnostics

This slice is a pure file-move refactor — no runtime behaviour changes.

**Inspection surfaces:**
- `git status --short` after each task to confirm renames staged cleanly
- `ls Assets/Scripts/Game/Popup/` to verify only `UnityViewContainer.cs` remains
- `ls Assets/Scripts/Game/Meta/ Assets/Scripts/Game/Shop/ Assets/Scripts/Game/LevelFlow/ Assets/Scripts/Game/ConfirmDialog/` to verify new destinations
- Unity compile errors surfaced via `read_console` (K002) if any namespace/reference breaks

**Failure visibility:**
- If a move was silently skipped (file already at destination from a prior run), `git status` will show a clean tree — treat this as success, not failure. Verify file presence directly with `ls`.
- If Unity shows `CS0246` (type not found) after moves, the file was moved but a `using` directive or hard reference still points to the old path. Since all files stay within the same assembly, no `using` changes are needed — namespace declarations must be verified to be path-independent.

**Redaction:** No secrets or credentials involved.

## Verification

| Check | Command | Pass condition |
|---|---|---|
| Popup/ cleaned | `find Assets/Scripts/Game/Popup/ -name "*.cs" \| sort` | Only `UnityViewContainer.cs` |
| Meta/ gains ObjectRestored | `ls Assets/Scripts/Game/Meta/` | IObjectRestoredView.cs, ObjectRestoredPresenter.cs, ObjectRestoredView.cs present |
| Shop/ created | `ls Assets/Scripts/Game/Shop/` | IShopView.cs, ShopPresenter.cs, ShopView.cs present |
| LevelFlow/ created | `ls Assets/Scripts/Game/LevelFlow/` | 7 `.cs` files present |
| ConfirmDialog/ created | `ls Assets/Scripts/Game/ConfirmDialog/` | 3 `.cs` files present |
| EditMode tests pass | via Unity MCP run_tests | 340 tests pass, 0 failures |
| **Failure diagnostic** | `rg "namespace.*Popup" Assets/Scripts/Game/Meta/ Assets/Scripts/Game/Shop/ Assets/Scripts/Game/LevelFlow/ Assets/Scripts/Game/ConfirmDialog/` | No matches (namespaces updated away from Popup) |
