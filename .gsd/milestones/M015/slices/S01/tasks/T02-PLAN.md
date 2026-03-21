# T02: tools/ Lib Layer — dry_run, build_number, config

**Slice:** S01
**Milestone:** M015

## Goal
Implement three shared Ruby helpers in `tools/fastlane/lib/` that all lanes depend on: dry-run wrapper, build number reader/writer (following the aabbccdd convention exactly), and config file loader.

## Must-Haves

### Truths
- `ruby -c tools/fastlane/lib/dry_run.rb` exits 0
- `ruby -c tools/fastlane/lib/build_number.rb` exits 0
- `ruby -c tools/fastlane/lib/config.rb` exits 0
- `build_number.rb` correctly decodes `10001` → version `0.1.0`, counter `1`
- `build_number.rb` correctly encodes version `0.1.0`, counter `2` → `10002`
- `build_number.rb` reads `AndroidBundleVersionCode` from `ProjectSettings/ProjectSettings.asset`
- `build_number.rb` reads `bundleVersion` from `ProjectSettings/ProjectSettings.asset`
- `with_dry_run` in dry-run mode logs `[DRY RUN] Would: <description>` and returns mock value without executing block
- `with_dry_run` in live mode executes the block and returns its result
- `load_config` raises a clear error if the JSON file does not exist or is malformed

### Artifacts
- `tools/fastlane/lib/dry_run.rb` — module `DryRun` with `with_dry_run(dry_run:, name:, description:, mock_return: nil, &block)`
- `tools/fastlane/lib/build_number.rb` — module `BuildNumber` with `current(project_root:)`, `next_build(project_root:)`, `write(project_root:, bundle_number:)`
- `tools/fastlane/lib/config.rb` — module `Config` with `load(path:)` returning parsed Hash
- `tools/fastlane/lib/.gitkeep` or README so the dir is tracked

### Key Links
- `build_number.rb` reads `ProjectSettings/ProjectSettings.asset` — YAML-like Unity format, must use regex not YAML parser (Unity format is not valid YAML)
- `dry_run.rb` → imported by `fastlane/Fastfile` and `tools/fastlane/Fastfile` via `load` or `require`

## Steps
1. Write `tools/fastlane/lib/dry_run.rb` — `DryRun` module with `with_dry_run`
2. Write `tools/fastlane/lib/build_number.rb` — `BuildNumber` module; parse `bundleVersion` and `AndroidBundleVersionCode` with regex; implement encode/decode matching `Docs/BUILD_NUMBERS.md` exactly
3. Write `tools/fastlane/lib/config.rb` — `Config` module; `JSON.parse(File.read(path))` with existence check and rescue
4. Inline-test build_number decode: `ruby -e "require_relative 'tools/fastlane/lib/build_number'; puts BuildNumber.decode(10001).inspect"` — verify `{major:0, minor:1, patch:0, counter:1}`
5. Inline-test encode: verify `BuildNumber.encode(0,1,0,2)` == `10002`
6. Inline-test dry_run: verify block not called when `dry_run: true`
7. Run `ruby -c` on all three files

## Context
- Unity `ProjectSettings.asset` is not valid YAML — it uses Unity-specific tags. Parse with regex: `bundleVersion: (\S+)` and `AndroidBundleVersionCode: (\d+)`
- Write back with `gsub` replacing the matched line — do not attempt to round-trip YAML
- Build number encoding per `Docs/BUILD_NUMBERS.md`: `aa*1_000_000 + bb*10_000 + cc*100 + dd`
- Current: bundleVersion=`0.1.0`, AndroidBundleVersionCode=`10001`
- `with_dry_run` should accept keyword arg `dry_run:` so callers can pass `dry_run: options[:dry_run]`
