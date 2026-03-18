---
id: M010
provides:
  - Packages/simple-jigsaw/ — git submodule tracking https://github.com/Simple-Magic-Studios/simple-jigsaw.git (commit 0b7f7b3)
  - com.simple-magic-studios.simple-jigsaw registered as local UPM package (file:simple-jigsaw/Assets/SimpleJigsaw)
  - com.unity.render-pipelines.universal 17.3.0 added to manifest alongside existing built-in RP
  - SimpleJigsaw.Runtime asmdef (autoReferenced:true) compiling with only nullable warnings (Clipper2)
  - Assets/Scenes/JigsawDemo.unity — standalone demo scene wired to 4×4 puzzle config
  - Assets/JigsawDemo/PuzzleSceneDriver.cs — driver script (outside package root, copied to project)
  - Assets/JigsawDemo/DemoGridConfig.asset — 4×4 GridLayoutConfig, ClassicKnob profile
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset — PieceRenderConfig with correct PuzzlePiece shader GUID
key_files:
  - .gitmodules
  - Packages/manifest.json
  - Packages/packages-lock.json
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json
  - Assets/Scenes/JigsawDemo.unity
  - Assets/JigsawDemo/DemoGridConfig.asset
  - Assets/JigsawDemo/DemoPieceRenderConfig.asset
  - Assets/JigsawDemo/PuzzleSceneDriver.cs
key_decisions:
  - "D057: simple-jigsaw as git submodule — editable and pushable"
  - "D058: Local UPM path points to Assets/SimpleJigsaw subdirectory — package.json lives there not at repo root"
  - "D059: URP added alongside built-in RP — no default render pipeline asset change"
  - "D060: JigsawDemo scene not in EditorBuildSettings"
  - "PuzzleSceneDriver.cs is outside package root — must be copied to project for use"
  - "Package PieceRenderConfig.asset had stale shader GUID — project-local override with correct GUID"
completed_slices:
  - S01: Submodule & Package Registration
  - S02: Jigsaw Demo Scene
---

# M010: Simple Jigsaw Package Integration

**simple-jigsaw added as editable git submodule, registered as local UPM package, standalone demo scene ready**

## Summary

Two slices. S01 added the git submodule and updated the manifest with the local path reference and URP. S02 created the demo scene, GridLayoutConfig, PieceRenderConfig, and copied `PuzzleSceneDriver.cs` from the submodule (it lives outside the package root and is not exported by the package). Unity resolved the local package without issues; `SimpleJigsaw.Runtime` compiles with only nullable reference type warnings in Clipper2 (harmless, upstream issue).

## What Works Now

- `Packages/simple-jigsaw/` is an editable, pushable git submodule
- `com.simple-magic-studios.simple-jigsaw` resolves in Unity Package Manager as a local package
- `Assets/Scenes/JigsawDemo.unity` opens cleanly; Press Play to generate a 4×4 jigsaw puzzle
- URP is present for shader compilation without affecting existing game scenes

## UAT Gate

Open `Assets/Scenes/JigsawDemo.unity` → Press Play → confirm 16 pieces appear with jigsaw edge shapes (see S02-UAT.md for full criteria). If pieces appear pink, a URP renderer asset needs to be assigned on the camera.

## Gotchas for Future Work

- `PuzzleSceneDriver.cs` and other scripts in `Assets/Scenes/` and `Assets/Textures/` in the source repo are NOT part of the package — they're project-local demo code that won't be included when the package is referenced
- The package's `Runtime/Configs/PieceRenderConfig.asset` has a stale shader GUID — always use `Assets/JigsawDemo/DemoPieceRenderConfig.asset` or a project-local override
- `git submodule update --remote Packages/simple-jigsaw` updates to latest master; commit the resulting submodule pointer change
