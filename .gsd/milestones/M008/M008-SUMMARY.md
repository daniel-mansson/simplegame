---
milestone: M008
provides:
  - IPopupView.AnimateInAsync/AnimateOutAsync — animation contract on all popup views
  - IInputBlocker.FadeInAsync/FadeOutAsync — overlay fade contract
  - PopupViewBase — abstract MonoBehaviour with LitMotion default animations (bounce-up in, scale+fade out)
  - PopupWindowShell — concrete PopupViewBase for standalone use (not used on popup scene objects)
  - UnityInputBlocker — LitMotion alpha fade (0→0.5 in, 0.5→0 out), input timing split
  - PopupManager — rewritten orchestration: block+fadeIn+animateIn concurrent on show; unblock+fadeOut(forgotten)+animateOut on dismiss
  - All 6 popup views inherit PopupViewBase with TMP_Text fields and wired _canvasGroup/_panel
  - 8 UI prefab assets: BigPopupWindow, SmallPopupWindow, 3 buttons, 3 text styles (TMP)
  - SceneSetup.CreateBootScene rebuilt with prefab-based popup construction
  - Boot scene regenerated with TMP wiring and correct InputBlocker alpha=0
  - TMP added to Game and Editor asmdefs
  - DismissAllAsync unblock fix (Unblock per popup, matching Block per show)
  - PrefabKitSetup.cs — reproducible menu item for prefab kit regeneration
slices_completed: [S01, S02, S03, S04]
key_files:
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Core/MVP/PopupViewBase.cs
  - Assets/Scripts/Core/MVP/PopupWindowShell.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Editor/PrefabKitSetup.cs
  - Assets/Prefabs/UI/ (8 prefab assets)
  - Assets/Scenes/Boot.unity
key_decisions:
  - "FadeOutAsync fire-and-forget: input unblocked before visual animation ends (R080)"
  - "PopupViewBase uses .Bind(x => cg.alpha = x) not BindToAlpha extension"
  - "Window prefabs are structural templates — view component added by SceneSetup on root"
  - "PopupWindowShell needed for standalone prefab use (abstract class can't be added directly)"
  - "DismissAllAsync Unblock per popup — matches Block per show in PopupManager"
  - "TMP bundled in com.unity.ugui 2.0.0 — no manifest change needed"
  - "InputBlocker alpha starts at 0 (fixed from 1) — correct initial state for LitMotion fade"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# M008: Popup Animation & UI Component Kit

**Full animated popup system delivered — LitMotion bounce-up/scale-out tweens, dim overlay fade with input timing split, TMP prefab kit, all 6 popups wired; 169 tests green.**

## What Was Built

**S01 (Animation Plumbing):** Extended `IPopupView` with `AnimateInAsync`/`AnimateOutAsync` and `IInputBlocker` with `FadeInAsync`/`FadeOutAsync`. Created `PopupViewBase` — abstract MonoBehaviour with LitMotion default tweens: bounce-up entrance (-80px Y offset, OutBounce, 0.4s) and scale+fade exit (0.85 scale + alpha 0, InBack, 0.25s). Rewrote `UnityInputBlocker` to animate alpha 0→0.5 on fade-in. Rewrote `PopupManager` with the R080 timing split: `Block()` + `FadeInAsync` + `ShowPopupAsync` concurrent on open; `Unblock()` + `FadeOutAsync.Forget()` before awaiting `HidePopupAsync` on close.

**S02 (TMP Prefab Kit):** Added `Unity.TextMeshPro` to Game and Editor asmdefs (bundled in com.unity.ugui, no manifest change). Created `PrefabKitSetup.cs` with all 8 prefab assets: BigPopupWindow, SmallPopupWindow (CanvasGroup + Panel), PositiveButton, DestructiveButton, NeutralButton, TitleText, BodyText, ButtonLabel. Fixed `DismissAllAsync` bug: was calling `Unblock()` only once for the whole batch — should call per popup to balance individual `Block()` calls.

**S03 (Wire All Popups):** Updated all 6 view files to `TMP_Text`. Rewrote `SceneSetup.CreateBootScene` with `InstantiateWindowPrefab` + `InstantiateButton` + `CreateTMPText` helpers. Each popup is now built from a BigPopup or SmallPopup prefab shell (unpacked), with the concrete view MonoBehaviour added on the root and `_canvasGroup`/`_panel` refs wired. Boot scene regenerated.

**S04 (Integration Verification):** Final test run confirmed 169/169 EditMode tests passing. UAT script written for human play-mode verification.

## Deviations from Plan

- `PopupWindowShell` was added in S02 (not originally planned) — needed because abstract classes can't be added as components in the Unity editor; used for standalone prefab animation capability
- `DismissAllAsync` regression fixed in S02 — orchestration bug introduced in S01 when rewriting timing split
- More IPopupView mocks than expected (DemoWiringTests had MockConfirmDialogView not in K004) — K004 updated

## Requirements Addressed

- R079 ✓ — visible dim overlay fades in/out with popup
- R080 ✓ — input unblocked at fade-out start, not animation end
- R081 ✓ — IPopupView.AnimateInAsync/OutAsync with PopupViewBase default
- R082 ✓ — TMP-based UI prefab kit (8 assets)
- R083 ✓ — all 6 popups wired to prefab components in SceneSetup
