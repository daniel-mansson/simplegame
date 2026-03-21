---
id: S04
milestone: M015
provides:
  - ios beta lane: pilot upload with api_key, ipa path from config, changelog from release_notes.txt
  - Tester management: reads fastlane/config/testers.json, adds emails to named groups
  - Dry-run mode: logs all pilot and add_testers intent without API calls
  - skip_waiting_for_build_processing:true — lane returns immediately after upload
key_files:
  - fastlane/Fastfile (ios beta lane)
  - fastlane/config/testers.json
  - fastlane/metadata/default/release_notes.txt
key_decisions:
  - "skip_waiting_for_build_processing:true — don't block lane on Apple processing (can take 30+ min)"
  - "distribute_external:false on initial upload — separate step to add to groups"
  - "Changelog sourced from fastlane/metadata/default/release_notes.txt — same file for all builds"
drill_down_paths: []
verification_result: pass (static — runtime requires macOS + ASC API key + real .ipa)
completed_at: 2026-03-21T21:10:00Z
---

# S04: iOS Distribution

**ios beta lane implemented in S01: pilot upload with API key auth, tester management from config, changelog from metadata file. All actions dry-run aware.**

## What Was Built

The ios beta lane reads the .ipa path from build config, loads tester groups from testers.json, reads the changelog from release_notes.txt, and calls pilot with skip_waiting_for_build_processing to return immediately. Tester emails are added to named groups via subsequent pilot calls.

Lane was implemented in S01 as part of the full Fastfile. S04 is the verification milestone that this lane is correct — no code changes needed.

## Deviations
None — lane implemented ahead of slice in S01.
