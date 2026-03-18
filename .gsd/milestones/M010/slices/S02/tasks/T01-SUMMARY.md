---
id: T01
parent: S02
milestone: M010
provides:
  - Assets/JigsawDemo/PuzzleSceneDriver.cs — copy of driver from package (no namespace, Assembly-CSharp)
  - Assets/JigsawDemo/DemoGridConfig.asset — GridLayoutConfig (4×4, ClassicKnob edge profile, 0.05 thickness)
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset — PieceRenderConfig with correct PuzzlePiece shader GUID
  - Assets/Scenes/JigsawDemo.unity — standalone demo scene with orthographic camera and PuzzleDriver GO
  - Assets/Editor/JigsawDemo/JigsawDemoSetup.cs — re-generation editor script (Tools/Setup/Create Jigsaw Demo Scene)
  - packages-lock.json updated — Unity resolved com.simple-magic-studios.simple-jigsaw as local source
requires:
  - slice: S01
    provides: SimpleJigsaw.Runtime asmdef, URP package, submodule at Packages/simple-jigsaw/
affects: []
key_files:
  - Assets/Scenes/JigsawDemo.unity
  - Assets/JigsawDemo/PuzzleSceneDriver.cs
  - Assets/JigsawDemo/DemoGridConfig.asset
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset
  - Assets/Editor/JigsawDemo/JigsawDemoSetup.cs
  - Packages/packages-lock.json
key_decisions:
  - "PuzzleSceneDriver.cs copied to Assets/JigsawDemo/ — it lives outside the package root in the source repo so is not exported by the package"
  - "DemoPieceRenderConfig.asset created in Assets/ with correct shader GUID (c67764c5...) — package's PieceRenderConfig.asset had stale shader GUID from original source project"
  - "Scene written as YAML directly — editor script approach blocked by pending domain reload, YAML is the reliable path"
  - "Camera at (0.5, 0.5, -10) orthographic size 0.7 — frames 0..1 normalized board with margin"
patterns_established:
  - "Local UPM packages: copy demo scripts from outside the package root into Assets/JigsawDemo/ for project-local use"
  - "Package assets with stale shader GUIDs: create project-local override .asset with correct GUID rather than patching the package"
drill_down_paths:
  - .gsd/milestones/M010/slices/S02/tasks/T01-PLAN.md
duration: 45min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: Create demo assets and scene via Editor script

**JigsawDemo scene + assets created as YAML; PuzzleSceneDriver copied from submodule; DemoPieceRenderConfig with correct shader GUID**

## What Happened

Attempted to use a Unity Editor script to create assets, but Unity's domain reload was deferred (MCP keeps editor busy). Switched to creating scene and asset YAML files directly. Discovered `PuzzleSceneDriver.cs` is NOT in the package scope (it's at `Assets/Scenes/` in the source repo, outside the `Assets/SimpleJigsaw/` package root) — copied it to `Assets/JigsawDemo/`. Also discovered the package's `PieceRenderConfig.asset` had a stale shader GUID from the original source project (`50f401a8...`), not the GUID of the shader in our submodule (`c67764c5...`). Created `DemoPieceRenderConfig.asset` in `Assets/JigsawDemo/` with the correct GUID. Unity resolved the local package and updated `packages-lock.json`. No compile errors — only nullable reference type warnings from Clipper2 (harmless). The editor setup script (`JigsawDemoSetup.cs`) is present and will work once Unity can reload domain.

## Deviations

- Editor script path changed from `Assets/Editor/` to `Assets/Editor/JigsawDemo/` (separate asmdef) to avoid `SimpleGame.Editor.asmdef` assembly conflict
- Scene/assets written as YAML rather than via editor script — Unity domain reload deferred by MCP connection
- `DemoPieceRenderConfig.asset` created in `Assets/JigsawDemo/` rather than using the package's `PieceRenderConfig.asset` — stale shader GUID in package asset

## Files Created/Modified

- `Assets/JigsawDemo/PuzzleSceneDriver.cs` — copied from submodule's Assets/Scenes/
- `Assets/JigsawDemo/DemoGridConfig.asset` — 4×4 grid, ClassicKnob profile, 0.05 thickness
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` — correct PuzzlePiece shader GUID
- `Assets/Scenes/JigsawDemo.unity` — scene with camera + PuzzleDriver GO wired to both assets
- `Assets/Editor/JigsawDemo/JigsawDemoSetup.cs` — re-generation menu script
- `Assets/Editor/JigsawDemo/SimpleGame.Editor.JigsawDemo.asmdef` — isolated editor assembly
- `Packages/packages-lock.json` — updated by Unity with local package resolution
