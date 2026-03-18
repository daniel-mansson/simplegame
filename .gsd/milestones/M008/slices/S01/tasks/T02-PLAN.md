# T02: UnityInputBlocker Fade + PopupManager Orchestration

**Slice:** S01
**Milestone:** M008

## Goal

Implement `FadeInAsync`/`FadeOutAsync` on `UnityInputBlocker` with LitMotion, and update `PopupManager` to orchestrate the timing split: block+fadeIn concurrent with show, unblock at dismiss start (not end).

## Must-Haves

### Truths
- `UnityInputBlocker.FadeInAsync`: CanvasGroup alpha animates 0→0.5 over 0.2s, LitMotion
- `UnityInputBlocker.FadeOutAsync`: CanvasGroup alpha animates 0.5→0 over 0.2s, LitMotion
- `PopupManager.ShowPopupAsync`: calls `Block()`, fires `FadeInAsync` + `ShowPopupAsync` concurrently with `UniTask.WhenAll`, pushes popup after both complete
- `PopupManager.DismissPopupAsync`: pops stack, calls `Unblock()` immediately, fires `FadeOutAsync` (not awaited — fire-and-forget via `.Forget()`), then awaits `HidePopupAsync`
- `PopupManager.DismissAllAsync`: same pattern — unblock per popup immediately, FadeOutAsync fire-and-forget, await HidePopupAsync
- `UnityInputBlocker` CanvasGroup starts at alpha=0, blocksRaycasts managed by `Block()`/`Unblock()` as before

### Artifacts
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — implements FadeInAsync/FadeOutAsync with LitMotion
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — updated orchestration

### Key Links
- `PopupManager` calls `IInputBlocker.FadeInAsync` — interface contract from T01
- `PopupManager.DismissPopupAsync` calls `Unblock()` before awaiting `HidePopupAsync` — this is the timing split (R080)

## Steps

1. Read current `UnityInputBlocker.cs` — add `FadeInAsync` and `FadeOutAsync` using `LMotion.Create(0f, 0.5f, 0.2f).Bind(x => _canvasGroup.alpha = x).ToUniTask(ct)` pattern
2. Ensure `_canvasGroup.alpha` starts at 0 in Awake/Start (or verify SceneSetup sets it to 0)
3. Read current `PopupManager.cs` — rewrite `ShowPopupAsync` to: `Block()` then `await UniTask.WhenAll(_inputBlocker.FadeInAsync(ct), _container.ShowPopupAsync(popupId, ct))` then push
4. Rewrite `DismissPopupAsync`: pop stack, `Unblock()`, `_inputBlocker.FadeOutAsync(ct).Forget()`, `await _container.HidePopupAsync(popupId, ct)`
5. Rewrite `DismissAllAsync`: same pattern per popup in the while loop
6. Verify: PopupManagerTests still pass (mocks return UniTask.CompletedTask for fade methods — timing logic is unchanged from mock perspective)

## Context
- K004: IInputBlocker mocks already updated in T01 — they return UniTask.CompletedTask so PopupManager tests work without real LitMotion
- The `_isOperating` guard must remain — prevents re-entrant show/dismiss
- FadeOutAsync is fire-and-forget on dismiss: input is immediately usable, overlay fades visually in background
- For DismissAllAsync with multiple popups: only fire FadeOutAsync once (first pop), or fire per pop? — fire once on first pop (when stack goes from N to N-1); the overlay has already faded, subsequent pops just hide immediately. Simpler: fire FadeOutAsync only when stack becomes empty (matches current Unblock logic).
- `UnityInputBlocker` is in `SimpleGame.Core` asmdef which already has LitMotion.Extensions — use it directly
