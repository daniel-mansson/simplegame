---
id: S01
milestone: M015
provides:
  - Complete Fastlane scaffold (Gemfile, Appfile, Matchfile, Pluginfile)
  - Two-tier folder structure: fastlane/ (project-specific) + tools/ (extractable orchestration)
  - DryRun module: with_dry_run wraps every API call and file mutation; validation runs in all modes
  - BuildNumber module: decode/encode aabbccdd convention; reads/writes ProjectSettings.asset
  - Config module: load/validate JSON config files; resolve_env for secret indirection
  - All 9 lanes implemented (register, certs, build, beta, metadata × 2 platforms + status)
  - bootstrap_ios and bootstrap_android lanes in tools/
  - JSON config data files: app.json, testers.json, build.json
  - Metadata text files: 6 iOS (default/) + 4 Android (en-US/)
  - .gitignore updated for secrets and build artifacts
key_files:
  - Gemfile
  - fastlane/Fastfile
  - fastlane/Appfile
  - fastlane/Matchfile
  - fastlane/config/app.json
  - fastlane/config/build.json
  - fastlane/config/testers.json
  - tools/fastlane/Fastfile
  - tools/fastlane/lib/dry_run.rb
  - tools/fastlane/lib/build_number.rb
  - tools/fastlane/lib/config.rb
key_decisions:
  - "Two-tier structure enforced: fastlane/ calls into tools/fastlane/lib/ but not reverse"
  - "All secrets reference env var names in config — never values. Config.resolve_env() reads at runtime"
  - "Dry-run mode: validation always runs; only API calls and file mutations are skipped"
  - "Build number: regex parse ProjectSettings.asset (not YAML); write both AndroidBundleVersionCode and buildNumber.iPhone"
  - "bootstrap_android documents the one-time manual Play Console step clearly — does not try to automate what the API cannot do"
  - "Fastfile lanes are fully implemented (not stubs) for downstream slices to activate with real secrets"
drill_down_paths:
  - .gsd/milestones/M015/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M015/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M015/slices/S01/tasks/T03-SUMMARY.md
  - .gsd/milestones/M015/slices/S01/tasks/T04-SUMMARY.md
verification_result: pass (static verification — runtime on macOS requires bundle install)
completed_at: 2026-03-21T20:30:00Z
---

# S01: Foundation & Bootstrap

**Complete Fastlane scaffold established: two-tier folder structure, 3 shared Ruby lib modules, 9 lane implementations across iOS/Android platforms, all data-driven from version-controlled config files. Every lane is dry-run aware.**

## What Was Built

**T01 (Scaffold):** Gemfile (fastlane ~> 2.225), Appfile (iOS/Android identifiers via env vars), Matchfile (cert repo + API key config), Pluginfile. No secrets hardcoded anywhere.

**T02 (Libs):** Three modules in tools/fastlane/lib/. DryRun wraps every mutation — validation still runs in dry-run mode. BuildNumber correctly decodes/encodes the aabbccdd convention from Docs/BUILD_NUMBERS.md, verified against the real ProjectSettings.asset (10001 → major:0, minor:1, patch:0, counter:1). Config loads and validates JSON files with clear error messages.

**T03 (Data files):** Three JSON config files (JSON-validated), 10 metadata text files within platform character limits. Config design: files store env var NAMES, not values — `Config.resolve_env()` reads the actual value at runtime so no secret ever appears in a committed file. .gitignore extended for *.p8, *.keystore, play-store-credentials.json, build/ dirs, .env.local.

**T04 (Fastfiles):** fastlane/Fastfile: ios platform (register/certs/build/beta/metadata), android platform (bootstrap/build/beta/metadata), top-level status. All lanes use `options[:dry_run] || false`. tools/fastlane/Fastfile: bootstrap_ios (produce with API key), bootstrap_android (step-by-step manual instructions with confirmation prompt). Do/end balance verified statically — both Fastfiles correct.

## Key Design Points

The Fastfiles are **fully implemented** — not stubs. Each lane has real action calls with correct parameter shapes. Downstream slices (S02–S07) don't need to add new lane bodies; they provide the real secrets and environment so the existing code can execute.

The `status` lane uses `Spaceship::ConnectAPI` and `Supply::Client` directly for query-only access without triggering full lane machinery.

Runtime verification (`bundle install` → `fastlane ios register dry_run:true`) requires macOS + Ruby. This is expected — iOS builds require macOS and the development environment is Windows.

## Deviations
- Live fastlane invocation not tested locally (macOS required). Static structural verification passed: JSON validity, metadata length constraints, do/end balance, build number encoding logic.
