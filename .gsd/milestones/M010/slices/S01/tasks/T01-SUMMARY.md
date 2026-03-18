---
id: T01
parent: S01
milestone: M010
provides:
  - git submodule at Packages/simple-jigsaw/ tracking upstream at commit 0b7f7b3
  - .gitmodules with submodule entry for https://github.com/Simple-Magic-Studios/simple-jigsaw.git
  - Packages/manifest.json entry: com.simple-magic-studios.simple-jigsaw → file:simple-jigsaw/Assets/SimpleJigsaw
  - Packages/manifest.json entry: com.unity.render-pipelines.universal → 17.3.0
  - SimpleJigsaw.Runtime asmdef present and accessible at Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/
requires: []
affects: [S02]
key_files:
  - .gitmodules
  - Packages/manifest.json
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/SimpleJigsaw.Runtime.asmdef
key_decisions:
  - "Local path points to Assets/SimpleJigsaw subdirectory, not repo root — package.json lives there not at repo root"
  - "URP 17.3.0 matches what simple-jigsaw source project uses; no render pipeline asset created"
patterns_established:
  - "Submodule in Packages/ directory — editable, pushable upstream"
drill_down_paths:
  - .gsd/milestones/M010/slices/S01/tasks/T01-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: Add submodule and register package

**Git submodule added at Packages/simple-jigsaw/ with local UPM path entry and URP 17.3.0 in manifest**

## What Happened

Added `simple-jigsaw` as a git submodule at `Packages/simple-jigsaw/` via `git submodule add`. The submodule tracks master HEAD at commit `0b7f7b3` (v1.1-104). Updated `Packages/manifest.json` with two new entries: the local path reference pointing at `file:simple-jigsaw/Assets/SimpleJigsaw` (where `package.json` lives inside the repo) and URP `17.3.0` (same version the simple-jigsaw source project uses). Both `package.json` and `SimpleJigsaw.Runtime.asmdef` confirmed present in the submodule.

## Deviations

None.

## Files Created/Modified

- `.gitmodules` — created by git submodule add; contains path + URL for Packages/simple-jigsaw
- `Packages/manifest.json` — added two dependency entries
- `Packages/simple-jigsaw/` — submodule directory (160000 gitlink entry)
