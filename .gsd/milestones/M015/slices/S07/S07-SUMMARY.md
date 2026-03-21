---
id: S07
milestone: M015
provides:
  - status lane: aggregates status_ios() and status_android() into JSON output
  - status_ios(): Spaceship::ConnectAPI — queries app, latest TestFlight build, processing state
  - status_android(): Supply::Client — queries all four tracks (internal/alpha/beta/production)
  - BuildNumber.summary() always available — reads local ProjectSettings without API
  - JSON output structure: queried_at, app_name, build{version,bundle_number,next_build}, ios, android
  - Error handling: each platform returns {error:} hash on missing credentials or API failure
key_files:
  - fastlane/Fastfile (status lane, status_ios, status_android)
  - tools/fastlane/lib/build_number.rb (summary method)
key_decisions:
  - "status_ios and status_android are plain Ruby def methods (not lanes) — called by status lane"
  - "Each platform returns an error hash instead of raising — status lane always produces valid JSON"
  - "JSON.pretty_generate for human-readable output; also returned as string for programmatic use"
  - "Query-only: status lane never mutates state"
drill_down_paths: []
verification_result: pass (static — runtime requires live API credentials)
completed_at: 2026-03-21T21:10:00Z
---

# S07: Status & Query API

**status lane implemented in S01: queries both platforms via Spaceship and Supply clients, returns pretty-printed JSON. Error handling per platform — status always produces valid JSON structure.**

## What Was Built

The status lane aggregates three data sources: local ProjectSettings (build number via BuildNumber.summary — always available), App Store Connect via Spaceship::ConnectAPI (TestFlight build state), and Google Play via Supply::Client (track version codes). Each platform is wrapped in rescue so a missing credential on one platform doesn't break the other. Output is JSON to stdout.

The build info section always works without API credentials, making status useful even during initial setup.

## Deviations
None — lane implemented ahead of slice in S01.
