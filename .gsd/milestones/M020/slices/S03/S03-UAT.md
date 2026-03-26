---
id: S03
milestone: M020
written: 2026-03-26
---

# S03: Move Remaining Popup Feature Files into Feature Folders — UAT

**Milestone:** M020
**Written:** 2026-03-26

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S03 is a pure file-move refactor with no runtime behaviour change. There is nothing new to interact with — the UAT confirms the correct files are in the correct locations and that Unity still compiles and tests pass.

## Preconditions

- Unity Editor is open with the project loaded
- `milestone/M020` branch is checked out
- No pending uncommitted changes (`git status` is clean)

## Smoke Test

Run `find Assets/Scripts/Game/Popup -name "*.cs"` from the project root. If it returns exactly one line (`Assets/Scripts/Game/Popup/UnityViewContainer.cs`), the slice is basically working — all feature popup files were moved out of Popup/.

## Test Cases

### 1. Popup/ contains only UnityViewContainer.cs

1. From project root: `find Assets/Scripts/Game/Popup -name "*.cs" | sort`
2. **Expected:** Exactly one result: `Assets/Scripts/Game/Popup/UnityViewContainer.cs`

### 2. Meta/ contains ObjectRestored popup trio

1. Run: `ls Assets/Scripts/Game/Meta/`
2. **Expected:** Output includes `IObjectRestoredView.cs`, `ObjectRestoredPresenter.cs`, `ObjectRestoredView.cs` (alongside existing Meta files: EnvironmentData.cs, MetaProgressionService.cs, RestorableObjectData.cs, WorldData.cs)

### 3. Shop/ contains 3 files

1. Run: `find Assets/Scripts/Game/Shop -name "*.cs" | sort`
2. **Expected:** Exactly 3 results: `IShopView.cs`, `ShopPresenter.cs`, `ShopView.cs`

### 4. LevelFlow/ contains 7 files

1. Run: `find Assets/Scripts/Game/LevelFlow -name "*.cs" | sort`
2. **Expected:** Exactly 7 results: `ILevelCompleteView.cs`, `LevelCompletePresenter.cs`, `LevelCompleteView.cs`, `ILevelFailedView.cs`, `LevelFailedPresenter.cs`, `LevelFailedView.cs`, `LevelFailedChoice.cs`

### 5. ConfirmDialog/ contains 3 files

1. Run: `find Assets/Scripts/Game/ConfirmDialog -name "*.cs" | sort`
2. **Expected:** Exactly 3 results: `IConfirmDialogView.cs`, `ConfirmDialogPresenter.cs`, `ConfirmDialogView.cs`

### 6. Unity compiles without errors

1. Open Unity Editor and focus the window to trigger recompilation if needed
2. Check the Console panel
3. **Expected:** No red error messages. No missing-script warnings on any scene.

### 7. EditMode tests pass

1. Open Test Runner (Window → General → Test Runner)
2. Select EditMode tab
3. Click "Run All"
4. **Expected:** 347 tests pass, 0 failures

## Edge Cases

### Namespace mismatch is not a bug

1. Run: `rg "namespace SimpleGame.Game.Popup" Assets/Scripts/Game/LevelFlow/`
2. **Expected:** Matches found — these files retain their original namespace declaration.
3. **Verify it's not a problem:** The 347 tests passing confirms Unity resolves these types correctly regardless of directory location. Namespace does not need to match directory path.

### Services/ is still absent

1. Run: `ls Assets/Scripts/Game/Services/ 2>&1`
2. **Expected:** Error or empty — the directory should not exist. (Removed in S01.)

## Failure Signals

- Any `.cs` file other than `UnityViewContainer.cs` found in `Assets/Scripts/Game/Popup/` → a file was not moved
- Missing files in LevelFlow/, ConfirmDialog/, Shop/, or Meta/ → a move was incomplete
- Red compile errors in Unity Console → a namespace or assembly reference was broken by the move
- Test count below 347 → either a test file was moved to a non-compiled location, or a type reference was broken
- Missing-script warning on Boot, MainMenu, InGame, or Settings scene → a MonoBehaviour GUID was broken (should not happen with `git mv`, but check if a raw copy occurred)

## Requirements Proved By This UAT

- none (structural refactor — no capability requirement changed)

## Not Proven By This UAT

- Runtime gameplay behaviour (out of scope for a pure file-move slice)
- Play mode functionality — only EditMode tests are run
- S04 orphaned `.meta` cleanup — that is S04's responsibility

## Notes for Tester

This slice made no functional changes. If the game played correctly before S03, it plays correctly after. The UAT is entirely about confirming the file layout and Unity compile/test health. If test count is 347, the refactor is safe.
