# S01: Core MVP Infrastructure & Project Setup — Research

**Date:** 2026-03-15

## Summary

S01 is the foundation slice — it establishes the Unity project, installs UniTask, defines the core MVP type system (IView, Presenter\<TView\>, UIFactory), creates a sample domain service, and proves the wiring pattern works via edit-mode tests. The project is currently completely empty — no Unity project structure, no scripts, no packages.

The primary risk is getting the MVP wiring pattern right on the first pass: the Presenter\<TView\> base class, the UIFactory construction pattern, and the view interface conventions need to be comfortable enough that all downstream slices (S02–S05) can build on them without friction. The secondary risk — UniTask installation — is low: UniTask v2.5.x installs via a simple UPM git URL and is well-proven on Unity 6.

The recommended approach is: (1) create the Unity project via CLI batchmode with `-createProject`, (2) add UniTask to `manifest.json` before first editor open so package resolution happens automatically, (3) define the MVP types as plain C# in a clean folder structure, (4) write edit-mode tests proving presenter construction with a mocked view interface and injected service, and (5) run tests via CLI batchmode to verify.

## Recommendation

**Create the project headlessly, define types as plain C#, test everything in edit-mode.**

- Use `Unity.exe -batchmode -createProject <path> -quit` to scaffold the project structure. This generates `Assets/`, `Packages/manifest.json`, `ProjectSettings/`, etc.
- Immediately edit `Packages/manifest.json` to add UniTask before first full editor open.
- Define all MVP base types (`IView`, `Presenter<TView>`, `UIFactory`) as plain C# classes — no MonoBehaviour dependencies in the core types. This keeps them testable in edit-mode.
- Use Unity's built-in Test Framework (1.6.0, included with 6000.3.4f1) with NUnit for edit-mode tests.
- Folder structure: `Assets/Scripts/Core/MVP/` for base types, `Assets/Scripts/Core/Services/` for domain services, `Assets/Tests/EditMode/` for tests.
- Assembly definitions (`.asmdef`) are required for the test assembly to reference the runtime assembly.

## Don't Hand-Roll

| Problem | Existing Solution | Why Use It |
|---------|------------------|------------|
| Async/await in Unity | UniTask (Cysharp.Threading.Tasks) v2.5.x | Zero-allocation, CancellationToken support, awaitable scene loading, PlayerLoop integration. Decision D007 mandates this. |
| Unit testing framework | Unity Test Framework 1.6.0 (built-in) + NUnit 2.0.3 | Ships with Unity 6000.3.4f1, no additional install needed. Supports edit-mode and play-mode test runners. |
| Project creation | Unity CLI `-createProject` | Generates correct project structure with proper `manifest.json`, `ProjectSettings/`, etc. No need to manually create folders. |

## Existing Code and Patterns

- No existing code — project is empty. All patterns must be established fresh.
- `.gsd/DECISIONS.md` — 13 architectural decisions that constrain the implementation. Key ones for S01:
  - D001: MVP with strict separation (views expose interfaces, presenters are plain C#)
  - D002: Constructor/init injection, no DI framework
  - D003: No static fields holding state
  - D011: Central UIFactory constructs all presenters
  - D013: Views have no backward references to presenters/models/services

## Constraints

- **Unity 6000.3.4f1** — must use this specific editor version (installed at `C:\Program Files\Unity\Hub\Editor\6000.3.4f1\`)
- **No static state** (D003) — rules out singleton patterns, static Instance fields. Must be verified by grep.
- **No DI framework** (D002) — all wiring is manual constructor/init injection via UIFactory
- **uGUI only** (D004) — Canvas/GameObject UI, not UI Toolkit. Views are MonoBehaviours.
- **UniTask for all async** (D007) — no coroutines, no bare `Task`, no Unity `Awaitable`
- **View independence** (D013) — view interfaces expose events (for user actions) and methods (for state updates). No SetPresenter, no backward references.
- **Edit-mode tests preferred** (D012) — presenters and services must be testable without entering play mode
- **Test Framework 1.6.0** — bundled with Unity 6000.3.4f1, uses NUnit via `com.unity.ext.nunit` 2.0.3
- **Assembly definitions required** — test assemblies need `.asmdef` with `Editor` platform and reference to the runtime assembly; runtime code also needs an `.asmdef` for test assemblies to reference it

## Common Pitfalls

- **Presenter inheriting MonoBehaviour** — Presenters must be plain C# classes. If they're MonoBehaviours, they can't be constructed in edit-mode tests without a GameObject. Keep them pure.
- **View interfaces leaking Unity types** — View interfaces should use plain C# types (string, int, Action, etc.) where possible. Using `Button` or `Text` in the interface couples the presenter to uGUI. Events should be `event Action` or `event Action<T>`, not `Button.onClick`.
- **Circular reference between presenter and view** — The presenter holds a reference to IView. The view must NOT hold a reference to the presenter (D013). Communication from view to presenter is via events on the interface. This is asymmetric by design.
- **UIFactory becoming a God class** — The factory will grow as screens/popups are added. Keep each Create method focused: receive the view interface, inject the correct service(s), return the presenter. Don't let the factory own business logic.
- **Forgetting `.asmdef` for test discovery** — Unity's test runner only discovers tests in assemblies with `.asmdef` files that have `"includePlatforms": ["Editor"]` and reference `UnityEngine.TestRunner` / `UnityEditor.TestRunner`. Without these, tests won't appear.
- **UniTask not compiling in tests** — The test assembly's `.asmdef` must reference `UniTask` if any test code uses UniTask types. Even if S01 tests don't use async, downstream slices will need this.
- **Static state creeping in** — Easy to accidentally add `static` fields for convenience. The grep check (`grep -r "static " --include="*.cs"`) at milestone end will catch this, but better to never introduce it.

## Open Risks

- **UniTask git URL resolution on first import** — UPM git packages require `git` to be on PATH. If the machine's git isn't configured, package resolution will fail silently. Should verify git is available before creating the project.
- **Presenter base class design may need iteration** — The `Presenter<TView>` pattern (generic base with view interface as type parameter) is clean but may need lifecycle methods (Initialize, Dispose) that aren't obvious until S02/S03 flesh out screen/popup lifecycles. Design for extensibility now.
- **Edit-mode test runner in batchmode** — Running tests via `Unity.exe -runTests -batchmode -testPlatform editmode` should work but may have quirks with Unity 6. First run will validate this path.
- **Unity version mismatch** — The spec originally called for 6000.3.10f1 but we're using 6000.3.4f1. Both are Unity 6.3 LTS. No API differences expected for our use case, but noting the deviation.

## Forward Intelligence

These findings are specifically for downstream slices building on S01's outputs:

- **IView marker interface** — Keep it empty (`interface IView { }`). Screen and popup view interfaces will extend it. Don't put lifecycle methods here — those belong in more specific interfaces (IScreenView, IPopupView) defined in S02/S03.
- **Presenter\<TView\> needs Dispose** — S02 will need to clean up presenters when screens unload. Add a virtual `Dispose()` or `Cleanup()` method to the base presenter now, even if S01 tests don't exercise it.
- **UIFactory should accept services at construction time** — The factory is constructed once (in boot, S05) with all domain services. Each `CreateXxxPresenter(IXxxView view)` method uses those stored services to construct the presenter. This pattern is clean for the demo but may need per-screen service subsets at scale.
- **UniTask in test assemblies** — S02/S03 tests will need async test support. The test `.asmdef` should reference UniTask from the start so it's ready.
- **Scene names for S02** — The screen manager in S02 will need scene name constants or an enum. S01 doesn't need to define these, but the folder structure should leave room for a `Constants/` or `Config/` directory.

## Skills Discovered

| Technology | Skill | Status |
|------------|-------|--------|
| Unity (general) | `wshobson/agents@unity-ecs-patterns` (3.1K installs) | available — ECS-focused, not directly relevant to MVP/uGUI work |
| Unity (general) | `rmyndharis/antigravity-skills@unity-developer` (568 installs) | available — general Unity dev, could be useful |
| UniTask | `creator-hian/claude-code-plugins@unity-unitask` (8 installs) | available — very low install count, evaluate cautiously |

**Recommendation:** None of the discovered skills are a strong match for this MVP/uGUI architecture work. The `unity-developer` skill might provide general guidance but has modest adoption. The `unity-unitask` skill has very few installs. Skip skill installation for now — the Context7 UniTask docs and Unity test framework docs are sufficient.

## Requirements Coverage

This slice owns or supports the following requirements:

| Req | Role | What S01 Must Deliver |
|-----|------|-----------------------|
| R001 | **primary owner** | MVP base types: IView, Presenter\<TView\>, concrete view interface pattern |
| R002 | **primary owner** | View independence pattern — view interface has no backward refs |
| R003 | **primary owner** | Interface-per-view convention — one interface per view, presenter depends on interface |
| R004 | **primary owner** | UIFactory — central factory that constructs presenters with wired deps |
| R005 | **primary owner** | Constructor injection pattern — proven by factory + presenter construction |
| R006 | **primary owner** | No static state — established as convention, verified by grep |
| R007 | **primary owner** | Domain services pattern — at least one example service injected into a presenter |
| R014 | **primary owner** | UniTask installed and compiling in the project |
| R015 | **primary owner** | Edit-mode test infrastructure — test assembly, first passing tests |
| R017 | **primary owner** | Isolation testability — presenter tested with mocked view, no Unity runtime needed |

## Key Implementation Details

### Unity Project Creation (CLI)

```
"C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" -batchmode -createProject "C:\OtherWork\simplegame" -quit
```

This generates the standard Unity project structure. Since the current directory already has `.git` and `.gsd`, the Unity project files will be created alongside them.

### UniTask Installation

Add to `Packages/manifest.json` before first editor open:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
```

No version pin needed for initial setup — latest main branch is stable (v2.5.x). Can pin later with `#2.5.10` suffix if needed.

### Assembly Definition Structure

- `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime code, references UniTask
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — edit-mode tests, references runtime assembly + UniTask + test framework

### Running Tests (CLI)

```
"C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe" -batchmode -runTests -projectPath "C:\OtherWork\simplegame" -testPlatform editmode -testResults "C:\OtherWork\simplegame\TestResults.xml" -quit
```

## Sources

- UniTask installation and API patterns (source: [Context7 UniTask docs](/cysharp/unitask))
- Unity CLI `-createProject` and `-runTests` commands (source: [Unity 6.3 Editor CLI docs](https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html))
- Unity Test Framework 1.6.0 ships with 6000.3.4f1 (source: local inspection of `C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Data\Resources\PackageManager\BuiltInPackages\com.unity.test-framework\package.json`)
- UniTask vs Unity 6 Awaitable comparison (source: [UniTask README on GitHub](https://github.com/Cysharp/UniTask))
