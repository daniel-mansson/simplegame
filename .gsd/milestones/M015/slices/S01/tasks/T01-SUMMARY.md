---
id: T01
parent: S01
milestone: M015
provides:
  - Gemfile pinning fastlane ~> 2.225, ruby ~> 3.0
  - fastlane/Appfile with iOS/Android identifiers and env-var-based secrets
  - fastlane/Matchfile with placeholder env-var config for git_url, api_key
  - fastlane/Pluginfile (minimal source block)
key_files:
  - Gemfile
  - fastlane/Appfile
  - fastlane/Matchfile
  - fastlane/Pluginfile
key_decisions:
  - "All secrets in Appfile/Matchfile reference env vars by name — never hardcode values"
  - "Matchfile uses api_key_path for App Store Connect API key auth (no 2FA)"
  - "Gemfile.lock should be committed (Fastlane convention)"
patterns_established:
  - "Env var reference pattern: ENV['VAR_NAME'] || fallback — used throughout all config files"
duration: 10min
verification_result: pass
completed_at: 2026-03-21T20:00:00Z
---

# T01: Gemfile, Appfile, Matchfile Scaffold

**Core Fastlane project files created: Gemfile (fastlane ~> 2.225), Appfile (iOS/Android identifiers via env vars), Matchfile (match git repo and API key config), Pluginfile.**

## What Happened

Created the four foundational files that let `bundle exec fastlane` resolve. All secret values (team ID, Apple ID, ASC key, match git URL) are read from environment variables — nothing is hardcoded. The Appfile uses `for_platform` blocks to separate iOS and Android concerns cleanly. The Matchfile is pre-wired for API key auth to avoid 2FA in CI.

## Deviations
None.

## Files Created/Modified
- `Gemfile` — Ruby gem manifest, pins fastlane and ruby version
- `fastlane/Appfile` — app identifier configuration with env-var refs
- `fastlane/Matchfile` — codesigning configuration with match git repo env vars
- `fastlane/Pluginfile` — empty plugin Gemfile (required by fastlane)
