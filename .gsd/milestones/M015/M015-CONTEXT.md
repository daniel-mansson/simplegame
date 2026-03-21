# M015: Fastlane Distribution Pipeline — Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

## Project Description

Puzzle Tap by Simple Magic Studios. Unity mobile jigsaw puzzle game targeting iOS and Android.
- Bundle IDs: `com.simplemagicstudios.puzzletap` (iOS), `com.SimpleMagicStudios.PuzzleTap` (Android)
- Current version: `0.1.0`, build number `10001` (encoding convention in `Docs/BUILD_NUMBERS.md`)
- No existing Fastlane setup. No `fastlane/` directory. No `tools/` directory.

## Why This Milestone

Manual distribution is a bottleneck. Every build requires clicking through App Store Connect and Google Play Console — setting up provisioning, uploading binaries, updating metadata. This milestone eliminates all of that with CLI lanes that handle the full lifecycle.

The goal is: someone new to the project runs one command to register the app, one command to build, and one command to distribute. Everything else is config files in source control.

## User-Visible Outcome

### When this milestone is complete:

- `fastlane ios register` registers the app on App Store Connect (no portal clicking)
- `fastlane ios certs` creates/fetches certificates and provisioning profiles
- `fastlane ios build` produces a signed .ipa from the Unity project
- `fastlane ios beta` uploads to TestFlight with testers from a config file
- `fastlane android build` produces a signed .aab from the Unity project
- `fastlane android beta` uploads to Play internal track
- `fastlane ios metadata` and `fastlane android metadata` push store text from version-controlled files
- `fastlane status` returns JSON-structured current state for both platforms
- Every lane accepts `dry_run:true` to validate intent without mutating real state
- Android bootstrap step is documented clearly: one manual first upload required (platform API limitation)

### Entry point / environment

- Entry point: `bundle exec fastlane <platform> <lane>` from project root
- Environment: local dev (macOS required for iOS builds); Android builds possible on any OS with Java/SDK
- Live dependencies: App Store Connect API (Apple), Google Play Developer API (Google)

## Completion Class

- Contract complete means: all lanes exist with real implementations, dry-run mode functional, config files present and documented
- Integration complete means: iOS build → TestFlight path exercised end-to-end with real API calls; Android build → Play internal track exercised end-to-end
- Operational complete means: status lane returns real data from both platforms

## Final Integrated Acceptance

- `fastlane ios build dry_run:true` runs without error and logs full intent
- `fastlane android build dry_run:true` runs without error and logs full intent
- `fastlane status` returns valid JSON with real data from App Store Connect and Google Play
- iOS build → TestFlight: .ipa produced, uploaded, visible in TestFlight
- Android build → Play internal: .aab produced, uploaded, visible in Play Console

## Two-Tier Folder Structure

This is a design constraint, not just a preference:

```
fastlane/           ← standard location; project-specific lanes
  Appfile
  Fastfile          ← platform lanes: ios, android
  Pluginfile
  config/           ← data files: app.json, testers.json, build.json
  metadata/         ← store metadata text files (iOS and Android)
    default/        ← en-US defaults
    android/
  Matchfile

tools/              ← higher-level orchestration; designed to be extracted
  fastlane/
    Fastfile        ← bootstrap lane, multi-project orchestration stubs
    lib/            ← shared Ruby helpers: dry_run, status formatting, build number math
```

The `tools/` layer imports helpers from `tools/fastlane/lib/`. Project lanes in `fastlane/` may call into `tools/fastlane/lib/` but not the reverse. This interface is the extract-later seam.

## Dry-Run Design

All lanes must support `dry_run:true`. Implementation pattern:
- A shared `with_dry_run(action_name, description)` helper in `tools/fastlane/lib/dry_run.rb`
- In dry-run mode, the helper logs `[DRY RUN] Would: <description>` and returns a mock value
- Lanes use this wrapper for every API call and file mutation
- Dry-run does not skip validation — it still checks that config files exist and are well-formed

## Build Number Convention

Defined in `Docs/BUILD_NUMBERS.md`. Encoding: `aabbccdd` where `aa`=major, `bb`=minor, `cc`=patch, `dd`=build counter (01–99). A lane reads current `bundleVersion` and `AndroidBundleVersionCode` from `ProjectSettings/ProjectSettings.asset`, increments the `dd` counter, and writes both back before building.

Current values: version `0.1.0`, bundle number `10001`.

## Apple API Constraints

- App Store Connect API key (key_id, issuer_id, .p8) is required for CI-safe auth
- `produce` (app creation) has **limited** API key support — may need Apple ID for initial registration only. This is acceptable; the lane documents when it falls back.
- `match`, `gym`, `pilot`, `deliver` all have full API key support

## Android API Constraints (Hard Platform Limit)

**Google Play cannot create new apps programmatically.** The Publisher API only manages existing apps. This means:
1. The bootstrap lane documents the required one-time manual step: create app in Play Console and upload first APK manually
2. After that first upload exits Draft status, all subsequent operations (uploads, metadata, track promotion) are fully automated via `upload_to_play_store`

The bootstrap lane for Android should print clear instructions and block with a confirmation prompt before proceeding.

## Authentication / Secrets

- Apple: `ASC_KEY_ID`, `ASC_ISSUER_ID`, `ASC_KEY_FILEPATH` (path to .p8 file, gitignored)
- Android: `GOOGLE_PLAY_KEY_FILE` (path to service account JSON, gitignored)
- Match: `MATCH_PASSWORD` (for cert repo encryption), `MATCH_GIT_URL` (cert repo URL)
- Android keystore: `ANDROID_KEYSTORE_PATH`, `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD`, `ANDROID_STORE_PASSWORD`
- Unity: `UNITY_PATH` (executable path, platform-specific)

All secrets sourced from environment or local `.env` file (gitignored). Never committed.

## Risks and Unknowns

- Unity CLI build flags differ between versions — the Unity executable path and `-buildTarget` flags must be verified against the installed Unity version
- `produce` API key support: may require fallback to Apple ID for app creation step. This is known and acceptable; document it clearly.
- Android keystore: if one doesn't exist, the build lane must create it or fail with clear guidance
- Match git repo: must be provisioned before `certs` lane runs. Bootstrap lane should verify or create it.

## Existing Codebase / Prior Art

- `Docs/BUILD_NUMBERS.md` — authoritative build number convention; lanes must follow it exactly
- `ProjectSettings/ProjectSettings.asset` — source of truth for version, bundle ID, build number
- `Assets/` — Unity project; build lane exports from project root
- No existing Fastlane, Gemfile, or tools/ — this milestone creates everything from scratch

## Scope

### In Scope

- Fastlane scaffold: Gemfile, Appfile, Fastfile, Matchfile, Pluginfile
- iOS: app registration (produce), certs (match), build (gym), TestFlight upload (pilot), metadata (deliver)
- Android: build (Unity CLI → AAB), Play upload (supply), metadata (supply)
- Status/query lanes with JSON output
- Dry-run mode across all lanes
- Config data files in fastlane/config/ (app.json, testers.json, build.json)
- Metadata text files in fastlane/metadata/ (description, keywords, release notes)
- tools/ orchestration layer with extract-later design

### Out of Scope / Non-Goals

- Screenshot upload (R132 — deferred)
- GitHub Actions CI wiring (R133 — deferred)
- Multi-project support beyond API design (R134 — deferred)
- App review submission or production promotion automation (R135 — out of scope)
- IAP or subscription management (R136 — out of scope)

## Open Questions

- Match git repo: where will it live? (user needs to provide URL; lane should prompt if not set)
- Apple ID for `produce` fallback: should the lane prompt interactively or require a separate env var? Current thinking: require `APPLE_ID` env var and document that it's only needed once for registration.
