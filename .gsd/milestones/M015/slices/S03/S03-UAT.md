# S03: Unity Build Pipeline — UAT

## Prerequisites

- macOS, Unity 6000.0.x, `bundle install` complete
- `UNITY_PATH` set to correct Unity executable

## Test Cases

### UAT-01: iOS dry-run build

```bash
bundle exec fastlane ios build dry_run:true
```

**Expected:**
- Exits 0
- Prints current and next build numbers
- `[DRY RUN] Would: Write build number ...`
- `[DRY RUN] Would: Run Unity CLI to export Xcode project to build/ios`
- `[DRY RUN] Would: Build and sign .ipa from ...`

### UAT-02: Android dry-run build

```bash
bundle exec fastlane android build dry_run:true
```

**Expected:** Same structure for Android.

### UAT-03 (live, macOS only): Real iOS Unity build

```bash
export UNITY_PATH=/Applications/Unity/Hub/Editor/6000.0.30f1/Unity.app/Contents/MacOS/Unity
bundle exec fastlane ios build
```

**Expected:** `build/ios/Unity-iPhone.xcodeproj` created. Build number in ProjectSettings incremented.

### UAT-04 (live, macOS only): Real Android Unity build

```bash
export ANDROID_KEYSTORE_PATH=/path/to/puzzletap.keystore
export ANDROID_KEY_ALIAS=puzzletap
export ANDROID_KEY_PASSWORD=...
export ANDROID_STORE_PASSWORD=...
bundle exec fastlane android build
```

**Expected:** `build/android/PuzzleTap.aab` created.
