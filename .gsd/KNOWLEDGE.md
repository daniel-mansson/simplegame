# Knowledge

Append-only register of project-specific rules, patterns, and lessons learned.

---

### K001 — Unity MCP: mcporter call syntax for args with spaces
**Date:** 2026-03-16

When calling Unity MCP tools via `mcporter call`, arguments with spaces in their values break when passed as positional `key:value` pairs (mcporter splits on whitespace). Two workarounds:

1. **Shell quoting:** Wrap the entire key:value in quotes — `mcporter call unityMCP.execute_menu_item --output raw "menu_path:Tools/Setup/Create And Register Scenes"`
2. **JSON args flag:** Use `--args '{"menu_path": "Tools/Setup/Create And Register Scenes"}'`

When using the `mcp_call` tool from pi, it handles JSON args natively, but the underlying mcporter CLI invocation can still hit this. If `mcp_call` fails with "Too many positional arguments", fall back to a `Bash` call with explicit quoting.

---

### K002 — Unity MCP: read_console for compiler errors
**Date:** 2026-03-16

The Unity MCP server exposes `read_console` (not `get_logs` or `get_console_logs`) for reading Unity Editor console output. Use it to check for compile errors:

```
mcporter call unityMCP.read_console --output raw --args '{"count": 30}'
```

Or via `mcp_call`:
```
mcp_call(server: "unityMCP", tool: "read_console", args: {"count": 30})
```

---

### K003 — Domain-reload-disabled editor: new test files not detected
**Date:** 2026-03-16

When Unity runs with domain reload disabled (Enter Play Mode Settings), newly created test `.cs` files (e.g. `PopupTests.cs`) are not picked up by the test runner until the editor is restarted or domain reload is manually triggered. The same applies to changes in editor scripts like `SceneSetup.cs` — the old compiled version continues to run. This also affects `Assets/Refresh` — it refreshes the asset database but doesn't recompile scripts.

**Workaround:** User must focus the Unity Editor window (which may trigger recompilation) or restart Unity.

---

### K004 — IInputBlocker interface: keep mocks in sync
**Date:** 2026-03-16
**Updated:** 2026-03-18

When adding members to `IInputBlocker` (in `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs`), all mock implementations in test files must be updated. Known mocks:
- `MockInputBlocker` in `Assets/Tests/EditMode/Core/PopupManagerTests.cs`
- `MockInputBlockerGame` in `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `MockInputBlockerForInGame` in `Assets/Tests/EditMode/Game/InGameTests.cs`

Additionally, when adding members to view interfaces that extend `IPopupView` (e.g. `ILevelCompleteView`), mocks in these files also need updating:
- `MockLevelCompleteView`, `MockLevelFailedView`, `MockRewardedAdView`, `MockIAPPurchaseView`, `MockObjectRestoredView` in `Assets/Tests/EditMode/Game/PopupTests.cs`
- `MockConfirmDialogView` in `Assets/Tests/EditMode/Game/DemoWiringTests.cs`

---

### K005 — Unity scene file m_EditorClassIdentifier must be updated after class rename
**Date:** 2026-03-17

When renaming a MonoBehaviour class (e.g. `UnityPopupContainer` → `UnityViewContainer`), `git mv` preserves the `.meta` GUID so the scene's `m_Script` binding continues to resolve correctly at runtime. However, the `.unity` scene file also stores `m_EditorClassIdentifier` as a plain string containing the old class name. This does not break runtime binding, but shows the stale name in the Unity Inspector and will cause a `rg "OldName" Assets/` check to return a match.

**Fix:** After `git mv`, run `sed -i 's/OldClassName/NewClassName/g' Assets/Scenes/Boot.unity` (or the relevant scene file). Always run `rg "OldName" Assets/` across all file types (not just `.cs`) to catch scene file references.


---

### K006 — mcporter run_tests on Windows: UV_HANDLE_CLOSING assertion crash
**Date:** 2026-03-17

`mcporter call unityMCP.run_tests testMode:EditMode` consistently crashes on Windows with:
```
Assertion failed: !(handle->flags & UV_HANDLE_CLOSING), file src\win\async.c, line 76
```
This is a libuv/mcporter issue where the test runner's long-running async response triggers a handle lifecycle conflict on Windows. The same crash occurs with `batch_execute` wrapping `run_tests`.

**Workaround:** Use stdin pipe mode to bypass the CLI arg handling path:
```bash
echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin
```
This returns `{"job_id": "...", "status": "running", ...}` immediately.

Poll for results with (this works fine via normal CLI):
```bash
mcporter call unityMCP.get_test_job job_id=<job_id>
```
When `status == "succeeded"`, check `result.summary.total`, `.passed`, `.failed`.

Note: `mcp_call(server: "unityMCP", tool: "run_tests", args: {...})` via the pi tool also triggers the same crash. The stdin pipe workaround is the only reliable method.

---

### K007 — SceneSetup must be re-run after any SerializeField change on GameBootstrapper
**Date:** 2026-03-18

After adding or changing `[SerializeField]` fields on `GameBootstrapper` (or any Boot scene MonoBehaviour), the Boot scene file is NOT automatically updated. `SceneSetup.cs` holds the programmatic wiring logic, but it only runs when `Tools/Setup/Create And Register Scenes` is executed from the Unity Editor menu.

**Symptom:** Fields appear correctly in `.cs` source but are null at runtime — Unity serializes the old field layout from the `.unity` file.

**Fix:** Run `Tools/Setup/Create And Register Scenes` from the Unity Editor menu (or via `mcporter call unityMCP.execute_menu_item --output raw "menu_path:Tools/Setup/Create And Register Scenes"`). Commit the resulting changes to all four `.unity` scene files.

**Rule:** Any task that adds a `[SerializeField]` to a Boot scene component AND updates `SceneSetup.cs` is not complete until SceneSetup has been run and the scene file diff has been committed.


---

### K008 — Always clean up empty folders after moves
**Date:** 2026-03-18

After `git mv` reorganizations, empty source directories are left behind as untracked filesystem entries. Git doesn't track empty directories — only their `.meta` files (in Unity projects). The `.meta` file for the now-empty folder remains staged/committed as a dangling reference.

**Rule:** After any `git mv` that empties a folder, immediately:
1. `rmdir <empty-folder>` — removes the directory
2. `git rm <empty-folder>.meta` — removes the orphaned meta from tracking
3. Include both in the same commit as the move

**Example:** Moving `Assets/Prefabs/UI/Popups/` contents to `Assets/Prefabs/Game/Popups/` left `Assets/Prefabs/UI/Popups/` empty. The `.meta` file persisted in the repo until explicitly removed.

---

### K009 — simple-jigsaw package: scripts outside Assets/SimpleJigsaw/ are not exported
**Date:** 2026-03-18

The `simple-jigsaw` package.json is at `Assets/SimpleJigsaw/package.json` in the source repo. Unity sees the package as rooted at `Assets/SimpleJigsaw/`. Scripts in `Assets/Scenes/` (like `PuzzleSceneDriver.cs`) and `Assets/Textures/` etc. are part of the source project, NOT the package — they are NOT visible when the package is referenced via the local path. To use them, copy them into `Assets/JigsawDemo/` or similar project-local location.

**Also:** The package's `Runtime/Configs/PieceRenderConfig.asset` has a stale shader GUID (`50f401a8...`) from the original source project. The PuzzlePiece shader in our submodule has GUID `c67764c5...`. Always use the project-local `DemoPieceRenderConfig.asset` or create a fresh `PieceRenderConfig` asset in `Assets/` with the correct GUID.


---

### K010 — com.unity.ads.ios-support: SKAdNetwork background fetcher blocks Unity MCP reconnect
**Date:** 2026-03-20

Adding `com.unity.ads.ios-support` to `Packages/manifest.json` causes Unity MCP to become unresponsive after the domain reload triggered by the package import. The package's `PostProcessBuildPlist.cs` and associated editor scripts make a background network call to fetch SKAdNetwork IDs from Unity's servers during `InvokePackagesCallback` (1.7s in the import profiler). This appears to block the MCP WebSocket server from reconnecting after domain reload.

**Symptom:** `mcporter call unityMCP.read_console` returns `"Unity session not ready for 'read_console' (ping not answered)"` indefinitely after the package is installed, even though the Editor.log shows a clean compile and domain reload.

**Fix:** Remove `com.unity.ads.ios-support` from the manifest. For ATT functionality, use a direct P/Invoke bridge: place `ATTBridge.mm` in `Assets/Plugins/iOS/` and call `[DllImport("__Internal")]` functions from C# behind `#if UNITY_IOS`. This provides identical ATT behaviour with zero package dependencies and no background network activity in the editor.

**Rule:** Avoid `com.unity.ads.ios-support` in this project. If ATT API changes require it in the future, test the package import in isolation and verify MCP reconnects before committing.

---

### K011 — Bee compilation pipeline stuck on stale content hashes
**Date:** 2026-03-25

When Unity's Bee compiler gets stuck in a persistent compile-error retry loop, it stops regenerating the dag from fresh file content. Subsequent source file changes are not picked up — Bee uses cached content hashes from the old `.dag` binary and recompiles the old failing code indefinitely.

**Symptom:** Error messages show wrong line numbers (matching an old version of the file), or the same error persists after a verified fix is on disk. `ls -la` shows the source file is newer than the reported error's context, but the dll timestamp predates the fix.

**Important:** Old compile errors persist in `Editor.log` forever — the log is append-only. Always check if errors appear *after the last `Starting:` line* to determine if they are from the current build. Use:
```python
python3 -c "
log = open('C:/Users/Daniel/AppData/Local/Unity/Editor/Editor.log', errors='replace').read()
last = log.split('Starting: ')[-1]
errors = [l for l in last.split('\n') if 'error CS' in l]
print('\n'.join(errors) if errors else 'No errors in last compile run')
"
```

**Fix:** Delete the active dag binary AND json files, plus tundra caches (do NOT delete all 5.5GB of `Library/Bee/`):
```
rm Library/Bee/<hash>EDbg.dag
rm Library/Bee/<hash>EDbg.dag.payloads
rm Library/Bee/<hash>EDbg.dag_derived
rm Library/Bee/<hash>EDbg.dag_fsmtime
rm Library/Bee/<hash>EDbg.dag.outputdata
rm Library/Bee/<hash>EDbg.dag.json
rm Library/Bee/<hash>EDbg-inputdata.json
rm Library/Bee/tundra.digestcache
rm Library/Bee/tundra.scancache
```
Then `Assets/Refresh` from the Unity Editor. Bee rebuilds the dag from scratch on next compile.

**How to identify the active dag hash:** Check the `Starting:` lines in `Editor.log` — the hash appears in the path e.g. `Library/Bee/900b0aEDbg.dag.json` or `900b0aEDbg-inputdata.json`.

---

### K012 — Windows ripgrep path quoting: OS error 123 with bare paths
**Date:** 2026-03-30

On Windows, `rg "pattern" Assets/Scripts/` (with a forward-slash path) can return `OS error 123` ("The filename, directory name, or volume label syntax is incorrect") in certain shell environments (Git Bash / PowerShell with a Node-spawned subprocess).

**Symptom:** Verification steps using `rg` exit with code 1 and `OS error 123` even when the target files exist and the pattern is correct.

**Workaround:** Replace `rg` with `grep -rn` for path-based searches, or use backslash paths on Windows (`Assets\Scripts\`). The `grep` built-in in Git Bash handles forward-slash paths correctly. Alternatively, pipe file content: `grep -c "pattern" path/to/file.cs`.

**Rule:** In verification commands on Windows, prefer `grep -c "pattern" path/to/file` over `rg "pattern" path/` for single-file checks. For directory-wide searches, `rg` is fine with explicit file paths but fragile with trailing slash directory arguments.
