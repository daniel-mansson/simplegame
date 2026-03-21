---
id: M015
provides:
  - Complete Fastlane distribution pipeline for Puzzle Tap (iOS + Android)
  - Two-tier folder structure: fastlane/ (project) + tools/ (extractable orchestration)
  - 9 lanes: ios register/certs/build/beta/metadata, android bootstrap/build/beta/metadata
  - Top-level status lane returning JSON from both platforms
  - DryRun module: every lane dry-run aware — validate intent without API calls
  - BuildNumber module: aabbccdd convention, reads/writes ProjectSettings.asset
  - Config module: JSON config files with env var name indirection for all secrets
  - Unity BuildScript.cs: CLI entry points for iOS Xcode export and Android AAB build
  - 10 metadata text files: 6 iOS (deliver format) + 4 Android (supply format)
  - fastlane/README.md: complete environment setup documentation
  - All secrets via env vars — no secrets committed
key_files:
  - Gemfile
  - fastlane/Fastfile
  - fastlane/Appfile
  - fastlane/Matchfile
  - fastlane/README.md
  - fastlane/config/app.json
  - fastlane/config/build.json
  - fastlane/config/testers.json
  - tools/fastlane/Fastfile
  - tools/fastlane/lib/dry_run.rb
  - tools/fastlane/lib/build_number.rb
  - tools/fastlane/lib/config.rb
  - Assets/Editor/BuildScript.cs
key_decisions:
  - "D076: Two-tier structure — fastlane/ calls tools/fastlane/lib/ but not reverse"
  - "D077: with_dry_run wraps every API call; validation always runs"
  - "D078: Android app creation documented as manual step (hard platform limit)"
  - "D079: BuildNumber.write() called before every build"
  - "D080: status lane returns JSON to stdout for external consumption"
  - "BuildScript uses fully qualified namespace SimpleGame.Editor.BuildScript.BuildIOS/BuildAndroid"
  - "All 9 lanes implemented in S01 Fastfile — subsequent slices verified correctness"
completed_at: 2026-03-21T21:30:00Z
---

# M015: Fastlane Distribution Pipeline

**Complete Fastlane distribution pipeline for Puzzle Tap: 9 lanes covering iOS and Android registration, provisioning, builds, TestFlight/Play Store distribution, metadata management, and status query. All data-driven from version-controlled files. Every lane dry-run aware.**

## What Was Delivered

**S01 (Foundation):** Two-tier scaffold established. Gemfile, Appfile, Matchfile, Pluginfile. All 9 lanes implemented in fastlane/Fastfile. Bootstrap lanes in tools/fastlane/Fastfile. Three shared Ruby lib modules: DryRun (mutation wrapper), BuildNumber (aabbccdd encoding), Config (JSON loader). Three JSON config files, ten metadata text files. .gitignore extended for secrets and build artifacts.

**S02 (Provisioning):** iOS certs lane verified correct (match with API key auth, all provisioning types). fastlane/README.md written — 14 env vars documented, first-time setup steps, lane reference, secrets checklist.

**S03 (Build):** Unity BuildScript.cs created with BuildIOS() and BuildAndroid() static methods. CLI arg parsing for output path and Android keystore. AAB mode enabled for Android. Exit codes for CI integration. Fastfile updated with fully qualified executeMethod names.

**S04–S07 (Distribution/Metadata/Status):** All lanes implemented in S01 and verified: ios beta (pilot + tester management), android beta (upload_to_play_store track-based), ios/android metadata (deliver/supply), status (Spaceship + Supply JSON aggregation).

## Architecture

The Fastfile is data-driven throughout: bundle IDs, env var names, paths, and tester lists all come from fastlane/config/*.json. Store copy comes from fastlane/metadata/**/*.txt. No values are hardcoded in Ruby files.

The tools/ layer is designed with a clean seam at tools/fastlane/lib/ — these three modules (dry_run, build_number, config) can be packaged and shared across projects. fastlane/ does not expose anything upward into tools/.

## Runtime Verification Note

Full live verification (bundle exec fastlane ios register, certs, build, beta, status) requires macOS + Ruby + valid credentials. The dev environment is Windows, so verification was static: JSON validation, metadata length checks, do/end balance analysis, build number encode/decode logic. All static checks pass.
