---
estimated_steps: 5
estimated_files: 5
---

# T01: Create Unity project, install UniTask, set up folder structure and assembly definitions

**Slice:** S01 — Core MVP Infrastructure & Project Setup
**Milestone:** M001

## Description

Create the Unity 6000.3.4f1 project headlessly via CLI batchmode, add UniTask to the package manifest before first full editor open, establish the folder structure for runtime code and tests, and create assembly definition files that wire up the reference chain (test → runtime → UniTask). This is the foundation everything else compiles against.

## Steps

1. Run `Unity.exe -batchmode -createProject "C:\OtherWork\simplegame" -quit` to generate the standard Unity project structure (`Assets/`, `Packages/manifest.json`, `ProjectSettings/`, etc.). Since the directory already has `.git` and `.gsd`, Unity will add its files alongside them.
2. Edit `Packages/manifest.json` to add UniTask: `"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"` in the dependencies block.
3. Create folder structure:
   - `Assets/Scripts/Core/MVP/`
   - `Assets/Scripts/Core/Services/`
   - `Assets/Tests/EditMode/`
4. Create `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime assembly definition referencing `UniTask`:
   ```json
   {
     "name": "SimpleGame.Runtime",
     "rootNamespace": "SimpleGame",
     "references": ["UniTask"],
     "includePlatforms": [],
     "excludePlatforms": [],
     "allowUnsafeCode": false,
     "overrideReferences": false,
     "precompiledReferences": [],
     "autoReferenced": true,
     "defineConstraints": [],
     "versionDefines": [],
     "noEngineReferences": false
   }
   ```
5. Create `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — test assembly definition:
   ```json
   {
     "name": "SimpleGame.Tests.EditMode",
     "rootNamespace": "SimpleGame.Tests",
     "references": [
       "SimpleGame.Runtime",
       "UniTask",
       "UnityEngine.TestRunner",
       "UnityEditor.TestRunner"
     ],
     "includePlatforms": ["Editor"],
     "excludePlatforms": [],
     "allowUnsafeCode": false,
     "overrideReferences": true,
     "precompiledReferences": ["nunit.framework.dll"],
     "autoReferenced": false,
     "defineConstraints": ["UNITY_INCLUDE_TESTS"],
     "versionDefines": [],
     "noEngineReferences": false
   }
   ```
6. Update `.gitignore` with Unity-standard ignores (Library/, Temp/, Logs/, obj/, *.csproj, *.sln, etc.).
7. Open project in batchmode to trigger package resolution and compilation: `Unity.exe -batchmode -projectPath "C:\OtherWork\simplegame" -quit`. Verify exit code 0 and no compilation errors in the Editor.log.

## Must-Haves

- [ ] Unity project structure exists with `Assets/`, `Packages/`, `ProjectSettings/`
- [ ] UniTask is in `manifest.json` and resolves on project open
- [ ] Folder structure created: `Assets/Scripts/Core/MVP/`, `Assets/Scripts/Core/Services/`, `Assets/Tests/EditMode/`
- [ ] Runtime `.asmdef` references UniTask
- [ ] Test `.asmdef` references runtime assembly, UniTask, and test framework
- [ ] Project compiles cleanly in batchmode (exit code 0)

## Observability Impact

- **New signals introduced:** `Packages/packages-lock.json` (package resolution confirmation), `Logs/Editor.log` (compilation errors, UniTask import messages), `Library/Bee/` (incremental build cache).
- **Inspection command:** `grep -i "error\|exception\|unitask\|failed" /c/OtherWork/simplegame/Logs/Editor.log | tail -40`
- **Failure state:** If batchmode exits non-zero, `Editor.log` will contain `CompileError` or `error CS` lines. Assembly reference failures appear as `will not be loaded due to errors`.
- **Future agent inspection:** After this task, a future agent can run `Unity.exe -batchmode -projectPath ... -quit` and check `$LASTEXITCODE` plus `Editor.log` to verify the project baseline is healthy.

## Verification

- Unity batchmode open completes with exit code 0
- Editor.log shows no compilation errors
- `Packages/manifest.json` contains the UniTask entry
- Both `.asmdef` files exist with correct references

## Inputs

- Unity Editor at `C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe`
- Empty project directory with `.git` and `.gsd` already present
- UniTask git URL: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

## Expected Output

- `Packages/manifest.json` — with UniTask dependency added
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime assembly definition
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — test assembly definition
- `Assets/Scripts/Core/MVP/` — empty directory ready for T02
- `Assets/Scripts/Core/Services/` — empty directory ready for T02
- `Assets/Tests/EditMode/` — directory with `.asmdef`, ready for T03
- `.gitignore` — updated with Unity-standard entries
