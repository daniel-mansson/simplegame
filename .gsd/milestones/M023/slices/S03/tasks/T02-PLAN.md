---
estimated_steps: 25
estimated_files: 3
skills_used: []
---

# T02: Wire CameraConfig asset in SceneSetup and run scene generation

Create a CameraConfig ScriptableObject asset in SceneSetup.CreateInGameScene() and wire it to the CameraController's [SerializeField] _config field. This closes the last gap — without this wiring, the CameraController's Config property returns null and all camera features (auto-tracking, level-start sequence, manual input) are silently skipped.

## Steps

1. **SceneSetup.CreateInGameScene()** — After the existing `cam.gameObject.AddComponent<CameraController>();` line (around line 385), add CameraConfig asset creation and wiring:
   ```csharp
   // Create or load CameraConfig asset
   var cameraConfigPath = "Assets/Data/CameraConfig.asset";
   var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(cameraConfigPath);
   if (cameraConfig == null)
   {
       cameraConfig = ScriptableObject.CreateInstance<CameraConfig>();
       if (!System.IO.Directory.Exists("Assets/Data"))
           System.IO.Directory.CreateDirectory("Assets/Data");
       AssetDatabase.CreateAsset(cameraConfig, cameraConfigPath);
   }
   var camController = cam.gameObject.GetComponent<CameraController>();
   WireSerializedField(camController, "_config", cameraConfig);
   ```
   IMPORTANT: Get the `CameraController` reference via `cam.gameObject.GetComponent<CameraController>()` since AddComponent was already called. Wire to `_config` which is the `[SerializeField] private CameraConfig _config` field.

2. **Run SceneSetup** — Execute `Tools/Setup/Create And Register Scenes` via Unity MCP to regenerate all scene files. Per K007, scene files must be regenerated after SerializeField wiring changes.

3. **Verify** — Confirm `CameraConfig.asset` exists at `Assets/Data/CameraConfig.asset` and the InGame scene file references it.

## Must-Haves

- [ ] CameraConfig asset created at `Assets/Data/CameraConfig.asset` if it doesn't exist, loaded if it does
- [ ] CameraController's `_config` field wired to the CameraConfig asset in the InGame scene
- [ ] SceneSetup run successfully — all 4 scene files regenerated
- [ ] All existing 358+ EditMode tests still pass

## Inputs

- ``Assets/Editor/SceneSetup.cs` — existing scene setup with CreateInGameScene adding CameraController via AddComponent`
- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — CameraConfig ScriptableObject with all fields (from T01)`
- ``Assets/Scripts/Game/InGame/CameraController.cs` — CameraController with [SerializeField] private CameraConfig _config (from T01)`

## Expected Output

- ``Assets/Editor/SceneSetup.cs` — CameraConfig asset creation and wiring added to CreateInGameScene`
- ``Assets/Data/CameraConfig.asset` — created by SceneSetup run`
- ``Assets/Scenes/InGame.unity` — regenerated with CameraConfig wired to CameraController`

## Verification

grep -c "CameraConfig" Assets/Editor/SceneSetup.cs returns >= 2 && test -f Assets/Data/CameraConfig.asset (after SceneSetup run) && Unity EditMode test runner: all tests pass (0 failures)
