# S01: Foundation & Bootstrap

**Goal:** Establish the complete Fastlane scaffold and two-tier folder structure. Implement the dry-run helper, build number library, config loader, and bootstrap lanes for both platforms. After this slice all downstream slices have their shared infrastructure.

**Demo:** `bundle exec fastlane ios register dry_run:true` runs without error and logs full intent. `bundle exec fastlane android bootstrap` prints the manual Play Console instructions and halts cleanly. All config files exist and are documented.

## Must-Haves

- `bundle exec fastlane` resolves (Gemfile + Gemfile.lock present, `bundle install` clean)
- `fastlane/Appfile` contains bundle IDs, team ID, Apple ID placeholders
- `fastlane/Fastfile` has `ios` and `android` platform shells with stub lanes for all 7 planned lanes
- `fastlane/Matchfile` present with placeholder config
- `fastlane/config/app.json`, `testers.json`, `build.json` exist and are documented
- `tools/fastlane/lib/dry_run.rb` — `with_dry_run` helper; tested via `ruby -e`
- `tools/fastlane/lib/build_number.rb` — reads/writes `ProjectSettings/ProjectSettings.asset`; correctly decodes/encodes the `aabbccdd` convention
- `tools/fastlane/lib/config.rb` — `load_config(path)` with validation
- `tools/fastlane/Fastfile` — `bootstrap_ios` and `bootstrap_android` lanes
- `fastlane ios register dry_run:true` exits 0 and logs intent (no real API calls)
- `fastlane android bootstrap` prints clear manual step instructions
- Ruby syntax clean: `ruby -c` passes on all `.rb` and `Fastfile` files
- `.gitignore` updated to exclude secrets and build artifacts

## Tasks

- [x] **T01: Gemfile, Appfile, Matchfile scaffold**
  Core Ruby/Fastlane project files. Gemfile pins fastlane version. Appfile sets app identifiers. Matchfile stubs cert repo config.

- [x] **T02: tools/ lib layer — dry_run, build_number, config**
  Three shared Ruby helpers. dry_run.rb wraps API calls. build_number.rb reads/writes ProjectSettings.asset using the aabbccdd encoding. config.rb loads and validates JSON config files.

- [x] **T03: Config data files and metadata structure**
  fastlane/config/app.json, testers.json, build.json with all documented fields. fastlane/metadata/ directory tree with placeholder text files. .gitignore additions.

- [x] **T04: Fastfile — platform shells, bootstrap lanes, and dry-run wiring**
  fastlane/Fastfile with ios/android platform blocks and all lane stubs. tools/fastlane/Fastfile with bootstrap_ios and bootstrap_android. Wire dry_run helper. Verify `fastlane ios register dry_run:true` exits 0.

## Files Likely Touched

- `Gemfile`
- `Gemfile.lock` (generated)
- `fastlane/Appfile`
- `fastlane/Fastfile`
- `fastlane/Matchfile`
- `fastlane/Pluginfile`
- `fastlane/config/app.json`
- `fastlane/config/testers.json`
- `fastlane/config/build.json`
- `fastlane/metadata/default/*.txt`
- `fastlane/metadata/android/en-US/*.txt`
- `tools/fastlane/Fastfile`
- `tools/fastlane/lib/dry_run.rb`
- `tools/fastlane/lib/build_number.rb`
- `tools/fastlane/lib/config.rb`
- `.gitignore`
