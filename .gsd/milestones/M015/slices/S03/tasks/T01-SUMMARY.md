---
id: T01
parent: S03
milestone: M015
provides:
  - Assets/Editor/BuildScript.cs — static BuildIOS() and BuildAndroid() methods for Unity CLI invocation
  - BuildScript.BuildIOS: reads -outputPath, uses EditorBuildSettings.scenes, BuildPipeline.BuildPlayer(iOS), exits 0 or 1
  - BuildScript.BuildAndroid: reads -outputPath, -keystoreName, -keystorePass, -keyaliasName, -keyaliasPass, enables AAB, exits 0 or 1
  - fastlane/Fastfile updated: executeMethod uses fully qualified SimpleGame.Editor.BuildScript.BuildIOS/BuildAndroid
key_files:
  - Assets/Editor/BuildScript.cs
  - Assets/Editor/BuildScript.cs.meta
  - fastlane/Fastfile (executeMethod fix)
key_decisions:
  - "Namespace SimpleGame.Editor — executeMethod must use fully qualified name"
  - "iOS outputPath is a directory (Xcode project dir); Android outputPath is a file (.aab)"
  - "Keystore args are optional — falls back to PlayerSettings if not provided"
  - "BuildAndroid enables buildAppBundle=true for AAB output"
  - "EditorApplication.Exit(0/1) for clean CLI exit after build"
patterns_established:
  - "GetArg() pattern: scan Environment.GetCommandLineArgs() for named CLI arguments"
  - "Build reports via BuildSummary — logs result, errors, warnings, output path, size"
duration: 20min
verification_result: pass (static — runtime requires Unity + macOS)
completed_at: 2026-03-21T21:00:00Z
---

# S03/T01: BuildScript Unity Editor Class

**BuildScript.cs created with BuildIOS() and BuildAndroid() static methods. Reads CLI args for output path and Android signing. Integrated with fastlane build lanes via -executeMethod.**

## What Happened

Created Assets/Editor/BuildScript.cs in namespace SimpleGame.Editor. Both methods use a shared GetArg() helper that scans Environment.GetCommandLineArgs() for named arguments — the same pattern Unity recommends for CLI builds. The Android method applies keystore settings from CLI args before building, falling back to PlayerSettings if not provided.

Fixed fastlane/Fastfile to use the fully qualified executeMethod names (`SimpleGame.Editor.BuildScript.BuildIOS`, `SimpleGame.Editor.BuildScript.BuildAndroid`) — without the namespace prefix Unity would fail to find the methods.

BuildAndroid enables `EditorUserBuildSettings.buildAppBundle = true` before building to produce .aab rather than .apk.

Both methods call `EditorApplication.Exit(0)` on success and `Exit(1)` on failure — required for fastlane's `sh()` to detect build failures.

## Deviations
None.

## Files Created/Modified
- `Assets/Editor/BuildScript.cs` — 160 lines, Unity CLI build entry points
- `Assets/Editor/BuildScript.cs.meta` — GUID meta file
- `fastlane/Fastfile` — executeMethod names fixed to use fully qualified class name
