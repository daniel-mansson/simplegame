# S02: Jigsaw Demo Scene — UAT

## What to verify

1. Open `Assets/Scenes/JigsawDemo.unity` in Unity Editor
2. Press Play
3. After a brief generation pause, a 4×4 grid of puzzle pieces should appear in the Game View
4. Each piece should have a jigsaw-shaped mesh (tab/blank edges, not flat rectangles)
5. The Regenerate button (top-left of Game View) should work — clicking it rebuilds the puzzle with the same seed

## Expected visual

- 16 pieces arranged in a solved grid
- Each piece has curved tab/blank edge shapes from the ClassicKnob profile
- Pieces rendered with default white color (no texture assigned)
- Piece outlines visible (OutlineEnabled = true, white, width 3)
- Dark background (0.15, 0.15, 0.18)

## Pass criteria

- Pieces appear with non-rectangular jigsaw shapes
- No pink/magenta materials (would indicate missing shader)
- No error messages in Console related to BoardFactory, PieceObjectFactory, or PuzzleSceneDriver
- Regenerate button functions

## Notes

- URP must be the active renderer for this scene for the shader to work. If pieces appear pink, open Window → Rendering → Render Pipeline Converter and ensure URP is active for this scene (or set a URP renderer asset on the camera).
- The `PuzzleSceneDriver` Config inspector field shows DemoGridConfig, RenderConfig shows DemoPieceRenderConfig — verify in Inspector while scene is open.
