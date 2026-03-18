# M011: Puzzle Domain Model & API

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

A mobile-style puzzle game where players restore a meta-world by completing jigsaw-style puzzle levels. Ten milestones of architecture and infrastructure are complete. InGame has stub gameplay (hearts + counter). This milestone replaces the stub with a real puzzle domain model and wires it into the InGame scene with rendered, tappable jigsaw piece GameObjects.

## Why This Milestone

The `simple-jigsaw` package is already integrated (M010) and actively evolving. Game code must not be coupled to its types. This milestone defines the domain boundary: a pure C# puzzle model that the game depends on, with a jigsaw adapter as the only coupling point. Without this, every change to `simple-jigsaw` bleeds into game logic.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Open the InGame scene, press Play, and see real jigsaw piece meshes rendered on screen
- Tap a piece to attempt placement — the model determines correct (piece placed, counter advances) or incorrect (heart lost)
- Win by placing all non-seed pieces; lose by exhausting hearts
- Level design is expressed as an ordered deck of piece IDs — the order determines how the puzzle is solved

### Entry point / environment

- Entry point: Unity Editor Play mode, InGame scene (direct play or via boot flow)
- Environment: Unity Editor, local dev
- Live dependencies involved: `simple-jigsaw` package (board generation), existing InGame scene infrastructure

## Completion Class

- Contract complete means: all EditMode tests pass; `SimpleGame.Puzzle` asmdef compiles with `noEngineReferences: true`; no `SimpleJigsaw.*` types visible outside the adapter
- Integration complete means: InGame scene runs in Play mode with real piece meshes, tap resolves via model, win/lose triggers correctly
- Operational complete means: none (no service lifecycle concerns this milestone)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Press Play in InGame scene → jigsaw pieces render at solved positions
- Tap the correct next piece → it is accepted by the model, counter updates
- Tap a wrong piece → heart is lost; zero hearts → lose popup
- Tap all pieces in correct order → win popup
- No `SimpleJigsaw.*` type appears in any import outside `JigsawLevelFactory` or the adapter layer

## Risks and Unknowns

- `simple-jigsaw` API stability — the package is expected to change; the adapter must absorb all breakage — this is the point of the milestone
- `PieceObjectFactory` returns `List<GameObject>` with no tap support — need a thin MonoBehaviour tap handler added at creation time; must not couple to game logic
- Assembly reference graph — `SimpleGame.Puzzle` must not reference `SimpleJigsaw`; need to verify no transitive reference sneaks in via `SimpleGame.Game`
- InGame scene currently has placeholder `OnPlaceCorrect`/`OnPlaceIncorrect` buttons — these are removed; the view interface changes, which will break existing tests that mock `IInGameView`

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — current stub presenter; `WaitForAction()` pattern is reused; `_totalPieces`/`_piecesPlaced` logic gets replaced by `PuzzleSession` delegation
- `Assets/Scripts/Game/InGame/IInGameView.cs` — `OnPlaceCorrect`/`OnPlaceIncorrect` events are replaced by `OnTapPiece(int pieceId)`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — constructs presenter + session; `_defaultTotalPieces` replaced by level definition
- `Assets/Scripts/Game/Boot/UIFactory.cs` — `CreateInGamePresenter` signature will change to accept `IPuzzleLevel`
- `Assets/JigsawDemo/PuzzleSceneDriver.cs` — reference for how `BoardFactory.Generate` + `PieceObjectFactory.CreateAll` are called
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Models/PuzzleBoard.cs` — source of piece descriptors and neighbor map
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Models/PieceDescriptor.cs` — `Id`, `SolvedPosition`, `Neighbors` list
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Models/NeighborMap.cs` — `GetNeighbors(pieceId)` returns neighbor list
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/PieceObjectFactory.cs` — returns `List<GameObject>`; tap handler must be added to each after creation
- `Assets/Tests/EditMode/Game/InGameTests.cs` — will need updating when `IInGameView` changes
- `Assets/Scripts/Game/Services/HeartService.cs` — reused as-is

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R060 — Real puzzle board with piece placement (promoted to active, owned by this milestone)
- R091 — Pure puzzle domain model with no Unity dependencies
- R092 — Placement rule: neighbor-presence validation
- R093 — Deck abstraction: ordered piece sequence
- R094 — Configurable deck layout: one-per-slot or shared
- R095 — Jigsaw adapter hides SimpleJigsaw types from game code
- R096 — InGame wired to PuzzleSession (tap → model → correct/incorrect)
- R097 — Jigsaw pieces rendered as tappable GameObjects in InGame scene
- R098 — Drag-and-drop UX (explicitly deferred, out of scope for M011)

## Scope

### In Scope

- `SimpleGame.Puzzle` assembly: pure C# puzzle domain model (`IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`, `PuzzleSession`, placement validator)
- `JigsawLevelFactory` adapter in `SimpleGame.Game`: converts `SimpleJigsaw.PuzzleBoard` → `IPuzzleLevel`
- `IInGameView` changed: `OnTapPiece(int pieceId)` replaces `OnPlaceCorrect`/`OnPlaceIncorrect`
- `InGamePresenter` delegates to `PuzzleSession.TryPlace(pieceId)`
- Thin `PieceTapHandler` MonoBehaviour on each piece GameObject
- `InGameSceneController` constructs `PuzzleSession` from level definition and wires the scene
- EditMode tests for all domain model logic and the adapter
- Tap-only interaction

### Out of Scope / Non-Goals

- Drag-and-drop UX (R098 — explicitly deferred)
- Camera auto-adjust (R099 — deferred)
- Real puzzle art/textures (plain material is fine)
- Piece selection UI / "tray" widget — tap directly on the board piece
- Level select / level data ScriptableObjects (hardcoded config for now)
- Any changes to `simple-jigsaw` internals

## Technical Constraints

- `SimpleGame.Puzzle` asmdef: `noEngineReferences: true` — zero `UnityEngine`/`UnityEditor` imports
- `SimpleGame.Puzzle` asmdef must NOT reference `SimpleJigsaw`
- Only `SimpleGame.Game` (via `JigsawLevelFactory`) may reference `SimpleJigsaw`
- No `PieceDragger` usage anywhere in game code
- No logic distributed across piece GameObjects — `PieceTapHandler` calls view only, nothing else
- Existing MVP conventions (D003, D004, D026, D027) apply to all new presenters

## Integration Points

- `simple-jigsaw` package — consumed only via `JigsawLevelFactory`; `BoardFactory.Generate()` is the entry point; `PieceObjectFactory.CreateAll()` for rendering
- `HeartService` — reused as-is from existing services
- `GameSessionService` — `CurrentLevelId` used for level label; `TotalPieces` replaced by `IPuzzleLevel.TotalPieceCount`
- `UIFactory` — `CreateInGamePresenter` signature updated to receive `IPuzzleLevel`
- InGame scene infrastructure — `InGameSceneController`, `InGameView`, popup flow unchanged in shape

## Open Questions

- Seed piece placement: are seeds defined in the level definition (pre-placed before game starts) or triggered by the first draw? — current thinking: seeds are part of `IPuzzleLevel` definition and are placed on board construction, before the deck starts
- Deck slot count for first playable level: one slot (shared deck) is simplest and sufficient for M011 — multi-slot is supported by the model but not exercised in the demo
