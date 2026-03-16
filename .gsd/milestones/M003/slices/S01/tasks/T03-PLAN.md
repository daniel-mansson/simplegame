---
estimated_steps: 4
estimated_files: 1
---

# T03: Rewrite SettingsPresenter with WaitForBack()

**Slice:** S01 — Presenter Async Result API
**Milestone:** M003

## Description

Remove `Func<UniTask> goBackCallback` from `SettingsPresenter`. Add a `UniTaskCompletionSource` (non-generic, void result) and expose `WaitForBack()` which resolves when back is pressed. Simplest case — no result value.

## Steps

1. Read current `SettingsPresenter.cs` in full
2. Remove `_goBackCallback` field and ctor param; update ctor to `(ISettingsView view)` only
3. Add `private UniTaskCompletionSource _backTcs` field (non-generic UniTask void TCS)
4. Add `public UniTask WaitForBack()`: cancel existing TCS if present, create new, return task; `HandleBackClicked` calls `_backTcs.TrySetResult()`; `Dispose()` calls `_backTcs?.TrySetCanceled()`

## Must-Haves

- [ ] Constructor is `SettingsPresenter(ISettingsView view)` — no callbacks
- [ ] `WaitForBack()` returns `UniTask`
- [ ] Back click → task resolves
- [ ] `Dispose()` cancels pending TCS
- [ ] No `.Forget()` in file

## Verification

- `validate_script` on file — zero errors
- No `Func<UniTask>` in file

## Inputs

- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — current callback-based implementation

## Expected Output

- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — rewritten; no callbacks; `WaitForBack()` exposed
