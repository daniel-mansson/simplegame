---
id: T02
parent: S01
milestone: M008
provides:
  - UnityInputBlocker.FadeInAsync/FadeOutAsync — LitMotion alpha 0→0.5 / 0.5→0
  - UnityInputBlocker.Awake() resets alpha to 0 and blocksRaycasts to false
  - PopupManager.ShowPopupAsync — Block() then WhenAll(FadeInAsync, ShowPopupAsync)
  - PopupManager.DismissPopupAsync — Unblock() immediately, FadeOutAsync.Forget(), await HidePopupAsync
  - PopupManager.DismissAllAsync — same per-last-popup timing split
requires:
  - slice: S01/T01
    provides: IInputBlocker with FadeInAsync/FadeOutAsync contract
affects: [T03, S02]
key_files:
  - Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
key_decisions:
  - "FadeOutAsync is fire-and-forget (.Forget()) in PopupManager — input unblocks before animation completes (R080)"
  - "FadeOut fires only when stack hits 0 (DismissAll) or on last popup (DismissPopup)"
  - "FadedAlpha constant = 0.5f — the dim overlay opacity target"
patterns_established:
  - "Unblock() before FadeOutAsync.Forget() before await HidePopupAsync — the timing split pattern"
drill_down_paths:
  - .gsd/milestones/M008/slices/S01/tasks/T02-PLAN.md
duration: 15min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T02: UnityInputBlocker Fade + PopupManager Orchestration

**LitMotion alpha fade on UnityInputBlocker; PopupManager rewritten with timing split — input unblocks before animation ends; 169 tests green.**

## What Happened

`UnityInputBlocker` now initialises `_canvasGroup.alpha = 0` and `blocksRaycasts = false` in `Awake()`. `FadeInAsync` animates 0→0.5, `FadeOutAsync` animates current alpha→0 (uses current alpha so it handles partially-faded states cleanly). Both use `LMotion.Create().WithEase(Ease.Linear).Bind().ToUniTask(ct)`.

`PopupManager.ShowPopupAsync` now calls `Block()` then `UniTask.WhenAll(FadeInAsync, container.ShowPopupAsync)` so the overlay and popup animate concurrently. `DismissPopupAsync` pops, then — only when the stack reaches zero — calls `Unblock()`, fires `FadeOutAsync().Forget()`, then awaits `HidePopupAsync`. This is the R080 timing split: input is available before the visual fade completes.

`DismissAllAsync` follows the same pattern inside the while loop, triggering the unblock+fade-forget only on the final iteration.

## Deviations

None from plan.

## Files Created/Modified

- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — FadeInAsync/FadeOutAsync, Awake init
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — orchestration rewrite
