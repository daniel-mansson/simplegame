# S02: Jigsaw Demo Scene

**Goal:** Create a standalone `Assets/Scenes/JigsawDemo.unity` scene with a `PuzzleSceneDriver` component wired to a `GridLayoutConfig` and `PieceRenderConfig`, proving the full generation pipeline works in Play Mode.

**Demo:** Open `Assets/Scenes/JigsawDemo.unity` and press Play — a grid of puzzle pieces appears at solved positions with correct jigsaw edge shapes and a texture applied.

## Must-Haves

- `Assets/JigsawDemo/` folder exists with demo assets
- `Assets/JigsawDemo/DemoGridConfig.asset` — GridLayoutConfig (4×4, ClassicKnob edge profile)
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` — PieceRenderConfig with PuzzlePiece shader assigned
- `Assets/Scenes/JigsawDemo.unity` — scene with a Camera and a GameObject carrying `PuzzleSceneDriver`
- `PuzzleSceneDriver.Config` field wired to `DemoGridConfig`
- `PuzzleSceneDriver.RenderConfig` field wired to `DemoPieceRenderConfig`
- No compile errors; scene opens cleanly in Unity Editor

## Tasks

- [ ] **T01: Create demo assets and scene via Editor script**
  Write an Editor script that creates all required ScriptableObject assets and the JigsawDemo scene, trigger it via Unity MCP.

## Files Likely Touched

- `Assets/Editor/JigsawDemoSetup.cs` (new, deleted after use or kept for re-generation)
- `Assets/JigsawDemo/DemoGridConfig.asset` (new)
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` (new)
- `Assets/Scenes/JigsawDemo.unity` (new)
