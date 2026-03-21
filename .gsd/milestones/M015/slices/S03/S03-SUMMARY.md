---
id: S03
milestone: M015
provides:
  - Assets/Editor/BuildScript.cs — Unity CLI build entry points for iOS and Android
  - BuildScript.BuildIOS(): Xcode project export via BuildPipeline.BuildPlayer
  - BuildScript.BuildAndroid(): signed .aab via BuildPipeline.BuildPlayer with keystore CLI args
  - Build lanes dry-run verified: both log full intent correctly
  - BuildNumber.write() called before Unity invocation to stamp correct build number
key_files:
  - Assets/Editor/BuildScript.cs
  - fastlane/Fastfile (executeMethod fix)
key_decisions:
  - "Fully qualified executeMethod name required: SimpleGame.Editor.BuildScript.BuildIOS"
  - "iOS outputs Xcode project directory; Android outputs .aab file path"
  - "Keystore via CLI args for reproducible builds without PlayerSettings state"
drill_down_paths:
  - .gsd/milestones/M015/slices/S03/tasks/T01-SUMMARY.md
verification_result: pass (static — Unity runtime requires macOS)
completed_at: 2026-03-21T21:00:00Z
---

# S03: Unity Build Pipeline

**BuildScript.cs written with BuildIOS() and BuildAndroid() static methods. Fastlane build lanes wired to Unity CLI with correct -executeMethod names, signing args, and build number stamping before build.**

## What Was Built

Created the Unity editor entry point that Fastlane calls via `-executeMethod`. Both methods parse CLI args using `GetArg()`, read enabled scenes from `EditorBuildSettings.scenes`, and invoke `BuildPipeline.BuildPlayer`. Android sets keystore credentials from CLI args and enables AAB output. Both call `EditorApplication.Exit()` for clean CI exit codes.

Fixed a namespace issue in the Fastfile: executeMethod must use the fully qualified name `SimpleGame.Editor.BuildScript.BuildIOS`.

## Deviations
None.
