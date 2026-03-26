# S01: Submodule & Package Registration

**Goal:** Add `simple-jigsaw` as an editable git submodule at `Packages/simple-jigsaw/`, register it as a local UPM package in `Packages/manifest.json`, and add URP alongside the existing built-in RP so the package's shaders can compile.

**Demo:** `git submodule status` shows the tracked commit. Unity resolves `com.simple-magic-studios.simple-jigsaw` with no compile errors. `SimpleJigsaw.Runtime` asmdef is visible to the project.

## Must-Haves

- `Packages/simple-jigsaw/` exists as a git submodule tracking `https://github.com/Simple-Magic-Studios/simple-jigsaw.git` at master HEAD
- `.gitmodules` contains the correct submodule entry
- `Packages/manifest.json` contains `"com.simple-magic-studios.simple-jigsaw": "file:simple-jigsaw/Assets/SimpleJigsaw"`
- `Packages/manifest.json` contains `"com.unity.render-pipelines.universal": "17.3.0"`
- No compile errors in the Unity project after the manifest change

## Tasks

- [x] **T01: Add submodule and register package**
  Add the git submodule, update manifest.json with the local path entry and URP, verify Unity can see the package types.

## Files Likely Touched

- `Packages/manifest.json`
- `.gitmodules` (created by git submodule add)
- `Packages/simple-jigsaw/` (new submodule directory)
