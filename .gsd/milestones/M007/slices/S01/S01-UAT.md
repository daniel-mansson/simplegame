# S01: IViewResolver + Container Refactor — UAT

**Milestone:** M007
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S01 is a pure compile-time and test-time refactor. No runtime behavior changed — popup show/hide still works via PopupId, the container is still in Boot scene, all game logic is identical. The correctness proofs are grep verification (rename complete), code inspection (interface + dual implementation), and test execution (5 new tests + 164+ regression gate). No new user-visible behavior to manually exercise.

## Preconditions

- Unity 6 (6000.3.10f1) project at `C:\OtherWork\simplegame` on `main` branch
- Unity Editor open (for test runner) OR Unity batchmode available
- All git changes committed (working tree clean — `git status` shows nothing to commit)
- K003 workaround applied: Unity Editor has been focused or restarted since `ViewContainerTests.cs` was created, so the new test file is compiled and visible to the test runner

## Smoke Test

Run: `rg "UnityPopupContainer" Assets/`

**Expected:** Command exits with code 1 and produces zero output. Any match means the rename is incomplete and the codebase will not compile.

## Test Cases

### 1. Rename Complete — No Stale References

**Purpose:** Prove `UnityPopupContainer` is fully erased from the codebase.

1. Open a terminal in `C:\OtherWork\simplegame`
2. Run: `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/`
3. **Expected:** Exit code 1, zero output
4. Run: `rg "UnityPopupContainer" Assets/`
5. **Expected:** Exit code 1, zero output (covers .unity scene files and any other asset types)

### 2. IViewResolver Interface Exists in Core

**Purpose:** Prove the new interface is in the correct namespace and location.

1. Run: `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/`
2. **Expected:** Matches `IViewResolver.cs` containing `public interface IViewResolver`
3. Run: `cat Assets/Scripts/Core/PopupManagement/IViewResolver.cs`
4. **Expected:** File contains `T Get<T>() where T : class;` in namespace `SimpleGame.Core.PopupManagement`

### 3. UnityViewContainer Implements Both Interfaces

**Purpose:** Prove the container implements the full dual-interface contract.

1. Run: `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/`
2. **Expected:** Matches line reading `public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver`
3. Run: `rg "GetComponentInChildren" Assets/Scripts/Game/Popup/UnityViewContainer.cs`
4. **Expected:** Matches `Get<T>()` body returning `GetComponentInChildren<T>(true)`

### 4. All Reference Files Updated

**Purpose:** Prove every file that previously referenced `UnityPopupContainer` now references `UnityViewContainer`.

1. Run: `grep "UnityViewContainer" Assets/Scripts/Game/Boot/GameBootstrapper.cs`
2. **Expected:** Finds `FindFirstObjectByType<UnityViewContainer>()` (type reference updated)
3. Run: `grep "UnityViewContainer" Assets/Editor/SceneSetup.cs`
4. **Expected:** Finds `AddComponent<UnityViewContainer>()` (type reference updated)
5. Run: `grep "UnityViewContainer" Assets/Scenes/Boot.unity`
6. **Expected:** Finds `m_EditorClassIdentifier` containing `UnityViewContainer` (scene file patched)

### 5. GUID Preservation (Meta File Integrity)

**Purpose:** Prove the `.meta` GUID was preserved by `git mv` so Boot scene serialization is intact.

1. Run: `git log --oneline Assets/Scripts/Game/Popup/ | head -5`
2. **Expected:** Log shows a rename commit (e.g. "rename UnityPopupContainer → UnityViewContainer")
3. Run: `git status Assets/Scripts/Game/Popup/`
4. **Expected:** Clean working tree — no deleted or untracked files

### 6. New Tests Exist (5 Tests, MockViewResolver Present)

**Purpose:** Prove ViewContainerTests.cs was created with the required test count and mock double.

1. Run: `rg "\[Test\]" Assets/Tests/EditMode/Game/ViewContainerTests.cs`
2. **Expected:** 5 matches (one per test method)
3. Run: `rg "MockViewResolver" Assets/Tests/EditMode/Game/ViewContainerTests.cs`
4. **Expected:** Finds class definition `internal class MockViewResolver : IViewResolver` and usage in tests

### 7. Get<T>() Tests Pass in Unity Test Runner

**Purpose:** Prove the IViewResolver contract works — especially inactive-child resolution.

1. Open Unity Editor at `C:\OtherWork\simplegame`
2. Open Window → General → Test Runner
3. Run `EditMode` tests in `SimpleGame.Tests.Game`
4. Verify these 5 tests all show green:
   - `ViewContainerGetTests.Get_ReturnsCorrectInterface`
   - `ViewContainerGetTests.Get_ReturnsNull_WhenInterfaceNotFound`
   - `ViewContainerGetTests.Get_FindsInactiveChild`
   - `MockViewResolverTests.MockViewResolver_ReturnsRegistered`
   - `MockViewResolverTests.MockViewResolver_ReturnsNull_WhenNotRegistered`
5. **Expected:** All 5 pass. Total test count ≥ 169 (164 pre-existing + 5 new).

### 8. No Regression in Existing Tests

**Purpose:** Prove the rename did not break any of the 164 pre-existing tests.

1. In Unity Test Runner, run all EditMode tests
2. **Expected:** All 164+ pre-existing tests still pass. Zero failures, zero errors from the rename.

## Edge Cases

### Boot Scene Opens Without Errors

1. Open `Assets/Scenes/Boot.unity` in the Unity Editor
2. Select the `UnityViewContainer` GameObject (formerly `UnityPopupContainer`)
3. **Expected:** Inspector shows `UnityViewContainer` component with all 6 `[SerializeField]` popup references populated. No "Missing Script" warning. No yellow console warnings about missing component.

### Play Mode: Popup Show/Hide Unchanged

1. Enter Play Mode from the Boot scene
2. Navigate to a point where a popup would be shown (e.g. LevelComplete flow)
3. **Expected:** Popup appears and dismisses exactly as it did in M006 — no behavioral change from the rename

### MockViewResolver Available as S02 Test Double

1. In a new test file (simulating S02 usage), add `using SimpleGame.Tests.Game;`
2. Instantiate: `var resolver = new MockViewResolver();`
3. Register a view: `resolver.Register<ILevelCompleteView>(someView);`
4. Resolve: `var result = resolver.Get<ILevelCompleteView>();`
5. **Expected:** Compiles cleanly; `result` equals `someView`

## Failure Signals

- `rg "UnityPopupContainer" Assets/` returns exit 0 with matches → rename incomplete; Unity will emit CS0246 on next compile
- Unity Console shows "Missing Script" on the `UnityViewContainer` GameObject in Boot → `.meta` GUID was not preserved; need to re-run `git mv` and restore the original `.meta` content
- Unity Console shows CS0246 `UnityPopupContainer` type not found → a reference file was missed; run `rg "UnityPopupContainer" Assets/` to identify it
- `ViewContainerGetTests.Get_FindsInactiveChild` fails → `GetComponentInChildren<T>(true)` not being called with `includeInactive=true`; check the `Get<T>()` implementation in `UnityViewContainer.cs`
- Test count drops below 164 → a test file failed to compile due to stale reference; run `rg "UnityPopupContainer" Assets/Tests/` to find it

## Requirements Proved By This UAT

- R070 — `IViewResolver` interface exists in `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` with `T Get<T>() where T : class`; 5 tests prove the contract works including inactive-child resolution
- R071 — `UnityViewContainer` implements both `IPopupContainer<PopupId>` and `IViewResolver`; old name fully erased (`rg "UnityPopupContainer" Assets/` exit 1)

## Not Proven By This UAT

- R069 (popup views as individual prefabs) — container structure in Boot scene is present but full prefab organization is a Unity Inspector/scene layout concern; inspector visual check in Boot scene is the only proof available at this stage; full prefab migration is S02/S03 work
- R075 (zero FindFirstObjectByType in production) — `GameBootstrapper` still uses `FindFirstObjectByType` for the container and all scene controllers; this is deferred to S02 (SerializeField refs) and S03 (scene root convention)
- R076 (164+ tests passing in Unity batchmode) — new test file `ViewContainerTests.cs` subject to K003 (domain-reload-disabled editor); batchmode run should be confirmed after S02 with a fresh editor reload
- R077 (game flow identical) — no runtime behavior changed in S01, but full UAT play-through is deferred to M007 completion (S03)

## Notes for Tester

- If the Unity Editor was open during `ViewContainerTests.cs` creation, you may need to focus the Editor window (or restart Unity) to trigger recompilation before the test runner sees the new file (K003 — domain-reload-disabled editor).
- The `m_EditorClassIdentifier` in Boot.unity is a display-only string — it affects the Inspector label but not runtime binding. Even if this were stale, the game would work. The patch was applied for correctness and to keep `rg "UnityPopupContainer" Assets/` clean.
- `MockViewResolver` is in namespace `SimpleGame.Tests.Game` inside `ViewContainerTests.cs` — it is intentionally `internal`, scoped to the test assembly. S02 test authors should reference it from within the same test assembly (no import needed, same namespace).
