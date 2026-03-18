---
id: S02
milestone: M010
provides:
  - Assets/Scenes/JigsawDemo.unity — standalone demo scene, orthographic camera, PuzzleDriver GO
  - Assets/JigsawDemo/PuzzleSceneDriver.cs — driver script (copied from submodule outside package scope)
  - Assets/JigsawDemo/DemoGridConfig.asset — 4×4 GridLayoutConfig with ClassicKnob edge profile
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset — PieceRenderConfig with correct PuzzlePiece shader GUID
  - Assets/Editor/JigsawDemo/JigsawDemoSetup.cs — re-generation editor script
  - packages-lock.json confirms simple-jigsaw resolved as local package
requires:
  - slice: S01
    provides: SimpleJigsaw.Runtime asmdef, submodule, URP
affects: []
key_files:
  - Assets/Scenes/JigsawDemo.unity
  - Assets/JigsawDemo/PuzzleSceneDriver.cs
  - Assets/JigsawDemo/DemoGridConfig.asset
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset
  - Packages/packages-lock.json
key_decisions:
  - "PuzzleSceneDriver.cs is outside the package root — must be copied to project for use"
  - "Package PieceRenderConfig.asset has stale shader GUID — project-local DemoPieceRenderConfig with correct GUID"
  - "Scene written as YAML directly — domain reload deferred by MCP connection"
patterns_established:
  - "For local UPM packages: scripts outside the package.json root directory are NOT exported — copy them to Assets/ for use"
  - "Stale asset GUIDs in packages: create project-local override .asset with correct GUID"
drill_down_paths:
  - .gsd/milestones/M010/slices/S02/tasks/T01-SUMMARY.md
duration: 45min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S02: Jigsaw Demo Scene

**JigsawDemo scene and assets created; PuzzleSceneDriver wired to 4×4 GridLayoutConfig and PieceRenderConfig with correct shader**

## What Happened

Created the standalone demo scene and all required assets. Key findings: `PuzzleSceneDriver.cs` is not inside the package root (lives at `Assets/Scenes/` in the source repo, outside `Assets/SimpleJigsaw/`) so it doesn't export with the package — copied to `Assets/JigsawDemo/`. The package's `PieceRenderConfig.asset` had a stale shader GUID from the original project; created a project-local `DemoPieceRenderConfig.asset` with the correct GUID. Unity resolved the local package and confirmed no compile errors (only nullable warnings from Clipper2).

## Verification

| Check | Result |
|---|---|
| Assets/Scenes/JigsawDemo.unity exists | ✓ PASS |
| Assets/JigsawDemo/PuzzleSceneDriver.cs exists | ✓ PASS |
| Assets/JigsawDemo/DemoGridConfig.asset exists (4×4, ClassicKnob) | ✓ PASS |
| Assets/JigsawDemo/DemoPieceRenderConfig.asset exists (correct shader GUID) | ✓ PASS |
| Scene wires PuzzleSceneDriver.Config → DemoGridConfig | ✓ PASS |
| Scene wires PuzzleSceneDriver.RenderConfig → DemoPieceRenderConfig | ✓ PASS |
| packages-lock.json shows simple-jigsaw resolved as local | ✓ PASS |
| No compile errors (only nullable warnings in Clipper2) | ✓ PASS |

**UAT required:** Open JigsawDemo.unity and press Play to visually confirm pieces render.

## Files Created/Modified

- `Assets/Scenes/JigsawDemo.unity` + `.meta`
- `Assets/JigsawDemo/PuzzleSceneDriver.cs` + `.meta`
- `Assets/JigsawDemo/DemoGridConfig.asset` + `.meta`
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` + `.meta`
- `Assets/Editor/JigsawDemo/JigsawDemoSetup.cs` + `.meta`
- `Assets/Editor/JigsawDemo/SimpleGame.Editor.JigsawDemo.asmdef` + `.meta`
- `Packages/packages-lock.json`
