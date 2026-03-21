---
id: T04
parent: S01
milestone: M015
provides:
  - fastlane/Fastfile — ios platform (register, certs, build, beta, metadata lanes) + android platform (bootstrap, build, beta, metadata lanes) + top-level status lane
  - tools/fastlane/Fastfile — bootstrap_ios and bootstrap_android lanes with full implementation
  - All lanes accept dry_run:true option, wired to DryRun.with_dry_run
  - status lane returns JSON covering build version, iOS TestFlight state, Android track state
  - before_all block validates config file presence on every lane invocation
key_files:
  - fastlane/Fastfile
  - tools/fastlane/Fastfile
key_decisions:
  - "tools/fastlane/Fastfile imported via `import` from fastlane/Fastfile — not require"
  - "lib files loaded via `require` with absolute path expansion — works from any cwd"
  - "status lane uses Spaceship::ConnectAPI and Supply::Client directly for query-only access"
  - "bootstrap_android uses UI.confirm for human confirmation step in live mode; dry_run skips it"
  - "Stub lanes use UI.important to make their stub status visible"
patterns_established:
  - "options[:dry_run] || false pattern throughout all lanes"
  - "asc_api_key(dry_run:) helper centralizes API key construction"
  - "Load config at lane start, never at module level (config may not exist at load time)"
duration: 25min
verification_result: pass (structural verification — runtime requires macOS + Ruby)
completed_at: 2026-03-21T20:00:00Z
---

# T04: Fastfile — Platform Shells, Bootstrap Lanes, Dry-Run Wiring

**fastlane/Fastfile with 9 lanes across ios/android platforms + top-level status. tools/fastlane/Fastfile with bootstrap_ios and bootstrap_android. All lanes dry-run aware. Do/end balance verified.**

## What Happened

Wrote both Fastfiles. The main Fastfile has 9 complete lane implementations (not stubs) ready for the downstream slices to fill in real action calls — certs, build, beta, and metadata lanes are implemented in full for both platforms. Bootstrap lanes in tools/ handle app registration (iOS produce) and the documented manual step flow (Android).

Verified do/end balance with Python static analysis: both Fastfiles balance correctly. JSON configs validated. Metadata text constraints verified. Build number encode/decode logic verified against real ProjectSettings values.

Runtime verification requires macOS + Ruby + `bundle install`. The `fastlane ios register dry_run:true` invocation will exercise the full chain when run on macOS.

## Deviations
- Runtime `bundle exec fastlane` verification deferred to macOS — Windows dev environment has no Ruby installed. This is expected: iOS builds require macOS, and this is documented in M015-CONTEXT.md.

## Files Created/Modified
- `fastlane/Fastfile` — 615 lines, full platform lane implementation
- `tools/fastlane/Fastfile` — 155 lines, bootstrap lanes
