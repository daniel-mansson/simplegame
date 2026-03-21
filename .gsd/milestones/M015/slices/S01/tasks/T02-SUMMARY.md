---
id: T02
parent: S01
milestone: M015
provides:
  - tools/fastlane/lib/dry_run.rb — DryRun module with with_dry_run, require_env!, require_env_or_warn!
  - tools/fastlane/lib/build_number.rb — BuildNumber module with decode, encode, current, next_build, write, summary
  - tools/fastlane/lib/config.rb — Config module with load, load_app, load_build, load_testers, resolve_env
  - Build number decode/encode verified: 10001 → {major:0, minor:1, patch:0, counter:1}; encode(0,1,0,2) → 10002
  - ProjectSettings.asset regex parsing verified against real file
key_files:
  - tools/fastlane/lib/dry_run.rb
  - tools/fastlane/lib/build_number.rb
  - tools/fastlane/lib/config.rb
key_decisions:
  - "ProjectSettings.asset parsed with regex (not YAML) — Unity format is not valid YAML"
  - "write() updates both AndroidBundleVersionCode and buildNumber.iPhone with gsub"
  - "with_dry_run accepts keyword arg dry_run: for clean call sites"
  - "Validation (env var checks, config file existence) runs in dry-run mode — only API calls are skipped"
patterns_established:
  - "DryRun.with_dry_run pattern: wrap every API call and file mutation"
  - "DryRun.require_env_or_warn! for env vars needed only in live mode"
  - "BuildNumber module is the single source of truth for build number encoding"
duration: 20min
verification_result: pass
completed_at: 2026-03-21T20:00:00Z
---

# T02: tools/ Lib Layer — dry_run, build_number, config

**Three shared Ruby modules in tools/fastlane/lib/: DryRun (wrap/skip API calls), BuildNumber (read/write ProjectSettings.asset using aabbccdd encoding), Config (load/validate JSON config files).**

## What Happened

Implemented all three lib modules. Build number encoding verified in Python against the real ProjectSettings.asset values — decode(10001) correctly yields {major:0, minor:1, patch:0, counter:1}, encode(0,1,0,2) yields 10002. The ProjectSettings.asset regex approach (rather than YAML parsing) is correct because Unity's asset format uses custom tags that break standard YAML parsers.

The dry_run module separates concerns cleanly: `with_dry_run` skips the block and returns mock_return in dry mode; `require_env!` always raises (validation runs even in dry-run); `require_env_or_warn!` only warns in dry-run and raises in live mode.

## Deviations
None.

## Files Created/Modified
- `tools/fastlane/lib/dry_run.rb` — DryRun module
- `tools/fastlane/lib/build_number.rb` — BuildNumber module
- `tools/fastlane/lib/config.rb` — Config module
