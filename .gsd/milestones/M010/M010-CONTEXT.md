# M010: Simple Jigsaw Package Integration — Context

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

A Unity 6 (6000.3.10f1) mobile puzzle game. The game has a complete flow skeleton (main menu, meta world, gameplay stub, win/lose). This milestone introduces the actual puzzle rendering library (`simple-jigsaw`) as an editable submodule so real jigsaw gameplay can be built on top of it.

## Why This Milestone

The core gameplay currently uses a stub (counter + buttons). `simple-jigsaw` is the puzzle generation library that will power real gameplay. It needs to land in the project as a proper editable package — not a copy, not a one-time import — so it can be iterated on in parallel with the game. A demo scene proves the pipeline works end-to-end before any gameplay integration begins.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Open `Assets/Scenes/JigsawDemo.unity`, hit Play, and see a generated jigsaw puzzle rendered on screen with correctly shaped piece meshes and puzzle image UV-mapped onto the faces
- Navigate to `Packages/simple-jigsaw/`, edit library source files, and have Unity pick up the changes (submodule is live, not a snapshot)
- Push changes to `https://github.com/Simple-Magic-Studios/simple-jigsaw.git` from this project

### Entry point / environment

- Entry point: `Assets/Scenes/JigsawDemo.unity` → Play Mode
- Environment: Unity Editor, local dev
- Live dependencies involved: none (all generation is runtime/local)

## Completion Class

- Contract complete means: package resolves without errors, demo scene generates a board in Play Mode
- Integration complete means: `PuzzleSceneDriver` runs, pieces spawn as GameObjects, shader renders correctly with URP renderer
- Operational complete means: submodule is properly tracked in `.gitmodules` and can be updated/pushed

## Final Integrated Acceptance

- Hit Play in `JigsawDemo` scene → pieces appear at solved positions with jigsaw edge shapes and a texture
- `git submodule status` shows the correct commit for `Packages/simple-jigsaw`
- No compile errors or missing-script warnings in the project

## Risks and Unknowns

- URP dependency: `simple-jigsaw` shaders require `com.unity.render-pipelines.universal`; this project uses built-in RP. Resolution: add URP to manifest, but scope it to the demo scene only (do not migrate existing game scenes to URP). The existing game uses UGUI/Canvas — it will be visually unaffected by URP being present but not active.
- Package root path: `package.json` lives at `Assets/SimpleJigsaw/package.json` in the source repo (not at root). Unity local path reference must point to `Packages/simple-jigsaw/Assets/SimpleJigsaw` — not the repo root.
- Config assets (GridLayoutConfig, PieceRenderConfig) are ScriptableObjects — they must be created in `Assets/` not inside the package itself (package is read-only from Unity's perspective for assets).

## Existing Codebase / Prior Art

- `Packages/manifest.json` — current package manifest; add URP + local path entry here
- `Assets/Scenes/` — existing game scenes (Boot, MainMenu, Settings, InGame); do NOT modify these
- `Assets/Scripts/Game/Boot/SceneSetup.cs` — existing scene setup; do NOT add JigsawDemo to EditorBuildSettings unless explicitly asked
- `https://github.com/Simple-Magic-Studios/simple-jigsaw.git` — the library repo; master branch is current (v1.1.0 tagged)

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- (none from existing REQUIREMENTS.md directly map here — this milestone establishes new capability outside the existing game flow)

## Scope

### In Scope

- Git submodule at `Packages/simple-jigsaw/` tracking the upstream repo
- Local path entry in `Packages/manifest.json` pointing to `Packages/simple-jigsaw/Assets/SimpleJigsaw`
- URP added to `Packages/manifest.json` (package only, no render pipeline asset created for existing game scenes)
- `Assets/Scenes/JigsawDemo.unity` — standalone scene, not in EditorBuildSettings, not wired to game flow
- `GridLayoutConfig` ScriptableObject asset in `Assets/Data/` (or `Assets/JigsawDemo/`) for the demo
- `PieceRenderConfig` ScriptableObject asset for the demo
- A placeholder `Texture2D` asset in `Assets/JigsawDemo/` for the puzzle image
- `PuzzleSceneDriver` component (from the package) wired in the demo scene

### Out of Scope / Non-Goals

- Integration with existing game flow (Boot, MainMenu, InGame, GameBootstrapper)
- Real gameplay interaction (drag, snap, completion detection)
- Migrating existing game scenes to URP
- Any new C# scripts in `Assets/Scripts/` for this milestone

## Technical Constraints

- Unity 6 (6000.3.10f1) — package format and local path syntax applies
- `simple-jigsaw` asmdef is `autoReferenced: true` — no manual assembly reference needed in the demo scene driver
- The demo scene's `PuzzleSceneDriver` MonoBehaviour lives in the package itself (at `Assets/SimpleJigsaw/Assets/Scenes/PuzzleSceneDriver.cs` relative to the repo, resolved as part of the package)
- DO NOT add `JigsawDemo` to `EditorBuildSettings` — it is a standalone dev scene

## Integration Points

- `Packages/manifest.json` — local path + URP dependency entries
- `.gitmodules` — submodule tracking
- `Assets/Scenes/JigsawDemo.unity` — new scene, self-contained

## Open Questions

- None — all decisions made during discussion.
