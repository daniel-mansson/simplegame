# M015: Fastlane Distribution Pipeline

**Vision:** A complete Fastlane-based CLI pipeline that handles the full app distribution lifecycle for Puzzle Tap — from registering a brand new app on App Store Connect to uploading builds to TestFlight and Google Play. All configuration is data-driven from version-controlled files. Every lane supports dry-run mode. A status query API returns structured JSON for external consumption.

## Success Criteria

- `fastlane ios register dry_run:true` completes without error and logs intent
- `fastlane ios certs` fetches or creates certs and provisioning profiles via match
- `fastlane ios build` produces a signed .ipa with correct build number
- `fastlane ios beta` uploads to TestFlight and adds testers from config
- `fastlane android build` produces a signed .aab with correct build number
- `fastlane android beta` uploads .aab to Play internal track
- `fastlane ios metadata` and `fastlane android metadata` push store text from files
- `fastlane status` returns valid JSON covering both platforms
- All lanes exit 0 on `dry_run:true` without touching real APIs
- No secrets are committed; all auth is env-var or gitignored-file driven

## Key Risks / Unknowns

- `produce` (iOS app creation) has limited App Store Connect API key support — may need Apple ID fallback for the registration step only
- Unity CLI build flags are version-specific — must be verified against installed Unity version at implementation time
- Match git repo must be provisioned before the certs lane runs — bootstrap must verify or fail with clear guidance
- Android keystore may not exist — build lane must detect and guide creation

## Proof Strategy

- `produce` API key limitation → retire in S01 by implementing registration lane and testing dry-run; document Apple ID fallback requirement explicitly
- Unity CLI flags → retire in S03 by running a real Unity CLI build and confirming .ipa/.aab output
- Android no-API-create constraint → retire in S01 by implementing the documented manual step flow with clear lane output

## Verification Classes

- Contract verification: dry-run exit 0, config file existence checks, Ruby syntax clean
- Integration verification: real Unity CLI build produces .ipa / .aab; real API upload visible in TestFlight / Play Console
- Operational verification: status lane returns real JSON data from live APIs
- UAT / human verification: user confirms TestFlight build appears for testers; Play internal track shows uploaded build

## Milestone Definition of Done

This milestone is complete only when all are true:

- All 7 slices are complete with real implementations (no stub lanes)
- Dry-run mode works across all lanes without real API calls
- iOS build → TestFlight end-to-end exercised with real binaries
- Android build → Play internal track end-to-end exercised with real binaries
- `fastlane status` returns valid JSON from both live platforms
- All config is in version-controlled files; no hardcoded values in Fastfile
- No secrets committed to repo

## Requirement Coverage

- Covers: R119, R120, R121, R122, R123, R124, R125, R126, R127, R128, R129, R130, R131
- Partially covers: none
- Leaves for later: R132 (screenshots), R133 (CI), R134 (multi-project)
- Orphan risks: Android app creation (R120 — hard platform limit, documented)

## Slices

- [x] **S01: Foundation & Bootstrap** `risk:high` `depends:[]`
  > After this: `fastlane ios register dry_run:true` and `fastlane android bootstrap` run without error; folder structure, Gemfile, Appfile, config files, and tools/ scaffold all exist; dry-run helper wired

- [x] **S02: Cert & Provisioning** `risk:high` `depends:[S01]`
  > After this: `fastlane ios certs` fetches or creates development + appstore certs and provisioning profiles via match with API key auth; dry-run mode logs intent cleanly

- [x] **S03: Unity Build Pipeline** `risk:high` `depends:[S01]`
  > After this: `fastlane ios build` and `fastlane android build` produce real .ipa and .aab from the Unity project with correct build numbers written to ProjectSettings; dry-run logs full build intent

- [x] **S04: iOS Distribution** `risk:medium` `depends:[S02,S03]`
  > After this: `fastlane ios beta` uploads a real .ipa to TestFlight and adds testers from fastlane/config/testers.json; build is visible in TestFlight

- [x] **S05: Android Distribution** `risk:medium` `depends:[S03]`
  > After this: `fastlane android beta` uploads a real .aab to the Play internal track; build is visible in Play Console

- [x] **S06: Metadata Management** `risk:low` `depends:[S04,S05]`
  > After this: `fastlane ios metadata` and `fastlane android metadata` push app name, description, keywords, and release notes from fastlane/metadata/ to both stores without portal interaction

- [x] **S07: Status & Query API** `risk:low` `depends:[S04,S05]`
  > After this: `fastlane status` returns a JSON document with build version, latest TestFlight build + status, Play track versions, provisioning expiry — consumable by external scripts

## Boundary Map

### S01 → All slices

Produces:
- `fastlane/` scaffold: Gemfile, Gemfile.lock, Appfile, Fastfile (platform shells), Matchfile, Pluginfile
- `tools/fastlane/lib/dry_run.rb` — `with_dry_run(name, desc, &block)` helper
- `tools/fastlane/lib/build_number.rb` — `current_build_number()`, `next_build_number()`, `write_build_number(n)` reading/writing ProjectSettings.asset
- `tools/fastlane/lib/config.rb` — `load_config(path)` reading fastlane/config/app.json
- `fastlane/config/app.json` — bundle IDs, app name, SKU, team ID, language
- `fastlane/config/testers.json` — external tester groups and emails
- `fastlane/config/build.json` — Unity path, output directories, keystore config keys
- `tools/fastlane/Fastfile` — `bootstrap_ios`, `bootstrap_android` lanes (with dry-run)

Consumes: nothing (first slice)

### S02 → S04

Produces:
- `fastlane/Matchfile` — git_url, type, app_identifier, api_key config
- `ios_certs` lane — runs `match(type: "appstore")` and `match(type: "development")`; dry-run aware
- Provisioning profiles and certificates in match git repo (real state after non-dry-run)

Consumes from S01:
- `with_dry_run` helper
- `fastlane/config/app.json` → bundle ID, team ID
- App Store Connect API key env vars

### S03 → S04, S05

Produces:
- `ios_build` lane — Unity CLI → Xcode project → `gym` → .ipa; writes build number before build
- `android_build` lane — Unity CLI → .aab; writes build number before build
- `tools/fastlane/lib/build_number.rb` (implemented) — reads/writes `ProjectSettings.asset`
- Build artifacts at stable output paths: `build/ios/PuzzleTap.ipa`, `build/android/PuzzleTap.aab`

Consumes from S01:
- `with_dry_run` helper
- `build_number.rb`
- `fastlane/config/build.json` → Unity path, output dirs

Consumes from S02 (iOS only):
- Provisioning profile and signing identity (via match in lane)

### S04 → S07

Produces:
- `ios_beta` lane — `pilot` upload; tester management from testers.json; release notes from metadata
- TestFlight build visible (real) or intent logged (dry-run)

Consumes from S01:
- `with_dry_run`, `fastlane/config/testers.json`

Consumes from S02:
- API key, signing identity

Consumes from S03:
- `build/ios/PuzzleTap.ipa`

### S05 → S07

Produces:
- `android_beta` lane — `upload_to_play_store(track: "internal")`; release notes from metadata
- Play internal track build visible (real) or intent logged (dry-run)

Consumes from S01:
- `with_dry_run`, `fastlane/config/app.json`

Consumes from S03:
- `build/android/PuzzleTap.aab`

### S06 → (standalone)

Produces:
- `fastlane/metadata/default/` — name.txt, subtitle.txt, description.txt, keywords.txt, support_url.txt, release_notes.txt
- `fastlane/metadata/android/en-US/` — title.txt, short_description.txt, full_description.txt, release_notes.txt
- `ios_metadata` lane — `deliver(skip_binary_upload: true, skip_screenshots: true)`
- `android_metadata` lane — `upload_to_play_store(skip_upload_apk: true, skip_upload_aab: true)`

Consumes from S01:
- `with_dry_run`, `fastlane/config/app.json`

### S07 → (standalone)

Produces:
- `status_ios` lane — queries App Store Connect: app version, TestFlight latest build + status, provisioning expiry; returns Hash
- `status_android` lane — queries Play: track versions for internal/alpha/beta/production; returns Hash
- `status` lane — aggregates both, prints JSON to stdout

Consumes from S04:
- App Store Connect API key (already configured)

Consumes from S05:
- Google Play service account (already configured)
