---
id: S01
milestone: M010
provides:
  - git submodule at Packages/simple-jigsaw/ tracking https://github.com/Simple-Magic-Studios/simple-jigsaw.git
  - com.simple-magic-studios.simple-jigsaw registered as local UPM package (file:simple-jigsaw/Assets/SimpleJigsaw)
  - com.unity.render-pipelines.universal 17.3.0 added to manifest
  - SimpleJigsaw.Runtime asmdef (autoReferenced:true) accessible to the project
  - BoardFactory, PieceObjectFactory, GridLayoutConfig, PieceRenderConfig, PuzzleSceneDriver types available
requires: []
affects: [S02]
key_files:
  - .gitmodules
  - Packages/manifest.json
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/SimpleJigsaw.Runtime.asmdef
key_decisions:
  - "Local UPM path points to Assets/SimpleJigsaw subdirectory inside repo (not repo root) — package.json lives there"
  - "URP 17.3.0 added without creating a render pipeline asset — existing game scenes on built-in RP unaffected"
  - "Submodule in Packages/ directory — editable and pushable upstream"
patterns_established:
  - "git submodule add <url> Packages/<name> pattern for editable UPM packages from external repos"
drill_down_paths:
  - .gsd/milestones/M010/slices/S01/tasks/T01-SUMMARY.md
duration: 10min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S01: Submodule & Package Registration

**simple-jigsaw added as editable git submodule; registered as local UPM package with URP alongside built-in RP**

## What Happened

Single task. Added the git submodule at `Packages/simple-jigsaw/` pointing at the upstream repo. Updated `Packages/manifest.json` with the local path entry (`file:simple-jigsaw/Assets/SimpleJigsaw`) and URP 17.3.0. The `package.json` inside the submodule has `name: com.simple-magic-studios.simple-jigsaw` and `SimpleJigsaw.Runtime.asmdef` is `autoReferenced:true` — no manual assembly references needed anywhere. URP was added as a package only; no render pipeline asset was created, so existing game scenes remain on built-in RP.

## Verification

| Check | Result |
|---|---|
| `git submodule status` shows Packages/simple-jigsaw with commit hash | ✓ PASS — 0b7f7b3 |
| .gitmodules entry correct | ✓ PASS |
| manifest.json local path entry present | ✓ PASS |
| manifest.json URP 17.3.0 entry present | ✓ PASS |
| package.json exists in submodule | ✓ PASS |
| SimpleJigsaw.Runtime.asmdef exists | ✓ PASS |

## Files Created/Modified

- `.gitmodules` — submodule registration
- `Packages/manifest.json` — two new dependency entries
- `Packages/simple-jigsaw/` — populated submodule
