# T01: Create demo assets and scene via Editor script

**Slice:** S02
**Milestone:** M010

## Goal

Write an Editor script that creates the JigsawDemo ScriptableObject assets and scene, execute it via Unity MCP, then commit the resulting assets.

## Must-Haves

### Truths
- `Assets/JigsawDemo/DemoGridConfig.asset` exists with Rows=4, Columns=4, EdgeProfile set to ClassicKnob
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` exists with PieceShader assigned to `SimpleJigsaw/PuzzlePiece`
- `Assets/Scenes/JigsawDemo.unity` exists with a Main Camera and a PuzzleDriver GameObject carrying PuzzleSceneDriver
- PuzzleSceneDriver.Config → DemoGridConfig, PuzzleSceneDriver.RenderConfig → DemoPieceRenderConfig
- No compile errors in Console after import

### Artifacts
- `Assets/Editor/JigsawDemoSetup.cs` — Editor script (kept for re-generation)
- `Assets/JigsawDemo/DemoGridConfig.asset` — GridLayoutConfig
- `Assets/JigsawDemo/DemoPieceRenderConfig.asset` — PieceRenderConfig
- `Assets/Scenes/JigsawDemo.unity` — demo scene

### Key Links
- PuzzleSceneDriver.Config → DemoGridConfig.asset (via SerializeField)
- PuzzleSceneDriver.RenderConfig → DemoPieceRenderConfig.asset (via SerializeField)
- DemoGridConfig.EdgeProfile → ClassicKnobProfile.asset from package

## Steps
1. Write `Assets/Editor/JigsawDemoSetup.cs` — creates assets and scene
2. Trigger via Unity MCP execute_menu_item or run_editor_function
3. Verify assets created in correct paths
4. Verify scene opens cleanly
5. Commit all new assets

## Context
- ClassicKnobProfile.asset lives at `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Configs/ClassicKnobProfile.asset` — load via AssetDatabase.LoadAssetAtPath
- PuzzlePiece shader: `Shader.Find("SimpleJigsaw/PuzzlePiece")`
- PuzzleSceneDriver is in the package asmdef which is autoReferenced:true — accessible from Editor assembly
- The scene should NOT be added to EditorBuildSettings
- Use a top-level empty GameObject named "PuzzleDriver" to hold PuzzleSceneDriver; it acts as the parent transform for piece GameObjects
- Add a Camera with reasonable position (z=-10, orthographic, size=5) for a 4×4 grid that fits ~4 units wide
