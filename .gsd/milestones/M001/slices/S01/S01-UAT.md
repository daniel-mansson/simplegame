# S01: Core MVP Infrastructure & Project Setup — UAT

**Milestone:** M001
**Written:** 2026-03-15

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S01 has no runtime UI — all verification is via compiled artifacts, static analysis, and NUnit edit-mode test results. The primary deliverable is `TestResults.xml` proving the wiring pattern works. No Unity play mode or human visual inspection is required for this slice.

## Preconditions

- Unity 6000.3.4f1 is installed at `C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe`
- Project exists at `C:\OtherWork\simplegame`
- No Unity editor instance is open and locking the project
- Network access is available (only needed if UniTask must re-resolve from git)

## Smoke Test

Run the following and confirm exit code 0 and `TestResults.xml` is present and shows `result="Passed"`:

```bash
"C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
  -batchmode -runTests \
  -projectPath "C:/OtherWork/simplegame" \
  -testPlatform editmode \
  -testResults "C:/OtherWork/simplegame/TestResults.xml" \
  -logFile "C:/OtherWork/simplegame/Logs/uat-smoke.log"
echo "Exit: $?"

grep -E 'result=|failed=|passed=' /c/OtherWork/simplegame/TestResults.xml | head -3
```

**Expected:** Exit code `0`, output contains `result="Passed"` and `failed="0"`.

---

## Test Cases

### 1. UniTask is installed and resolved

1. Open `Packages/packages-lock.json` in a text editor or run:
   ```bash
   grep -A5 "com.cysharp.unitask" /c/OtherWork/simplegame/Packages/packages-lock.json
   ```
2. **Expected:** Entry for `com.cysharp.unitask` is present with a git commit hash (e.g., `"hash": "ad5ed25e82a3..."`) confirming the package was resolved from the git URL.

---

### 2. Project compiles cleanly with no errors

1. Run a batchmode compilation pass:
   ```bash
   "C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
     -batchmode -projectPath "C:/OtherWork/simplegame" \
     -logFile "C:/OtherWork/simplegame/Logs/uat-compile.log" -quit
   echo "Exit: $?"
   ```
2. Check the log:
   ```bash
   grep -c "error CS" /c/OtherWork/simplegame/Logs/uat-compile.log
   ```
3. **Expected:** Exit code `0`, grep returns `0` (no `error CS` lines).

---

### 3. All 6 edit-mode tests pass in batchmode

1. Run the test suite:
   ```bash
   "C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
     -batchmode -runTests \
     -projectPath "C:/OtherWork/simplegame" \
     -testPlatform editmode \
     -testResults "C:/OtherWork/simplegame/TestResults.xml" \
     -logFile "C:/OtherWork/simplegame/Logs/uat-tests.log"
   echo "Exit: $?"
   ```
   **Note: do NOT include `-quit` — it races the test runner.**
2. Verify results:
   ```bash
   grep -E 'result=|failed=|passed=|total=' /c/OtherWork/simplegame/TestResults.xml | head -3
   ```
3. **Expected:** `result="Passed"`, `total="6"`, `passed="6"`, `failed="0"`.

---

### 4. All 6 named tests are present and individually passed

1. Parse per-test results:
   ```bash
   grep 'test-case' /c/OtherWork/simplegame/TestResults.xml | \
     grep -o 'name="[^"]*"\|result="[^"]*"'
   ```
2. **Expected:** All 6 test names appear with `result="Passed"`:
   - `MockViewHasNoPresenterReference`
   - `PresenterCanBeConstructedWithMockView`
   - `PresenterDisposeUnsubscribesFromViewEvents`
   - `PresenterInitializeSetsWelcomeLabel`
   - `PresenterRespondsToViewEvents`
   - `UIFactoryCreatesSamplePresenterWithService`

---

### 5. No static state fields exist in any C# source

1. Run the static guard:
   ```bash
   grep -r "static " --include="*.cs" /c/OtherWork/simplegame/Assets/ | \
     grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
   ```
2. **Expected:** No output (exit code 1 = no matches). Any output is a violation.

---

### 6. Core types are pure C# with no Unity engine coupling

1. Check for MonoBehaviour in core scripts:
   ```bash
   grep -r "MonoBehaviour" --include="*.cs" /c/OtherWork/simplegame/Assets/Scripts/Core/
   ```
2. Check for UnityEngine using directives in core scripts:
   ```bash
   grep -r "using UnityEngine" --include="*.cs" /c/OtherWork/simplegame/Assets/Scripts/Core/
   ```
3. **Expected:** Both commands return no output. Any output is a violation.

---

### 7. ISampleView has no back-references to presenter or service types

1. Run:
   ```bash
   grep -n "Presenter\|GameService\|UIFactory" \
     /c/OtherWork/simplegame/Assets/Scripts/Core/MVP/ISampleView.cs
   ```
2. **Expected:** No output. ISampleView must be entirely self-contained.

---

### 8. Assembly definitions reference correct dependencies

1. Inspect runtime asmdef:
   ```bash
   cat /c/OtherWork/simplegame/Assets/Scripts/SimpleGame.Runtime.asmdef
   ```
   **Expected:** Contains `"UniTask"` in references, `"autoReferenced": true`.

2. Inspect test asmdef:
   ```bash
   cat /c/OtherWork/simplegame/Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef
   ```
   **Expected:** References `SimpleGame.Runtime`, `UniTask`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; `includePlatforms: ["Editor"]`.

---

## Edge Cases

### Edge Case 1: Test run with -quit included (should fail)

1. Run the test command WITH `-quit`:
   ```bash
   "C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
     -batchmode -runTests \
     -projectPath "C:/OtherWork/simplegame" \
     -testPlatform editmode \
     -testResults "C:/OtherWork/simplegame/TestResults-quit-test.xml" \
     -logFile "C:/OtherWork/simplegame/Logs/uat-quit-test.log" \
     -quit
   ```
2. **Expected:** `TestResults-quit-test.xml` may be missing or empty. This confirms the known race condition. Do not use `-quit` for test runs.

---

### Edge Case 2: Static guard catches a violation

1. Temporarily add a static field to any Core file (e.g., `private static int _test = 0;` in `GameService.cs`).
2. Run the static guard:
   ```bash
   grep -r "static " --include="*.cs" /c/OtherWork/simplegame/Assets/ | \
     grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
   ```
3. **Expected:** The offending line is returned.
4. Remove the test field before proceeding.

---

### Edge Case 3: MockSampleView has no presenter fields (reflection test)

The `MockViewHasNoPresenterReference` test validates this at runtime via reflection. As a manual check:
1. Open `Assets/Tests/EditMode/MVPWiringTests.cs`
2. Inspect the `MockSampleView` class definition
3. **Expected:** Fields are only `LastLabelText` (string), `UpdateLabelCallCount` (int), and the `OnButtonClicked` event. No Presenter, GameService, UIFactory, or ScreenManager references.

---

## Failure Signals

- `TestResults.xml` missing entirely → Unity crashed or `-quit` was included; check `Logs/uat-tests.log` for crash/hang
- `TestResults.xml` shows `failed="1"` or more → a wiring test broke; check per-test output in the XML for the assertion message
- `error CS` in `Editor.log` / compile log → compilation failure; check the specific error for the file and line
- `grep -r "static "` returns output → static state violation introduced; remove the offending field
- `grep -r "MonoBehaviour" Assets/Scripts/Core/` returns output → presenter or service was accidentally made a MonoBehaviour
- `Packages/packages-lock.json` missing `com.cysharp.unitask` → UniTask didn't resolve; check network and git URL in manifest.json
- Exit code non-zero from batchmode → Unity crash; open `Editor.log` and search for `exception` or `crash`

---

## Requirements Proved By This UAT

- R001 — MVP pattern with strict separation: IView, Presenter<TView>, ISampleView, SamplePresenter, UIFactory all exist; no layer references one it shouldn't (proven by compile + tests)
- R002 — View independence: ISampleView has no backward refs; MockSampleView reflection test verifies at runtime
- R003 — Interface-per-view: ISampleView is the interface; SamplePresenter depends only on ISampleView, never on MockSampleView or any concrete type
- R004 — Central UI factory: UIFactory.CreateSamplePresenter() is the single construction path; no `new SamplePresenter()` call outside the factory
- R005 — Constructor/init injection only: all deps flow via constructors; no static, no locator (proven by tests and static guard)
- R006 — No static state: static guard passes with no output
- R007 — Domain services: GameService exists as a plain C# class injected into SamplePresenter via UIFactory
- R014 — UniTask installed: packages-lock.json confirms resolution; zero compile errors
- R015 — Edit-mode unit tests: 6 tests pass in batchmode CLI
- R017 — Each layer testable in isolation: all 6 tests use MockSampleView (no Unity runtime); presenters, services, and factory all tested independently

## Not Proven By This UAT

- R008 — Boot scene initialization flow: requires S05
- R009 — Hybrid scene management: requires S02
- R010 — Screen navigation: requires S02
- R011 — Stack-based popups: requires S03
- R012 — Input blocker: requires S03
- R013 — Fade transitions: requires S04
- R016 — Demo screens with end-to-end dependency flow: requires S05
- UniTask async operations at runtime: UniTask compiles, but async operations (scene loading, transitions) are only exercised in S02–S04

## Notes for Tester

- All UAT steps here are artifact-driven — no Unity editor window needs to open. All commands run headless in batchmode.
- If Unity is already open on the project, close it before running batchmode commands. Unity will not open a second editor on the same project.
- The `TestResults.xml` at the project root is overwritten on each test run — it always reflects the most recent run.
- The `Logs/` directory may contain many `.log` files from earlier task runs (e.g., `T02-compile.log`, `T03-test-run2.log`). These are safe to read for historical context.
- A fresh clone of the repo will need to open Unity once to download UniTask from git. The first batchmode open may take several minutes while the git package resolves and caches.
