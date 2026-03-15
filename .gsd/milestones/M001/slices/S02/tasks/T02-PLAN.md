---
estimated_steps: 5
estimated_files: 4
---

# T02: Create placeholder scenes and register in EditorBuildSettings

**Slice:** S02 — Screen Management
**Milestone:** M001

## Description

Create the placeholder MainMenu and Settings scenes that ScreenManager will load at runtime, and register them in Unity's EditorBuildSettings so `SceneManager.LoadSceneAsync` can find them. Scene files cannot be hand-written (complex binary/YAML format) — they must be created via `EditorSceneManager.NewScene()` + `SaveScene()` in an Editor script executed via batchmode `-executeMethod`. This task closes the integration gap between ScreenManager's tested navigation logic and Unity's actual scene loading system.

## Steps

1. Create `Assets/Editor/SceneSetup.cs` — a static class with a `[MenuItem]`-attributed method and a static `CreateAndRegisterScenes()` method callable via `-executeMethod`. The method:
   - Creates `Assets/Scenes/` directory if it doesn't exist
   - Uses `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` to create a blank scene
   - Saves it via `EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity")`
   - Repeats for Settings.unity
   - Builds an `EditorBuildSettingsScene[]` array with both scenes enabled
   - Assigns to `EditorBuildSettings.scenes`
   - Logs success for verification
2. Create an assembly definition for the Editor folder or ensure SceneSetup.cs is in an Editor-only location (Unity auto-compiles `Assets/Editor/` as editor-only, no asmdef needed if using the default Assembly-CSharp-Editor)
3. Run batchmode: `Unity.exe -batchmode -projectPath C:\OtherWork\simplegame -executeMethod SceneSetup.CreateAndRegisterScenes -logFile Logs/scene-setup.log -quit`
4. Verify scene files exist on disk: `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Settings.unity`
5. Verify `ProjectSettings/EditorBuildSettings.asset` now contains both scene paths in `m_Scenes`

## Must-Haves

- [ ] `Assets/Scenes/MainMenu.unity` exists as a valid Unity scene file
- [ ] `Assets/Scenes/Settings.unity` exists as a valid Unity scene file
- [ ] Both scenes are registered in `EditorBuildSettings.scenes` (enabled)
- [ ] Editor script is in an editor-only location (won't compile into runtime builds)
- [ ] Project still compiles cleanly after scene creation

## Verification

- Batchmode `-executeMethod` exits successfully (exit code 0)
- `ls Assets/Scenes/MainMenu.unity Assets/Scenes/Settings.unity` → both exist
- `grep -c "MainMenu\|Settings" ProjectSettings/EditorBuildSettings.asset` → at least 2 matches
- Batchmode compile after scene creation → exit 0, zero `error CS`
- Full test suite still passes (re-run edit-mode tests to confirm no regression)

## Inputs

- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — scene names must match the `ToSceneName()` output (from T01)
- `ProjectSettings/EditorBuildSettings.asset` — currently has `m_Scenes: []`
- S01 forward intelligence: batchmode works reliably for project operations

## Expected Output

- `Assets/Editor/SceneSetup.cs` — Editor-only script for scene creation and registration
- `Assets/Scenes/MainMenu.unity` — empty placeholder scene
- `Assets/Scenes/Settings.unity` — empty placeholder scene
- `ProjectSettings/EditorBuildSettings.asset` — modified to include both scenes


## Observability Impact

**Signals added by this task:**

- `ProjectSettings/EditorBuildSettings.asset` — `m_Scenes` array populated with `MainMenu` and `Settings` paths; inspect to verify registration
- `Assets/Scenes/MainMenu.unity` / `Assets/Scenes/Settings.unity` — presence on disk is the primary deliverable signal; absence means batchmode failed
- `Logs/scene-setup.log` — batchmode log for the `-executeMethod` run; contains `[SceneSetup]` prefixed success lines and any `error CS` compile output
- `Logs/compile-verify.log` — second batchmode run log confirming project still compiles after scene creation

**Failure visibility:**
- Batchmode exit code non-zero → read `Logs/scene-setup.log` for error CS or exception stack
- Scenes missing → `EditorSceneManager.SaveScene` failed; check log for path errors or permission issues
- `EditorBuildSettings.asset` unchanged → `EditorBuildSettings.scenes` assignment failed silently; check log for exceptions after assignment
- Tests regress after this task → scene file import caused a recompile issue; run full test suite batchmode and inspect `TestResults.xml`
