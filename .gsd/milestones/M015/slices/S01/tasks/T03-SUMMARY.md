---
id: T03
parent: S01
milestone: M015
provides:
  - fastlane/config/app.json — 18-key app config with env var name indirection for all secrets
  - fastlane/config/testers.json — tester groups structure with empty email arrays (ready to fill)
  - fastlane/config/build.json — 16-key build environment config with Unity path and keystore env var refs
  - fastlane/metadata/default/ — 6 iOS metadata text files (name, subtitle, description, keywords, support_url, release_notes)
  - fastlane/metadata/android/en-US/ — 4 Android metadata text files (title, short_description, full_description, release_notes)
  - .gitignore additions — *.p8, *.keystore, *.jks, play-store-credentials.json, build/ios/, build/android/, .env.local
key_files:
  - fastlane/config/app.json
  - fastlane/config/testers.json
  - fastlane/config/build.json
  - fastlane/metadata/default/name.txt
  - fastlane/metadata/android/en-US/title.txt
key_decisions:
  - "Config JSON stores env var NAMES not values — config.rb resolve_env() reads the actual value at runtime"
  - "Metadata uses fastlane/metadata/default/ for iOS (deliver convention) and fastlane/metadata/android/en-US/ for Android (supply convention)"
  - "Gemfile.lock explicitly NOT in .gitignore — committed per Fastlane convention"
patterns_established:
  - "All secret references in config files use *_env suffix keys pointing to env var names"
duration: 15min
verification_result: pass
completed_at: 2026-03-21T20:00:00Z
---

# T03: Config Data Files and Metadata Structure

**All version-controlled data files created: 3 JSON config files (validated), 10 metadata text files within iOS and Android size limits, .gitignore updated to exclude secrets and build artifacts.**

## What Happened

JSON files validated with Python json.parse — all 3 parse cleanly. Metadata text files verified against platform constraints (iOS name ≤30 chars, subtitle ≤30 chars, keywords ≤100 chars; Android short description ≤80 chars). All pass. The config design uses env var name indirection — config.json stores the NAME of the env var (e.g. "ASC_KEY_ID"), not the value, so no secret ever appears in a committed file.

## Deviations
None.

## Files Created/Modified
- `fastlane/config/app.json` — app configuration
- `fastlane/config/testers.json` — tester groups
- `fastlane/config/build.json` — build environment config
- `fastlane/metadata/default/*.txt` — 6 iOS metadata files
- `fastlane/metadata/android/en-US/*.txt` — 4 Android metadata files
- `.gitignore` — appended secret and artifact exclusions
