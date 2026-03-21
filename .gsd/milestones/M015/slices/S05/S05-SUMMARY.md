---
id: S05
milestone: M015
provides:
  - android beta lane: upload_to_play_store with json_key, aab path, track parameter
  - Track selection: internal (default), alpha, beta, production via options[:track]
  - Release notes from fastlane/metadata/android/en-US/release_notes.txt
  - Dry-run: logs intent with package name and track
  - Pre-check: raises if .aab file not found (unless dry_run)
key_files:
  - fastlane/Fastfile (android beta lane)
  - fastlane/metadata/android/en-US/release_notes.txt
key_decisions:
  - "Default track is internal — safest default, requires explicit opt-in for public tracks"
  - "release_status:completed — required for internal track (draft not supported programmatically)"
  - "skip_upload_apk:true — only AAB upload supported"
drill_down_paths: []
verification_result: pass (static — runtime requires real .aab + Google Play service account)
completed_at: 2026-03-21T21:10:00Z
---

# S05: Android Distribution

**android beta lane implemented in S01: upload_to_play_store with service account auth, track selection, release notes from metadata. All actions dry-run aware.**

## What Was Built

The android beta lane reads the .aab path from build config, reads the Google Play key file path from env (via app_config), reads the changelog from the Android release_notes.txt file, and calls upload_to_play_store with the specified track (default: internal). Pre-checks that the AAB exists before attempting upload.

Lane was implemented in S01. S05 is the verification milestone.

## Deviations
None — lane implemented ahead of slice in S01.
