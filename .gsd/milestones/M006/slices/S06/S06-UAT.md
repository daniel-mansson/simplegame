# S06: Environment progression and full flow integration — UAT

**Milestone:** M006
**Written:** 2026-03-17

## UAT Type

- UAT mode: mixed (artifact + live-runtime)
- Why this mode is sufficient: Code correctness proven by tests in S01-S05. Integration requires play-mode verification after SceneSetup regeneration.

## Preconditions

- Branch merged into Unity project
- Run Tools/Setup/Create Test World Data (if not already done)
- Run Tools/Setup/Create And Register Scenes
- Unity compiles without errors

## Smoke Test

Enter play mode from Boot scene. Main screen shows Garden environment with objects and golden piece balance.

## Test Cases

### 1. Full flow navigation

1. Enter play mode → main screen appears
2. Tap Play → InGame scene loads with hearts and piece counter
3. Place correct pieces to win → LevelComplete popup shows golden pieces earned
4. Tap Continue → returns to main screen with updated balance
5. **Expected:** Full navigation loop works

### 2. Object restoration

1. Earn golden pieces by winning levels
2. On main screen, tap an object
3. **Expected:** Object progresses, balance decreases

### 3. Persistence

1. Complete some levels and restore some objects
2. Stop and re-enter play mode
3. **Expected:** Golden piece balance and object progress preserved

## Failure Signals

- Compile errors
- NullReferenceException in GameBootstrapper (missing WorldData, missing services)
- SceneSetup field wiring warnings in console
- Popups not showing (UnityPopupContainer serialized fields null)

## Requirements Proved By This UAT

- R058 — Full navigable flow
- R049 — Meta persistence via PlayerPrefs

## Not Proven By This UAT

- Environment unlocking (1-3 available rule) — logic exists in MainMenuSceneController.GetCurrentEnvironment() but needs multiple environments restored to demonstrate

## Notes for Tester

SceneSetup must be re-run after merge to regenerate all scenes with new view types. WorldData asset must exist at Assets/Data/WorldData.asset.
