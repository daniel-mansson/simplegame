# S04: Final Verification — Compile, Tests, Orphan Cleanup

**Goal:** Confirm clean project state: no orphaned `.meta` files, all target folder file counts match manifest, 340 tests pass, no missing-script warnings in scenes.

**Demo:** `rg -l "." Assets/Scripts/Game/Services/` returns nothing; `find Assets/Scripts/Game/Popup -name "*.cs" | wc -l` returns 1; 340 tests confirmed; Unity opens clean.

## Must-Haves

- Zero `.meta` files pointing to removed directories
- All expected feature folders exist with correct file counts (verified against manifest)
- 340 EditMode tests pass
- No missing-script warnings in Unity Console

## Tasks

- [ ] **T01: Audit file counts, check for orphaned .meta files, run final test pass, commit state**

## Files Likely Touched

- None — verification only; any cleanup is fixing previous slice gaps

## Notes

If S01–S03 were clean, this slice is just a pass/fail check. The value is having an explicit final gate that forces honest verification before marking the milestone complete.
