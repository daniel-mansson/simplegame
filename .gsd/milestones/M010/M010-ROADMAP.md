# M010: Simple Jigsaw Package Integration

**Vision:** Add `simple-jigsaw` as an editable git submodule, register it as a local Unity package, and prove the puzzle generation pipeline works end-to-end in a standalone demo scene.

## Success Criteria

- `git submodule status` shows `Packages/simple-jigsaw` tracking the upstream repo at a known commit
- `Packages/manifest.json` resolves `com.simple-magic-studios.simple-jigsaw` as a local path package with no import errors
- Opening `Assets/Scenes/JigsawDemo.unity` and pressing Play spawns a grid of puzzle pieces with correct jigsaw edge shapes and the puzzle image UV-mapped onto piece faces
- No compile errors introduced by adding URP alongside the existing built-in RP game

## Key Risks / Unknowns

- URP alongside built-in RP — adding the package is safe, but Unity may warn about render pipeline mismatches in existing scenes if URP is detected but no URP asset is set as default
- Package root path — `package.json` lives at `Assets/SimpleJigsaw/` inside the repo, not at repo root; local path must point there specifically

## Proof Strategy

- URP alongside built-in RP → retire in S01 by confirming zero compile errors after manifest change and confirming existing Boot/MainMenu scenes open without missing-shader warnings
- Package root path → retire in S01 by confirming Unity resolves `com.simple-magic-studios.simple-jigsaw` in Package Manager with correct displayName

## Verification Classes

- Contract verification: package appears in Package Manager, no compile errors, submodule tracked in `.gitmodules`
- Integration verification: Play Mode in JigsawDemo scene produces piece GameObjects with non-null meshes and a material
- Operational verification: `git submodule update --remote` can be run without error
- UAT / human verification: visual confirmation that puzzle pieces render with correct shapes and texture

## Milestone Definition of Done

This milestone is complete only when all are true:

- S01 complete: submodule added, package resolves, URP present, zero compile errors
- S02 complete: JigsawDemo scene opens and generates a visible puzzle in Play Mode
- `git submodule status` shows correct commit hash
- No missing-script or missing-shader warnings in existing game scenes

## Requirement Coverage

- Covers: new puzzle rendering capability (not mapped to existing REQUIREMENTS.md entries)
- Partially covers: none
- Leaves for later: gameplay integration (drag, snap, completion detection)
- Orphan risks: none

## Slices

- [ ] **S01: Submodule & Package Registration** `risk:medium` `depends:[]`
  > After this: Unity resolves `com.simple-magic-studios.simple-jigsaw` as a local editable package with correct displayName and no compile errors; `git submodule status` shows the tracked commit.

- [ ] **S02: Jigsaw Demo Scene** `risk:low` `depends:[S01]`
  > After this: Open `Assets/Scenes/JigsawDemo.unity` and press Play — a grid of puzzle pieces appears with jigsaw edge shapes and a texture applied to each piece face.

## Boundary Map

### S01 → S02

Produces:
- `Packages/simple-jigsaw/` — git submodule, editable, tracked at master HEAD
- `Packages/manifest.json` entry: `"com.simple-magic-studios.simple-jigsaw": "file:simple-jigsaw/Assets/SimpleJigsaw"`
- `Packages/manifest.json` entry: `"com.unity.render-pipelines.universal": "17.0.3"` (or compatible Unity 6 version)
- `SimpleJigsaw.Runtime` asmdef visible and auto-referenced
- `PuzzleSceneDriver`, `BoardFactory`, `PieceObjectFactory`, `GridLayoutConfig`, `PieceRenderConfig` types accessible

Consumes:
- nothing (first slice)

### S02 → (future gameplay integration)

Produces:
- `Assets/Scenes/JigsawDemo.unity` — standalone demo scene
- `Assets/JigsawDemo/DemoGridConfig.asset` — GridLayoutConfig ScriptableObject
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` — PieceRenderConfig ScriptableObject
- `Assets/JigsawDemo/PuzzleImage.png` (or placeholder texture)
- Proof that `BoardFactory.Generate(config, seed)` + `PieceObjectFactory.CreateAll()` pipeline works in this project

Consumes from S01:
- `SimpleJigsaw.Runtime` types (BoardFactory, PieceObjectFactory, GridLayoutConfig, PieceRenderConfig, PuzzleSceneDriver)
- URP package (shader compilation)
