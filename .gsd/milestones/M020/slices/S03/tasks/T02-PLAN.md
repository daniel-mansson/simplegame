# T02: Create LevelFlow/ and ConfirmDialog/, Verify Popup/ Clean, Run Tests, Commit

**Slice:** S03
**Milestone:** M020

## Goal

Create `LevelFlow/` and `ConfirmDialog/` with all remaining popup feature files, verify `Popup/` is down to 1 file, run tests, commit.

## Must-Haves

### Artifacts
- `LevelFlow/`: ILevelCompleteView.cs, LevelCompletePresenter.cs, LevelCompleteView.cs, ILevelFailedView.cs, LevelFailedPresenter.cs, LevelFailedView.cs, LevelFailedChoice.cs
- `ConfirmDialog/`: IConfirmDialogView.cs, ConfirmDialogPresenter.cs, ConfirmDialogView.cs
- `Popup/` contains exactly: UnityViewContainer.cs (and its .meta)

### Truths
- `find Assets/Scripts/Game/Popup -name "*.cs" | wc -l` returns 1
- `find Assets/Scripts/Game/Popup -name "*.cs"` shows only `UnityViewContainer.cs`
- 340 tests pass

## Steps

1. `git mv` 7 LevelFlow files: Popup/ → LevelFlow/
2. `git mv` 3 ConfirmDialog files: Popup/ → ConfirmDialog/
3. Verify: `find Assets/Scripts/Game/Popup -name "*.cs"` → only UnityViewContainer.cs
4. Run EditMode tests (K006 stdin method)
5. Verify 340 pass, 0 fail
6. `git add -A && git commit -m "refactor(S03): move popup feature files into feature folders; Popup/ contains only UnityViewContainer"`

## Context

- LevelFailedChoice.cs is a plain enum/struct in Popup/ — moves with LevelFlow/
- Popup/ will still have UnityViewContainer.cs and potentially PopupId.cs at Game root — PopupId.cs is at `Game/PopupId.cs`, NOT in `Popup/`, so it is unaffected
