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

When adding members to `IInputBlocker` (in `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs`), all mock implementations in test files must be updated. Known mocks:
- `MockInputBlocker` in `Assets/Tests/EditMode/Core/PopupManagerTests.cs`
- `MockInputBlockerGame` in `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `MockInputBlockerForInGame` in `Assets/Tests/EditMode/Game/InGameTests.cs`

---

### K005 — Unity scene file m_EditorClassIdentifier must be updated after class rename
**Date:** 2026-03-17

When renaming a MonoBehaviour class (e.g. `UnityPopupContainer` → `UnityViewContainer`), `git mv` preserves the `.meta` GUID so the scene's `m_Script` binding continues to resolve correctly at runtime. However, the `.unity` scene file also stores `m_EditorClassIdentifier` as a plain string containing the old class name. This does not break runtime binding, but shows the stale name in the Unity Inspector and will cause a `rg "OldName" Assets/` check to return a match.

**Fix:** After `git mv`, run `sed -i 's/OldClassName/NewClassName/g' Assets/Scenes/Boot.unity` (or the relevant scene file). Always run `rg "OldName" Assets/` across all file types (not just `.cs`) to catch scene file references.

