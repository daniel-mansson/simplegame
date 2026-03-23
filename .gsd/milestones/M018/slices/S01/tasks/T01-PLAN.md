# T01: Interface, Presenter & Tests

**Slice:** S01
**Milestone:** M018

## Goal
Define `IConsentGateView`, implement `ConsentGatePresenter` with PlayerPrefs gate and UniTask completion, and write edit-mode tests proving the flag logic and accept flow.

## Must-Haves

### Truths
- `ConsentGatePresenter.ShouldShow()` returns true when PlayerPrefs key absent
- `ConsentGatePresenter.ShouldShow()` returns false after `MarkAccepted()`
- `MarkAccepted()` writes PlayerPrefs key `"ConsentGate_Accepted"` = 1
- `WaitForAccept()` resolves when `OnAcceptClicked` fires
- `IConsentGateView` has NO close/dismiss/skip events — only `OnAcceptClicked`
- All existing edit-mode tests still pass

### Artifacts
- `Assets/Scripts/Game/Popup/IConsentGateView.cs` — interface extending `IPopupView`, `event Action OnAcceptClicked` only
- `Assets/Scripts/Game/Popup/ConsentGatePresenter.cs` — presenter with `ShouldShow()`, `MarkAccepted()`, `WaitForAccept()`, `Initialize()`, `Dispose()`
- `Assets/Tests/EditMode/Game/ConsentGateTests.cs` — edit-mode tests for flag logic and accept flow

### Key Links
- `ConsentGatePresenter` extends `Presenter<IConsentGateView>` (same base as `PlatformLinkPresenter`)
- `IConsentGateView` extends `IPopupView`

## Steps
1. Write `IConsentGateView` — single event `OnAcceptClicked`, no other events
2. Write `ConsentGatePresenter` — `HasAcceptedKey` const, `ShouldShow()`, `MarkAccepted()`, `WaitForAccept()` UniTask, `Initialize()` subscribes, `Dispose()` unsubscribes
3. Write `ConsentGateTests` — TearDown clears PlayerPrefs key; tests for ShouldShow true/false, MarkAccepted persists, WaitForAccept resolves on click

## Context
- Follow `PlatformLinkPresenter` exactly for structure — same PlayerPrefs pattern, same UniTaskCompletionSource pattern
- `IConsentGateView` must NOT have any dismiss path — this is a hard constraint (D094)
- PlayerPrefs key: `"ConsentGate_Accepted"`
