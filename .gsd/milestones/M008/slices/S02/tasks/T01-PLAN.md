# T01: Add TMP to Asmdefs

**Slice:** S02
**Milestone:** M008

## Goal

Add Unity.TextMeshPro reference to SimpleGame.Game.asmdef and Unity.TextMeshPro + Unity.TextMeshPro.Editor to SimpleGame.Editor.asmdef so popup views and SceneSetup can use TMP types.

## Must-Haves

### Truths
- `SimpleGame.Game.asmdef` references `Unity.TextMeshPro`
- `SimpleGame.Editor.asmdef` references `Unity.TextMeshPro` and `Unity.TextMeshPro.Editor`
- No compiler errors after change
- 169 tests pass

### Artifacts
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — updated references array
- `Assets/Editor/SimpleGame.Editor.asmdef` — updated references array

## Steps

1. Read `Assets/Scripts/Game/SimpleGame.Game.asmdef` — add `"Unity.TextMeshPro"` to references array
2. Read `Assets/Editor/SimpleGame.Editor.asmdef` — add `"Unity.TextMeshPro"` and `"Unity.TextMeshPro.Editor"` to references array
3. Verify via read_console that no compiler errors appear

## Context
- `com.unity.ugui` 2.0.0 is already in Packages/manifest.json — TMP is bundled, assembly name is `Unity.TextMeshPro`
- The editor assembly is `Unity.TextMeshPro.Editor` (verified in Library/PackageCache)
- SimpleGame.Core already references LitMotion — no changes needed there
