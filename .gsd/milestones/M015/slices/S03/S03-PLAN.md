# S03: Unity Build Pipeline

**Goal:** Create a Unity `BuildScript` editor class that exports iOS and Android builds via CLI. The fastlane build lanes invoke Unity with `-executeMethod BuildScript.BuildIOS` and `-executeMethod BuildScript.BuildAndroid`. Build number is written to ProjectSettings.asset before Unity runs.

**Demo:** `fastlane ios build dry_run:true` logs full intent. `fastlane android build dry_run:true` logs full intent. BuildScript.cs exists and handles CLI args for output path and signing.

## Must-Haves

- `Assets/Editor/BuildScript.cs` exists with `BuildIOS()` and `BuildAndroid()` static methods
- BuildScript reads `-outputPath` from command-line args
- BuildScript reads keystore settings from `-keystoreName`, `-keystorePass`, `-keyaliasName`, `-keyaliasPass` args (Android)
- BuildScript uses `BuildPipeline.BuildPlayer` with the correct scenes from `EditorBuildSettings.scenes`
- iOS output: Xcode project directory (BuildTarget.iOS)
- Android output: `.aab` file (BuildTarget.Android, build app bundle enabled)
- `fastlane ios build dry_run:true` exits 0 and logs full intent
- `fastlane android build dry_run:true` exits 0 and logs full intent

## Tasks

- [ ] **T01: BuildScript Unity editor class**
  Create Assets/Editor/BuildScript.cs with BuildIOS() and BuildAndroid() static methods. Command-line arg parsing for outputPath and signing. BuildPipeline.BuildPlayer call with correct options.

## Files Likely Touched

- `Assets/Editor/BuildScript.cs` (new)
- `Assets/Editor/BuildScript.cs.meta` (new, auto-generated or manual)
- `fastlane/Fastfile` (build lane — verify Unity CLI args match BuildScript expectations)
