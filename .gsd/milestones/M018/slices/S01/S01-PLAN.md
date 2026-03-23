# S01: Consent Gate Popup

**Goal:** First-launch ToS/Privacy popup that blocks the boot sequence until the player taps Accept. No close button, no dismiss path. PlayerPrefs flag written on accept; never shown again once accepted.

**Demo:** Launch game → consent popup appears (no X button) → tap Accept → main menu loads. Relaunch → straight to main menu.

## Must-Haves
- `IConsentGateView` interface with only `OnAcceptClicked` event — no dismiss/close events
- `ConsentGatePresenter` with `ShouldShow()` / `MarkAccepted()` / `WaitForAccept()` following PlatformLinkPresenter pattern
- `PopupId.ConsentGate` added to enum
- `ConsentGateView` MonoBehaviour wired in Boot scene via SceneSetup
- `UnityViewContainer` registers the new popup
- `GameBootstrapper` gates on consent before PlayFab login
- `UIFactory.CreateConsentGatePresenter` factory method
- Edit-mode tests: ShouldShow flag logic, MarkAccepted persists, Accept resolves WaitForAccept
- All existing tests still pass

## Tasks

- [ ] **T01: Interface, Presenter & Tests**
  `IConsentGateView`, `ConsentGatePresenter` (ShouldShow/MarkAccepted/WaitForAccept), edit-mode tests.

- [ ] **T02: View, Container & Boot Wiring**
  `ConsentGateView` MonoBehaviour, `PopupId.ConsentGate`, `UnityViewContainer` registration, `UIFactory` factory method, `SceneSetup` prefab instantiation, `GameBootstrapper` gate before PlayFab login.

## Files Likely Touched
- `Assets/Scripts/Game/Popup/IConsentGateView.cs` (new)
- `Assets/Scripts/Game/Popup/ConsentGatePresenter.cs` (new)
- `Assets/Scripts/Game/Popup/ConsentGateView.cs` (new)
- `Assets/Scripts/Game/PopupId.cs`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Tests/EditMode/Game/ConsentGateTests.cs` (new)
