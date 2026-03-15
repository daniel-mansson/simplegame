---
estimated_steps: 3
estimated_files: 1
---

# T03: Write and run edit-mode tests proving MVP wiring pattern

**Slice:** S01 — Core MVP Infrastructure & Project Setup
**Milestone:** M001

## Description

Write NUnit edit-mode tests that prove the MVP wiring pattern works: a presenter can be constructed with a mocked view interface and injected service through the UIFactory. These tests are the slice's primary deliverable — they retire the "MVP wiring pattern" risk identified in the roadmap. Run tests via Unity CLI batchmode and verify all pass.

## Steps

1. Create `Assets/Tests/EditMode/MVPWiringTests.cs` with a `MockSampleView` test helper class implementing `ISampleView` and the following test methods:
   - `PresenterCanBeConstructedWithMockView` — Construct a `SamplePresenter` directly with `MockSampleView` and `GameService`. Assert the presenter is not null and holds the correct view type.
   - `UIFactoryCreatesSamplePresenterWithService` — Construct a `UIFactory` with a `GameService`, call `CreateSamplePresenter` with a mock view. Assert the returned presenter is not null and is the correct type.
   - `PresenterInitializeSetsWelcomeLabel` — Construct presenter via factory, call `Initialize()`, verify that `MockSampleView.LastLabelText` was set to the welcome message from `GameService`.
   - `PresenterRespondsToViewEvents` — Initialize presenter, trigger `MockSampleView`'s `OnButtonClicked` event, verify the label was updated (service was called and view was updated).
   - `PresenterDisposeUnsubscribesFromViewEvents` — Initialize then Dispose the presenter. Trigger `OnButtonClicked` on the mock view. Verify the label text does NOT change after Dispose (event was unsubscribed).
   - `MockViewHasNoPresenterReference` — Use reflection to verify that `MockSampleView` has no fields of type `Presenter`, `SamplePresenter`, `GameService`, or any type from the services namespace. This structurally proves view independence.

2. The `MockSampleView` class should:
   - Implement `ISampleView`
   - Track `LastLabelText` via the `UpdateLabel` method
   - Expose a method `SimulateButtonClick()` that invokes the `OnButtonClicked` event
   - Have NO references to presenters, services, or any non-view type

3. Run tests via Unity CLI batchmode:
   ```
   "C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" -batchmode -runTests -projectPath "C:\OtherWork\simplegame" -testPlatform editmode -testResults "C:\OtherWork\simplegame\TestResults.xml" -quit
   ```
   Parse `TestResults.xml` to confirm all tests passed with zero failures.

## Must-Haves

- [ ] `MockSampleView` implements `ISampleView` with event and label tracking
- [ ] `MockSampleView` has zero references to presenter/service types (verified by reflection test)
- [ ] Test proves presenter construction with mock view works
- [ ] Test proves UIFactory creates presenter with correct service injection
- [ ] Test proves presenter responds to view events (Initialize subscribes)
- [ ] Test proves Dispose unsubscribes from events
- [ ] All tests pass in Unity CLI batchmode
- [ ] `TestResults.xml` shows zero failures

## Verification

- `TestResults.xml` exists and shows all tests passed (result="Passed", failures="0")
- `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing (including test files)

## Inputs

- `Assets/Scripts/Core/MVP/IView.cs` — marker interface from T02
- `Assets/Scripts/Core/MVP/Presenter.cs` — base presenter from T02
- `Assets/Scripts/Core/MVP/ISampleView.cs` — sample view interface from T02
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — concrete presenter from T02
- `Assets/Scripts/Core/MVP/UIFactory.cs` — factory from T02
- `Assets/Scripts/Core/Services/GameService.cs` — domain service from T02
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — test assembly definition from T01

## Expected Output

- `Assets/Tests/EditMode/MVPWiringTests.cs` — complete edit-mode test file with 6 passing tests
- `TestResults.xml` — Unity test runner output confirming zero failures

## Observability Impact

**What changes when this task runs:**
- `TestResults.xml` is created at `C:\OtherWork\simplegame\TestResults.xml` by the `-runTests` batchmode run. Presence of this file and `result="Passed" failed="0"` confirms all edit-mode tests passed.
- Unity's `Logs/Editor.log` captures the test runner's full output including any compilation errors that prevent tests from running. Search for `error CS` or `CompileError` to detect compilation failures.

**How a future agent inspects this task:**
```bash
# Confirm test file exists
ls /c/OtherWork/simplegame/Assets/Tests/EditMode/MVPWiringTests.cs

# Confirm all tests passed
grep -E 'result=|failed=|passed=' /c/OtherWork/simplegame/TestResults.xml | head -5

# Confirm zero compilation errors during the test run
grep -i "error CS\|CompileError\|will not be loaded" /c/OtherWork/simplegame/Logs/Editor.log | tail -20

# Confirm no static state leaked into test file
grep -r "static " /c/OtherWork/simplegame/Assets/Tests --include="*.cs" | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
```

**Failure state visibility:**
- Unity exits non-zero and `TestResults.xml` absent → compilation error before tests ran; check `Logs/Editor.log` for `error CS`.
- `TestResults.xml` exists with `failed="N"` (N > 0) → specific test assertion failed; failure messages are in `<test-case>` XML elements.
- `MockViewHasNoPresenterReference` test fails → mock inadvertently references a presenter/service type; check `MockSampleView` field declarations.
