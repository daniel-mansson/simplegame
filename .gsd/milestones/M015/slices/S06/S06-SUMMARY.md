---
id: S06
milestone: M015
provides:
  - ios metadata lane: deliver with skip_binary_upload + skip_screenshots, reads from fastlane/metadata/default/
  - android metadata lane: upload_to_play_store with skip_upload_apk + skip_upload_aab, reads from fastlane/metadata/android/
  - Metadata text files: 6 iOS files (name, subtitle, description, keywords, support_url, release_notes)
  - Metadata text files: 4 Android files (title, short_description, full_description, release_notes)
  - All files within platform character limits (verified)
key_files:
  - fastlane/Fastfile (ios metadata, android metadata lanes)
  - fastlane/metadata/default/*.txt
  - fastlane/metadata/android/en-US/*.txt
key_decisions:
  - "skip_screenshots:true — screenshot upload deferred to R132"
  - "force:true on deliver — suppresses interactive prompts in non-interactive mode"
  - "metadata_path points to fastlane/metadata/android/ (supply reads per-locale subdirectories)"
drill_down_paths: []
verification_result: pass (static — runtime requires live store credentials)
completed_at: 2026-03-21T21:10:00Z
---

# S06: Metadata Management

**Metadata lanes and text files implemented in S01/T03 + T04. deliver (iOS) and supply (Android) lanes configured to push store text without binaries or screenshots.**

## What Was Built

Both metadata lanes use the standard deliver/supply configuration with binary and screenshot uploads skipped. Metadata content is real Puzzle Tap copy, within platform character limits. The data-driven design means updating store copy is a git commit to a .txt file, not a portal session.

## Deviations
None — lanes and files implemented ahead of slice in S01.
