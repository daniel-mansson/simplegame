# S03: Stub gameplay screen with hearts — UAT

**Milestone:** M006
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: InGame rework is tested via edit-mode tests. View is text-stub only. No runtime verification needed until S06 integration.

## Preconditions

- Worktree merged or Unity pointed at worktree
- Unity compiles without errors

## Smoke Test

All 18 InGame tests (14 presenter + 4 controller) pass in edit-mode test runner.

## Test Cases

### 1. Piece placement wins

1. Run InGamePresenterTests — PlaceCorrect_AllPieces_ResolvesWin
2. **Expected:** Placing totalPieces correct pieces auto-resolves Win

### 2. Heart loss loses

1. Run InGamePresenterTests — PlaceIncorrect_AllHearts_ResolvesLose
2. **Expected:** Losing all 3 hearts auto-resolves Lose

### 3. Controller retry flow

1. Run InGameSceneControllerTests — RunAsync_LoseRetryThenWin_AdvancesLevel
2. **Expected:** Lose → retry creates fresh presenter → win → level advances

## Edge Cases

### Hearts survive partial loss

1. Place incorrect once
2. **Expected:** 2 hearts remain, game continues

### Mixed correct + incorrect

1. Alternate correct and incorrect placements
2. **Expected:** Win if pieces reach total before hearts reach 0

## Failure Signals

- Compile errors in SimpleGame.Game assembly (IInGameView signature change)
- InGamePresenter tests fail
- InGameSceneController tests fail
- Other test files fail due to UIFactory signature change (mitigated by optional parameter)

## Requirements Proved By This UAT

- R046 — Stub gameplay with hearts and piece counter (service + presenter layer)
- R056 — Interstitial ad debug log stub (Debug.Log at win/lose)
- R057 — Heart system 3 per level (presenter resets to 3, incorrect costs 1, 0 = lose)

## Not Proven By This UAT

- Runtime play-mode verification (S06)
- SceneSetup.cs UI creation (S06)
- Visual stub rendering

## Notes for Tester

The InGameSceneController no longer has the inner while loop — the presenter auto-resolves. Tests verify the retry flow works with fresh presenter creation.
