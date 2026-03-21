# S01: Foundation & Bootstrap — UAT

**Slice:** S01 — Foundation & Bootstrap
**Milestone:** M015 — Fastlane Distribution Pipeline

## Prerequisites

- macOS with Ruby ~> 3.0 installed
- Run from project root: `cd /path/to/simplegame`

## Setup

```bash
bundle install
```

Expected: resolves fastlane ~> 2.225 without errors.

## Test Cases

### UAT-01: Dry-run iOS register

```bash
bundle exec fastlane ios register dry_run:true
```

**Expected:**
- Exits 0
- Prints `[DRY RUN] Would: Register 'Puzzle Tap' (com.simplemagicstudios.puzzletap) on App Store Connect`
- No API calls made

### UAT-02: Dry-run iOS build

```bash
bundle exec fastlane ios build dry_run:true
```

**Expected:**
- Exits 0
- Prints current build number and next build number
- Prints `[DRY RUN] Would: Write build number ...`
- Prints `[DRY RUN] Would: Run Unity CLI to export Xcode project`
- Prints `[DRY RUN] Would: Build and sign .ipa`

### UAT-03: Dry-run Android build

```bash
bundle exec fastlane android build dry_run:true
```

**Expected:**
- Exits 0
- Prints current and next build number
- Dry-run logs for write_build_number and unity_build_android

### UAT-04: Android bootstrap instructions

```bash
bundle exec fastlane android bootstrap dry_run:true
```

**Expected:**
- Exits 0
- Prints all 6 manual setup steps clearly
- No confirmation prompt in dry-run mode

### UAT-05: Dry-run all remaining lanes

```bash
bundle exec fastlane ios certs dry_run:true
bundle exec fastlane ios beta dry_run:true
bundle exec fastlane ios metadata dry_run:true
bundle exec fastlane android beta dry_run:true
bundle exec fastlane android metadata dry_run:true
```

**Expected:** All exit 0 with `[DRY RUN] Would:` log lines.

### UAT-06: Status stub

```bash
bundle exec fastlane status
```

**Expected:**
- Exits 0
- Prints JSON to stdout (will show errors for missing API keys — that's expected)
- JSON structure present with `queried_at`, `build`, `ios`, `android` keys

### UAT-07: Config validation

```bash
ruby -e "require 'json'; %w[app.json testers.json build.json].each { |f| JSON.parse(File.read(\"fastlane/config/#{f}\")); puts \"OK: #{f}\" }"
```

**Expected:** Prints OK for all 3 files.

## Pass Criteria

All 7 UAT cases pass. No secrets visible in any output.
