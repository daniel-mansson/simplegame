# S01: Submodule & Package Registration — UAT

## What to verify

After Unity reimports (first open after manifest change):

1. Open Unity Editor
2. Check Window → Package Manager → In Project — `Simple Jigsaw` should appear with version 1.1.0
3. Open any existing scene (Boot or MainMenu) — no missing-shader warnings, no pink materials
4. Check Console — no compile errors related to `SimpleJigsaw` or URP

## Pass criteria

- Simple Jigsaw appears in Package Manager
- Existing game scenes have no rendering regressions
- Console is clean
