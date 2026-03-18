---
milestone: M008
provides:
  - IPopupView.AnimateInAsync/AnimateOutAsync — animation contract on all popup views
  - IInputBlocker.FadeInAsync/FadeOutAsync — overlay fade contract
  - PopupViewBase — abstract MonoBehaviour with LitMotion default animations (bounce-up in, scale+fade out)
  - PopupWindowShell — concrete PopupViewBase for standalone prefab use
  - UnityInputBlocker — LitMotion alpha fade (0→0.5 in, 0.5→0 out), input timing split, dim overlay owns visuals
  - PopupManager — rewritten orchestration: block+fadeIn+animateIn concurrent on show; unblock+fadeOut(forgotten)+animateOut on dismiss
  - PopupAnimationConfig ScriptableObject — all animation params (durations, offsets, ease curves, blocker alpha) editable in Inspector
  - All 6 popup views inherit PopupViewBase with TMP_Text fields and wired _canvasGroup/_panel/_animConfig
  - 8 UI component prefab assets in Assets/Prefabs/UI/: Windows/BigPopupWindow, Windows/SmallPopupWindow, Buttons/(Positive/Destructive/Neutral), Text/(Title/Body/ButtonLabel)
  - 6 game popup prefab assets in Assets/Prefabs/Game/Popups/: nested prefabs with live component prefab connections
  - SceneSetup.CreateBootScene — instantiates popup prefabs (no inline construction, no unpacking)
  - Boot scene — popup instances are live prefab connections; scene file compact
  - TMP added to Game and Editor asmdefs
  - DismissAllAsync unblock fix (Unblock per popup, matching Block per show)
  - PrefabKitSetup.cs — two-step reproducible generation: "Create UI Prefab Kit" then "Create Popup Prefabs"
slices_completed: [S01, S02, S03, S04]
key_files:
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Core/MVP/PopupViewBase.cs
  - Assets/Scripts/Core/MVP/PopupWindowShell.cs
  - Assets/Scripts/Core/MVP/PopupAnimationConfig.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Editor/PrefabKitSetup.cs
  - Assets/Prefabs/UI/Windows/ (2 window shell prefabs)
  - Assets/Prefabs/UI/Buttons/ (3 button prefabs)
  - Assets/Prefabs/UI/Text/ (3 text prefabs)
  - Assets/Prefabs/Game/Popups/ (6 nested popup prefabs)
  - Assets/Data/PopupAnimationConfig.asset
  - Assets/Scenes/Boot.unity
key_decisions:
  - "FadeOutAsync fire-and-forget: input unblocked before visual animation ends (R080)"
  - "PopupViewBase uses .Bind(x => cg.alpha = x) not BindToAlpha extension"
  - "Window prefabs are structural templates — view component added by SceneSetup on root"
  - "PopupWindowShell needed for standalone prefab use (abstract class can't be added directly)"
  - "DismissAllAsync Unblock per popup — matches Block per show in PopupManager"
  - "TMP bundled in com.unity.ugui 2.0.0 — no manifest change needed"
  - "InputBlocker alpha starts at 0 — correct initial state for LitMotion fade"
  - "Dim overlay owned by InputBlocker, not popup prefabs — one overlay shared across all popups"
  - "Popup prefabs are nested prefab assets (Layer 2) — buttons/text retain live prefab connections"
  - "UI components in Prefabs/UI/, game-specific popups in Prefabs/Game/ — separation of concerns"
  - "PopupAnimationConfig SO exposed for Inspector tuning — no hardcoded animation constants in code"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# M008: Popup Animation & UI Component Kit

**Full animated popup system delivered — LitMotion bounce-up/scale-out tweens, dim overlay fade with input timing split, TMP prefab kit, 6 nested popup prefabs with live connections, all wired in Boot scene; 169 tests green.**

## What Was Built

**S01 (Animation Plumbing):** Extended `IPopupView` with `AnimateInAsync`/`AnimateOutAsync` and `IInputBlocker` with `FadeInAsync`/`FadeOutAsync`. Created `PopupViewBase` — abstract MonoBehaviour with LitMotion default tweens. Rewrote `UnityInputBlocker` to animate alpha 0→0.5 on fade-in. Rewrote `PopupManager` with the R080 timing split: `Block()` + `FadeInAsync` + `ShowPopupAsync` concurrent on open; `Unblock()` + `FadeOutAsync.Forget()` before awaiting `HidePopupAsync` on close.

**S02 (TMP Prefab Kit):** Added `Unity.TextMeshPro` to Game and Editor asmdefs. Created `PrefabKitSetup.cs` with 8 component prefab assets (Windows, Buttons, Text). Fixed `DismissAllAsync` bug (Unblock per popup).

**S03 (Wire All Popups):** Updated all 6 view files to `TMP_Text`. Rebuilt `SceneSetup.CreateBootScene` to use prefab-based popup construction. Boot scene regenerated.

**S04 (Integration Verification):** 169/169 EditMode tests passing. UAT script written.

**Post-S04 improvements:**

- **`PopupAnimationConfig` ScriptableObject** — all animation parameters (durations, ease curves, Y offset, scale, blocker alpha) exposed as Inspector-editable fields. Both `PopupViewBase` and `UnityInputBlocker` read from it; null now logs an error rather than silently using fallback constants.
- **Dim overlay moved to InputBlocker** — removed per-popup dim Image from window prefabs. Single overlay on the InputBlocker canvas, shared across all popups. Fixes doubled overlay on concurrent popups.
- **Nested popup prefab assets** — 6 popup prefabs in `Assets/Prefabs/Game/Popups/` with live connections to window, button, and text component prefabs. Boot scene stores GUIDs only (scene file 5400 lines shorter). SceneSetup now simply instantiates the popup prefabs — no inline construction.
- **Folder reorganization** — generic UI components in `Assets/Prefabs/UI/` (Windows/, Buttons/, Text/); game-specific popups in `Assets/Prefabs/Game/Popups/`. Empty `UI/Popups/` folder cleaned up.

## Deviations from Plan

- `PopupWindowShell` added in S02 — needed because abstract classes can't be added as components
- `DismissAllAsync` regression fixed in S02 — orchestration bug introduced in S01
- Dim overlay fix discovered during testing — per-popup Image caused doubled overlay; moved to InputBlocker
- Popup prefab hierarchy added post-S04 — scene had no prefab connections initially; rebuilt as proper nested prefabs

## Requirements Addressed

- R079 ✓ — visible dim overlay (on InputBlocker) fades in/out with popup; single shared overlay
- R080 ✓ — input unblocked at fade-out start, not animation end
- R081 ✓ — IPopupView.AnimateInAsync/OutAsync with PopupViewBase default; PopupAnimationConfig tunable
- R082 ✓ — TMP-based UI prefab kit (8 component assets + 6 popup assets)
- R083 ✓ — all 6 popups wired to prefab components; Boot scene uses live prefab instances

## Regeneration Sequence

```
Tools/Setup/Create UI Prefab Kit    → Assets/Prefabs/UI/ (8 component prefabs)
Tools/Setup/Create Popup Prefabs    → Assets/Prefabs/Game/Popups/ (6 popup prefabs)
Tools/Setup/Create And Register Scenes → Assets/Scenes/Boot.unity (instantiates popup prefabs)
```
