# S02: Cert & Provisioning

**Goal:** Wire the `fastlane ios certs` lane to actually run `match`, and ensure the lane correctly handles App Store Connect API key auth, reads the Matchfile, and supports all three provisioning types (development, appstore, adhoc). Dry-run mode logs full intent.

**Demo:** `fastlane ios certs dry_run:true` exits 0 and logs exactly what match would do. `fastlane ios certs type:development dry_run:true` logs intent for development profile.

## Must-Haves

- `fastlane ios certs dry_run:true` exits 0 and logs DRY RUN intent
- `fastlane ios certs type:appstore dry_run:true` exits 0
- `fastlane ios certs type:development dry_run:true` exits 0
- `fastlane ios certs type:adhoc dry_run:true` exits 0
- Matchfile is complete — git_url, type, app_identifier, api_key config wired
- A `fastlane/README.md` documents the environment setup: which env vars to set, what the match git repo needs, how to run certs for the first time

## Tasks

- [ ] **T01: Matchfile completion and certs lane wiring**
  Verify the ios certs lane in fastlane/Fastfile correctly calls match with all required params. Confirm Matchfile is complete. Write environment setup docs.

## Files Likely Touched

- `fastlane/Matchfile` (verify completeness)
- `fastlane/Fastfile` (certs lane — already implemented in S01, verify)
- `fastlane/README.md` (new — environment setup docs)
