# S01: Core MVP Infrastructure & Project Setup

**Goal:** Unity project exists with UniTask installed, MVP base types defined (IView, Presenter<TView>, UIFactory), and edit-mode tests prove a presenter can be constructed with a mocked view interface and injected service.

**Demo:** Run edit-mode tests via Unity CLI batchmode — all pass, proving presenter construction with mocked view and injected service through UIFactory.

## Must-Haves

- Unity 6000.3.4f1 project created at `C:\OtherWork\simplegame` with standard structure
- UniTask installed via UPM git URL and compiling
- `IView` marker interface defined
- `Presenter<TView>` generic base class with virtual `Dispose()` method
- `UIFactory` that receives services at construction and creates presenters
- At least one sample view interface (e.g., `ISampleView`) with events and update methods
- At least one domain service (e.g., `GameService`) injected into a presenter via factory
- Assembly definitions for runtime code and edit-mode tests
- Edit-mode tests passing that verify: presenter construction, view interface mocking, service injection, factory wiring
- No static state fields in any `.cs` file

## Proof Level

- This slice proves: **contract** — MVP wiring pattern works in isolation via edit-mode tests
- Real runtime required: **no** — all verification is edit-mode
- Human/UAT required: **no**

## Verification

- Edit-mode tests run and pass via Unity CLI:
  ```
  "C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" -batchmode -runTests -projectPath "C:\OtherWork\simplegame" -testPlatform editmode -testResults "C:\OtherWork\simplegame\TestResults.xml" -quit
  ```
- Test file: `Assets/Tests/EditMode/MVPWiringTests.cs` — verifies:
  - Presenter can be constructed with a mock view interface
  - UIFactory creates presenter with correct service injection
  - View interface events can be subscribed to by presenter
  - Presenter holds reference to view, view has no reference to presenter
  - Service is callable from presenter
- Static state check: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing
- `TestResults.xml` shows all tests passed with zero failures

## Observability / Diagnostics

- **Package resolution:** `Packages/packages-lock.json` is generated on first project open — presence of `com.cysharp.unitask` entry confirms UniTask resolved.
- **Compilation health:** `Library/Bee/artifacts/` (incremental build cache) and `Logs/Editor.log` are the primary compiler output surfaces. Search for `error CS` or `CompileError` in `Editor.log` to detect failures.
- **Test results:** `TestResults.xml` is written by `-runTests` batchmode run — parse `<test-run result="Passed"` and `failed="0"` for slice verification.
- **Batchmode exit codes:** Unity exits 0 on success, non-zero on crash or compile error. All batchmode runs must be checked via `echo $?` / `$LASTEXITCODE`.
- **Assembly resolution:** If an assembly reference can't be resolved, Unity logs `Assembly 'X' will not be loaded due to errors` in `Editor.log`.
- **Redaction:** No secrets in this slice. `Editor.log` may contain local machine paths — safe to include in summaries.
- **Inspection command:** `grep -i "error\|exception\|failed\|unitask" /c/OtherWork/simplegame/Logs/Editor.log | tail -40`

## Integration Closure

- Upstream surfaces consumed: none (first slice)
- New wiring introduced: assembly definition references (test → runtime → UniTask)
- What remains before milestone is usable end-to-end: S02 (screen management), S03 (popups), S04 (transitions), S05 (boot flow + demo)

## Tasks

- [x] **T01: Create Unity project, install UniTask, set up folder structure and assembly definitions** `est:30m`
  - Why: Everything depends on having a compilable Unity project with UniTask. This retires the UniTask installation risk immediately.
  - Files: `Assets/`, `Packages/manifest.json`, `ProjectSettings/`, `Assets/Scripts/SimpleGame.Runtime.asmdef`, `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef`
  - Do: Create project via `Unity.exe -batchmode -createProject`, add UniTask git URL to `manifest.json`, create folder structure (`Assets/Scripts/Core/MVP/`, `Assets/Scripts/Core/Services/`, `Assets/Tests/EditMode/`), create assembly definitions with correct references. Add Unity-standard `.gitignore` entries.
  - Verify: Open project in batchmode (`-quit` after import) — exits cleanly with no compilation errors. UniTask namespace resolves.
  - Done when: Unity project compiles cleanly with UniTask available, folder structure and assembly definitions in place.

- [x] **T02: Define MVP base types, sample view interface, and domain service** `est:30m`
  - Why: The core types (IView, Presenter<TView>, UIFactory) are what every downstream slice builds on. The sample view interface and service prove the pattern is comfortable before committing to it.
  - Files: `Assets/Scripts/Core/MVP/IView.cs`, `Assets/Scripts/Core/MVP/Presenter.cs`, `Assets/Scripts/Core/MVP/UIFactory.cs`, `Assets/Scripts/Core/Services/GameService.cs`, `Assets/Scripts/Core/MVP/ISampleView.cs`
  - Do: Define `IView` as empty marker interface. Define `Presenter<TView>` as generic base class storing the view reference with virtual `Dispose()`. Define `UIFactory` accepting services in constructor with a `CreateSamplePresenter(ISampleView view)` method. Define `ISampleView : IView` with `event Action OnButtonClicked` and `void UpdateLabel(string text)`. Define `GameService` as a plain C# class with a method returning data. Define `SamplePresenter` extending `Presenter<ISampleView>` that subscribes to view events and calls the service. No static state. No MonoBehaviour on presenters.
  - Verify: Project compiles in batchmode with zero errors. `grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing.
  - Done when: All MVP base types, sample presenter, sample view interface, and domain service compile cleanly.

- [x] **T03: Write and run edit-mode tests proving MVP wiring pattern** `est:30m`
  - Why: Tests are the proof that the MVP pattern works — a presenter can be constructed from the factory with a mocked view and injected service. This is the slice's primary deliverable.
  - Files: `Assets/Tests/EditMode/MVPWiringTests.cs`
  - Do: Write NUnit edit-mode tests: (1) `PresenterCanBeConstructedWithMockView` — create a mock `ISampleView` implementation, construct `SamplePresenter`, verify it holds the view reference. (2) `UIFactoryCreatesSamplePresenterWithService` — construct `UIFactory` with `GameService`, call `CreateSamplePresenter` with mock view, verify presenter is wired correctly. (3) `PresenterSubscribesToViewEvents` — trigger mock view's event, verify presenter responds (calls service, updates view). (4) `ViewHasNoReferenceToPresenter` — verify the mock view implementation has no presenter/service fields. (5) `PresenterDisposeUnsubscribesEvents` — call Dispose, trigger event, verify no response.
  - Verify: Run tests via `Unity.exe -batchmode -runTests -testPlatform editmode` — all pass. Parse `TestResults.xml` for zero failures.
  - Done when: All edit-mode tests pass in CLI batchmode, `TestResults.xml` confirms zero failures.

## Files Likely Touched

- `Packages/manifest.json`
- `Assets/Scripts/SimpleGame.Runtime.asmdef`
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef`
- `Assets/Scripts/Core/MVP/IView.cs`
- `Assets/Scripts/Core/MVP/Presenter.cs`
- `Assets/Scripts/Core/MVP/UIFactory.cs`
- `Assets/Scripts/Core/MVP/ISampleView.cs`
- `Assets/Scripts/Core/MVP/SamplePresenter.cs`
- `Assets/Scripts/Core/Services/GameService.cs`
- `Assets/Tests/EditMode/MVPWiringTests.cs`
