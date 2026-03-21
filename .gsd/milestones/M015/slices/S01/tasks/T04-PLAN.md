# T04: Fastfile — Platform Shells, Bootstrap Lanes, Dry-Run Wiring

**Slice:** S01
**Milestone:** M015

## Goal
Write fastlane/Fastfile with ios/android platform blocks containing all planned lane stubs, and tools/fastlane/Fastfile with bootstrap_ios and bootstrap_android lanes. Wire the dry_run helper throughout. Verify the full invocation chain exits 0 in dry-run mode.

## Must-Haves

### Truths
- `bundle exec fastlane ios register dry_run:true` exits 0 and prints `[DRY RUN] Would: Register app on App Store Connect`
- `bundle exec fastlane android bootstrap` prints the manual Play Console instructions clearly and exits 0
- `bundle exec fastlane ios build dry_run:true` exits 0 (stub lane, dry-run aware)
- `bundle exec fastlane android build dry_run:true` exits 0 (stub lane, dry-run aware)
- `bundle exec fastlane ios certs dry_run:true` exits 0 (stub)
- `bundle exec fastlane ios beta dry_run:true` exits 0 (stub)
- `bundle exec fastlane android beta dry_run:true` exits 0 (stub)
- `bundle exec fastlane ios metadata dry_run:true` exits 0 (stub)
- `bundle exec fastlane android metadata dry_run:true` exits 0 (stub)
- `bundle exec fastlane status` exits 0 (stub, returns placeholder JSON)
- `ruby -c fastlane/Fastfile` exits 0
- `ruby -c tools/fastlane/Fastfile` exits 0

### Artifacts
- `fastlane/Fastfile` — `before_all` block loading lib helpers; `ios` platform with lanes: `register`, `certs`, `build`, `beta`, `metadata`; `android` platform with lanes: `bootstrap`, `build`, `beta`, `metadata`; top-level `status` lane
- `tools/fastlane/Fastfile` — `bootstrap_ios(options)` and `bootstrap_android(options)` lanes with full implementation (not stubs)

### Key Links
- `fastlane/Fastfile` → `load File.expand_path("../../tools/fastlane/lib/dry_run.rb", __dir__)` (and build_number, config)
- `fastlane/Fastfile` → calls `bootstrap_ios`/`bootstrap_android` defined in `tools/fastlane/Fastfile` via `import`
- `bootstrap_ios` → uses `with_dry_run` → calls `produce` action (dry-run: logs intent)
- `bootstrap_android` → prints instructions, prompts confirmation, exits

## Steps
1. Write `fastlane/Fastfile` — `before_all` loads all three lib files; ios platform block with 5 stub lanes + dry_run option; android platform block with 4 stub lanes + dry_run; top-level `status` stub
2. Write `tools/fastlane/Fastfile` — import in Fastfile via `import`; `bootstrap_ios` lane: load config, call `with_dry_run` wrapping `produce` action; `bootstrap_android` lane: print step-by-step manual instructions with UI.important + UI.message, ask for confirmation
3. Add `import "../../tools/fastlane/Fastfile"` at top of `fastlane/Fastfile`
4. Run `bundle exec fastlane ios register dry_run:true` — verify exit 0 and DRY RUN log
5. Run `bundle exec fastlane android bootstrap` — verify instructions print and exit 0
6. Run the remaining stub lanes with `dry_run:true` — verify all exit 0
7. Run `ruby -c` on both Fastfiles

## Context
- Fastlane `import` loads a Fastfile relative to the calling Fastfile; `load` loads a plain Ruby file
- `produce` requires: `app_identifier`, `app_name`, `language`, `app_version`, `sku` — all sourced from config
- `bootstrap_android` instructions should cover: (1) go to play.google.com/console, (2) create app, (3) upload first APK to internal track, (4) exit draft, (5) grant service account access, (6) run `fastlane android build` + `fastlane android beta`
- Stub lanes should `UI.important("Stub: implement in S02-S07")` so it's visible they're not done yet
- `status` stub should return and print `{"status": "stub", "note": "Implement in S07"}` as JSON
- Lane options are passed as `options[:dry_run]` in Fastlane — document this pattern in comments
