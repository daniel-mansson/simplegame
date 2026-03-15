---
id: T02
parent: S02
milestone: M001
provides:
  - Assets/Scenes/MainMenu.unity — placeholder scene for MainMenu screen
  - Assets/Scenes/Settings.unity — placeholder scene for Settings screen
  - EditorBuildSettings.scenes populated with both scenes (enabled)
  - Assets/Editor/SceneSetup.cs — editor-only batchmode-callable scene creator/registrar
key_files:
  - Assets/Editor/SceneSetup.cs
  - Assets/Scenes/MainMenu.unity
  - Assets/Scenes/Settings.unity
  - ProjectSettings/EditorBuildSettings.asset
key_decisions:
  - SceneSetup.cs placed in Assets/Editor/ (no asmdef needed; Unity auto-compiles as editor-only via default Assembly-CSharp-Editor)
  - NewSceneSetup.EmptyScene + NewSceneMode.Single used for each scene — creates a completely empty scene (no default camera/light) to minimize scene complexity
patterns_established:
  - Editor batchmode -executeMethod pattern for asset creation: static class in Assets/Editor/, no namespace required, method called as ClassName.MethodName
observability_surfaces:
  - ProjectSettings/EditorBuildSettings.asset — m_Scenes array; grep for MainMenu/Settings to verify registration
  - Assets/Scenes/MainMenu.unity + Settings.unity — presence on disk is the primary signal
  - Logs/scene-setup.log — batchmode log with [SceneSetup] prefixed success lines
  - Logs/compile-verify.log — compile-only batchmode pass confirming no regression
duration: ~10m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T02: Create placeholder scenes and register in EditorBuildSettings

**Created MainMenu.unity and Settings.unity placeholder scenes via batchmode `-executeMethod` and registered both in EditorBuildSettings; all 14 tests still pass.**

## What Happened

1. **Pre-flight fix**: Added `## Observability Impact` section to `T02-PLAN.md` as required.

2. **Created `Assets/Editor/SceneSetup.cs`** — static class with a `CreateAndRegisterScenes()` static method callable via `-executeMethod`. The method:
   - Creates `Assets/Scenes/` directory if absent
   - Uses `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` + `SaveScene()` for each scene
   - Builds an `EditorBuildSettingsScene[]` array with both scenes enabled
   - Assigns to `EditorBuildSettings.scenes`
   - Calls `AssetDatabase.Refresh()` to ensure Unity picks up the new files
   - Logs `[SceneSetup]` prefixed messages for verification

3. **Ran batchmode** `-executeMethod SceneSetup.CreateAndRegisterScenes` → exit code 0. All five `[SceneSetup]` log lines appeared in `Logs/scene-setup.log`. No `error CS` in log.

4. **Verified scene files**: Both `Assets/Scenes/MainMenu.unity` (3539 bytes) and `Assets/Scenes/Settings.unity` (3539 bytes) exist as valid YAML Unity scene files.

5. **Verified EditorBuildSettings.asset**: `m_Scenes` now contains both entries with `enabled: 1` and correct GUIDs.

6. **Compile verification**: Second batchmode pass (no `-executeMethod`) → exit code 0, zero `error CS`.

7. **Test suite**: Batchmode `-runTests -testPlatform EditMode` → `TestResults.xml` shows `result="Passed" total="14" passed="14" failed="0"`. No regressions.

## Verification

- **Batchmode scene creation**: exit code 0; `Logs/scene-setup.log` contains all 5 `[SceneSetup]` lines
- **Scene files on disk**: `ls Assets/Scenes/` → `MainMenu.unity`, `MainMenu.unity.meta`, `Settings.unity`, `Settings.unity.meta`
- **EditorBuildSettings**: `grep -c "MainMenu\|Settings" ProjectSettings/EditorBuildSettings.asset` → 3 (path + guid for each + directory name = well above threshold of 2)
- **Compile clean**: batchmode compile exit 0, zero `error CS` in `Logs/compile-verify.log`
- **Test suite**: 14/14 passed, 0 failures — no regression from scene files being added

### Slice-level verification status (S02)
- ✅ `ScreenManagerTests.cs` — 8 edit-mode tests pass: `TestResults.xml` shows all passing, 0 failures
- ✅ Static state guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output (exit 1)
- ✅ Compilation clean: batchmode compile exits 0 with zero `error CS`
- ✅ Scenes registered: `EditorBuildSettings.asset` contains MainMenu and Settings entries with `enabled: 1`

**All S02 slice-level verification checks pass.**

## Diagnostics

- `ProjectSettings/EditorBuildSettings.asset` — inspect `m_Scenes` array for path and enabled state
- `Assets/Scenes/MainMenu.unity` / `Assets/Scenes/Settings.unity` — YAML scene files; presence + non-zero size confirms creation
- `Logs/scene-setup.log` — `-executeMethod` run log; `[SceneSetup]` lines confirm execution path; `error CS` lines indicate compile failure
- `Logs/compile-verify.log` — second batchmode pass; grep for `error CS` to confirm clean compile
- `TestResults.xml` — `result` attribute on `test-run` element; `passed`/`failed` attributes for counts

## Deviations

None. All steps executed as written in the task plan. `NewSceneSetup.EmptyScene + NewSceneMode.Single` chosen for simplest possible scene (no camera, no light) — consistent with the plan's intent of placeholder scenes.

## Known Issues

None.

## Files Created/Modified

- `Assets/Editor/SceneSetup.cs` — created: editor-only static class with batchmode-callable scene creator/registrar
- `Assets/Scenes/MainMenu.unity` — created: empty placeholder scene for MainMenu screen
- `Assets/Scenes/Settings.unity` — created: empty placeholder scene for Settings screen
- `ProjectSettings/EditorBuildSettings.asset` — modified: `m_Scenes` populated with MainMenu and Settings entries (enabled)
- `.gsd/milestones/M001/slices/S02/tasks/T02-PLAN.md` — updated: added `## Observability Impact` section (pre-flight fix)
- `Logs/scene-setup.log` — created: batchmode log for -executeMethod run
- `Logs/compile-verify.log` — created: batchmode log for compile verification pass
