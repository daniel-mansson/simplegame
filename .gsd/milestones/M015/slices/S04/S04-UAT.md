# S04–S07: Distribution, Metadata, Status — UAT

## Prerequisites

- macOS, `bundle install` complete, all env vars set
- Real .ipa produced by `fastlane ios build`
- Real .aab produced by `fastlane android build`

## S04: iOS Distribution

### UAT-S04-01: Dry-run beta

```bash
bundle exec fastlane ios beta dry_run:true
```

**Expected:** Exits 0. DRY RUN logs for pilot upload and any tester group additions.

### UAT-S04-02 (live): Real TestFlight upload

```bash
bundle exec fastlane ios beta
```

**Expected:** Build visible in App Store Connect → TestFlight within ~30 minutes of processing.

## S05: Android Distribution

### UAT-S05-01: Dry-run beta

```bash
bundle exec fastlane android beta dry_run:true
bundle exec fastlane android beta track:alpha dry_run:true
```

**Expected:** Exits 0. DRY RUN log shows track name and package.

### UAT-S05-02 (live): Real Play upload

```bash
bundle exec fastlane android beta
```

**Expected:** Build visible in Play Console → Internal Testing.

## S06: Metadata

### UAT-S06-01: Dry-run metadata

```bash
bundle exec fastlane ios metadata dry_run:true
bundle exec fastlane android metadata dry_run:true
```

**Expected:** Both exit 0. DRY RUN logs reference metadata path.

## S07: Status

### UAT-S07-01: Status output

```bash
bundle exec fastlane status
```

**Expected:**
- Exits 0
- Prints JSON to stdout
- JSON has keys: `queried_at`, `app_name`, `build`, `ios`, `android`
- `build.version` is `"0.1.0"`
- `build.bundle_number` is `10001` (or current after builds)
- Platform sections show data or `{ "error": "..." }` (not crash)
