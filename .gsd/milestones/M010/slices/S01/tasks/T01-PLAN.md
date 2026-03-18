# T01: Add submodule and register package

**Slice:** S01
**Milestone:** M010

## Goal

Add `simple-jigsaw` as a git submodule at `Packages/simple-jigsaw/`, register it as a local UPM package, and add URP to the manifest so the package shaders can compile.

## Must-Haves

### Truths
- `git submodule status` shows `Packages/simple-jigsaw` with a commit hash
- `Packages/manifest.json` contains the local path entry for `com.simple-magic-studios.simple-jigsaw`
- `Packages/manifest.json` contains `com.unity.render-pipelines.universal` at version `17.3.0`
- `.gitmodules` contains the submodule entry pointing to `https://github.com/Simple-Magic-Studios/simple-jigsaw.git`
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json` exists (submodule populated)
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/SimpleJigsaw.Runtime.asmdef` exists

### Artifacts
- `Packages/simple-jigsaw/` — populated submodule directory
- `.gitmodules` — submodule registration
- `Packages/manifest.json` — updated with two new entries

### Key Links
- `Packages/manifest.json` `file:simple-jigsaw/Assets/SimpleJigsaw` → `Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json`

## Steps
1. Run `git submodule add https://github.com/Simple-Magic-Studios/simple-jigsaw.git Packages/simple-jigsaw`
2. Confirm `.gitmodules` was created and submodule populated
3. Read current `Packages/manifest.json`
4. Add `"com.simple-magic-studios.simple-jigsaw": "file:simple-jigsaw/Assets/SimpleJigsaw"` to the dependencies
5. Add `"com.unity.render-pipelines.universal": "17.3.0"` to the dependencies
6. Verify `Packages/simple-jigsaw/Assets/SimpleJigsaw/package.json` exists with correct name
7. Verify `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/SimpleJigsaw.Runtime.asmdef` exists
8. Commit: submodule add + manifest update

## Context
- The `package.json` in the source repo lives at `Assets/SimpleJigsaw/package.json` — NOT at the repo root. The local path in manifest.json must point to the subdirectory, not the repo root.
- URP version `17.3.0` matches what the simple-jigsaw source project uses. Unity 6000.3.x ships with this version family.
- Do NOT create any URP render pipeline asset or modify GraphicsSettings — just add the package to the manifest.
- The submodule goes in `Packages/` (Unity's package directory) not `Assets/` — this is intentional for UPM local packages.
