# T03: Config Data Files and Metadata Structure

**Slice:** S01
**Milestone:** M015

## Goal
Create all version-controlled data files: fastlane/config/ JSON files, fastlane/metadata/ text file tree, and .gitignore additions to exclude secrets and build artifacts.

## Must-Haves

### Truths
- `fastlane/config/app.json` parses as valid JSON and contains all required keys (documented inline)
- `fastlane/config/testers.json` parses as valid JSON with groups array structure
- `fastlane/config/build.json` parses as valid JSON with unity_path, output dirs, keystore keys
- `fastlane/metadata/default/` contains: name.txt, subtitle.txt, description.txt, keywords.txt, support_url.txt, release_notes.txt
- `fastlane/metadata/android/en-US/` contains: title.txt, short_description.txt, full_description.txt, release_notes.txt
- `.gitignore` excludes: `*.p8`, `*.keystore`, `*.jks`, `play-store-credentials.json`, `fastlane/report.xml`, `build/`, `.env.local`, `AuthKey_*.p8`
- No secrets appear in any committed file

### Artifacts
- `fastlane/config/app.json` — structured app config
- `fastlane/config/testers.json` — tester group config
- `fastlane/config/build.json` — build environment config
- `fastlane/metadata/default/*.txt` — 6 iOS metadata files with Puzzle Tap content
- `fastlane/metadata/android/en-US/*.txt` — 4 Android metadata files
- `.gitignore` additions (appended to existing)

### Key Links
- `fastlane/config/app.json` → read by `tools/fastlane/lib/config.rb` → consumed by all lanes
- `fastlane/metadata/` → consumed by S06 deliver/supply lanes
- `.gitignore` → prevents secrets from being committed

## Steps
1. Write `fastlane/config/app.json` with keys: `ios_bundle_id`, `android_package_name`, `app_name`, `sku`, `primary_language`, `apple_team_id_env` (name of env var), `asc_key_id_env`, `asc_issuer_id_env`, `asc_key_filepath_env`
2. Write `fastlane/config/testers.json` with structure: `{"groups": [{"name": "Internal Testers", "emails": []}]}`
3. Write `fastlane/config/build.json` with keys: `unity_path_env`, `ios_output_dir`, `android_output_dir`, `xcode_project_dir`, `android_keystore_path_env`, `android_key_alias_env`, `android_key_password_env`, `android_store_password_env`
4. Write all 6 `fastlane/metadata/default/*.txt` files with real Puzzle Tap content (name, subtitle, description, keywords, support URL placeholder, release notes)
5. Write all 4 `fastlane/metadata/android/en-US/*.txt` files with equivalent Android content
6. Append secrets and build artifact patterns to `.gitignore`
7. Verify JSON validity: `ruby -e "require 'json'; JSON.parse(File.read('fastlane/config/app.json'))"` for each config file

## Context
- Config JSON uses env var name strings (e.g. `"asc_key_id_env": "ASC_KEY_ID"`) so the lane knows which env var to read — never stores the value itself
- iOS metadata `name.txt` max 30 chars; `subtitle.txt` max 30 chars; `keywords.txt` comma-separated max 100 chars total
- Android `short_description.txt` max 80 chars; `full_description.txt` max 4000 chars
- Release notes should mention current version 0.1.0 and be placeholder text for now
- App name: "Puzzle Tap"; Company: "Simple Magic Studios"
