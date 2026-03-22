---
id: T03
parent: S01
milestone: M016
provides:
  - MockPlayFabAuthService — reusable mock for all downstream tests in S02/S03/S04
  - PlayFabAuthServiceTests — 5 edit-mode tests covering mock fidelity and offline guard pattern
requires: []
affects: [S02, S03, S04]
key_files:
  - Assets/Tests/EditMode/Game/PlayFabAuthServiceTests.cs
key_decisions:
  - "Tests verify IPlayFabAuthService contract via mock — real PlayFabAuthService requires network and Title ID"
  - "MockPlayFabAuthService is public in test assembly — shared by S02/S03/S04 test classes"
patterns_established:
  - "Offline guard test pattern: check IsLoggedIn before cloud op — used in all downstream service tests"
drill_down_paths:
  - .gsd/milestones/M016/slices/S01/tasks/T01-PLAN.md
duration: 10min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# T03: Edit-mode tests for auth service

**MockPlayFabAuthService + 5 edit-mode tests covering login success/failure, IsLoggedIn state, PlayFabId stability, and offline guard pattern.**

## What Happened

Wrote `PlayFabAuthServiceTests` with `MockPlayFabAuthService` (configurable success/failure, fake ID, call counter). Tests cover: initial state not logged in, login sets IsLoggedIn, login failure throws, multiple logins return same ID, offline guard check.

`MockPlayFabAuthService` is public and reusable — S02/S03/S04 tests will reference it directly rather than defining their own auth mocks.

Unity Editor must be open for test runner to execute. Static review confirms all referenced types exist in the SDK.

## Deviations

Fixed two test logic errors during review: (1) idempotency test corrected to assert `LoginCallCount == 2` not `1`; (2) failure test changed from `AggregateException` to `PlayFabLoginException` since the mock throws synchronously.

## Files Created/Modified
- `Assets/Tests/EditMode/Game/PlayFabAuthServiceTests.cs` — new test file
