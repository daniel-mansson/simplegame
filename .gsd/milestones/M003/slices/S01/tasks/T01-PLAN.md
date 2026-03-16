---
estimated_steps: 5
estimated_files: 1
---

# T01: Rewrite ConfirmDialogPresenter with WaitForConfirmation()

**Slice:** S01 — Presenter Async Result API
**Milestone:** M003

## Description

Remove the `Func<UniTask> dismissCallback` constructor parameter. Replace with a `UniTaskCompletionSource<bool>` field. Expose `WaitForConfirmation()` which creates a fresh TCS and returns its task. `HandleConfirm` completes with true; `HandleCancel` with false. Establishes the core UniTaskCompletionSource result-task pattern that all other presenters in this slice follow.

## Steps

1. Read current `ConfirmDialogPresenter.cs` in full
2. Remove `_dismissCallback` field and ctor param; update ctor signature to `(IConfirmDialogView view)` only
3. Add `private UniTaskCompletionSource<bool> _confirmTcs` field
4. Add `public UniTask<bool> WaitForConfirmation()`: if `_confirmTcs` is not null and not completed, cancel it first (TrySetCanceled); create new `UniTaskCompletionSource<bool>`, assign to `_confirmTcs`, return `_confirmTcs.Task`
5. Replace `HandleDismiss` with `HandleConfirm` (TrySetResult(true)) and `HandleCancel` (TrySetResult(false)); update `Initialize`/`Dispose` subscriptions accordingly; remove `.Forget()` usage

## Must-Haves

- [ ] Constructor signature is `ConfirmDialogPresenter(IConfirmDialogView view)` — no callbacks
- [ ] `WaitForConfirmation()` returns `UniTask<bool>`
- [ ] Confirm event → task resolves true; Cancel event → task resolves false
- [ ] `Dispose()` cancels any pending TCS (TrySetCanceled) so awaiting callers don't hang
- [ ] No `.Forget()` in file

## Verification

- `mcp_call unityMCP validate_script` on the file — zero errors
- Read the file back and confirm no `Func<UniTask>` or `Action` ctor params remain

## Inputs

- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — current callback-based implementation to replace

## Expected Output

- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — rewritten; TCS-backed; no callbacks
