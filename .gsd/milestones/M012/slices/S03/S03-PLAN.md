# S03: View + SceneController wiring

**Goal:** Replace the temporary bridge layer with the proper wired stack. Update `IInGameView` to slot-indexed API (`RefreshSlot`). Update `InGameView`, `MockInGameView`. Wire `InGameSceneController` to read `PuzzleModelConfig`. Update `JigsawLevelFactory` to produce output `PuzzleModel` accepts directly. Remove all temp bridges.

**Demo:** Play-from-editor with GridLayoutConfig assigned: 3 slots show pieces; tapping correct slot places piece and refills slot; tapping wrong slot costs heart; all pieces placed → win popup.

## Must-Haves

- `IInGameView` gains `RefreshSlot(int slotIndex, int? pieceId)` — replaces `RefreshTray(int?[])`
- `InGameView` implements `RefreshSlot` with slot-indexed callbacks
- `MockInGameView` in tests implements `RefreshSlot`
- `InGameSceneController` reads `PuzzleModelConfig`, builds `PuzzleModel` with correct slot count; removes `BuildPuzzleModel` temp bridge
- `JigsawLevelFactory` updated: `JigsawBuildResult` gains `PieceList`, `SeedIds`, `DeckOrder` alongside `Level` and `RawBoard`; scene controller uses these to construct `PuzzleModel` directly
- `InGamePresenter.HandleSlotChanged` updated to call `RefreshSlot` instead of `RefreshTray`
- Temp bridge in `InGameSceneController` removed
- All EditMode tests still pass

## Tasks

- [ ] **T01: IInGameView slot-indexed API + InGameView + InGamePresenter update**
  Add RefreshSlot to IInGameView. Update InGameView with slot-indexed callbacks. Update InGamePresenter to call RefreshSlot. Remove RefreshTray from interface.

- [ ] **T02: InGameSceneController wiring — PuzzleModelConfig + JigsawLevelFactory update**
  Wire InGameSceneController to read PuzzleModelConfig and construct PuzzleModel. Update JigsawLevelFactory to expose flat piece list, seeds, and deck. Remove BuildPuzzleModel bridge. Update MockInGameView and tests.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/IInGameView.cs`
- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/InGame/InGamePresenter.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs` — MockInGameView
