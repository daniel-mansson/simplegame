# T02: View, Container & Boot Wiring

**Slice:** S01
**Milestone:** M018

## Goal
Wire the consent gate end-to-end: ConsentGateView MonoBehaviour, PopupId registration, UnityViewContainer slot, UIFactory method, SceneSetup prefab, and GameBootstrapper gate before PlayFab login.

## Must-Haves

### Truths
- `PopupId.ConsentGate` exists in the enum
- `UnityViewContainer` shows/hides `ConsentGate` popup via `GetPopupObject`
- `GameBootstrapper.Start()` checks `ConsentGatePresenter.ShouldShow()` before PlayFab login; if true, awaits `WaitForAccept()` before proceeding
- `UIFactory.CreateConsentGatePresenter(view)` returns a `ConsentGatePresenter`
- `SceneSetup.CreateBootScene()` instantiates `ConsentGatePopup` prefab and wires it to `_consentGatePopup` on `UnityViewContainer`
- Project compiles with no errors
- Running `Tools/Setup/Create And Register Scenes` produces a Boot scene with the consent popup wired

### Artifacts
- `Assets/Scripts/Game/Popup/ConsentGateView.cs` — MonoBehaviour extending `PopupViewBase`, implementing `IConsentGateView`; wires `_acceptButton` onClick; `SetAcceptInteractable`; TMP links for ToS and Privacy Policy URLs
- `Assets/Scripts/Game/PopupId.cs` — `ConsentGate` added
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — `[SerializeField] _consentGatePopup`; `case PopupId.ConsentGate` in switch; `PlatformLink` case removed (PlatformLink uses FindFirstObjectInBootScene pattern, not container)
- `Assets/Scripts/Game/Boot/UIFactory.cs` — `CreateConsentGatePresenter(IConsentGateView)`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — consent gate block before PlayFab login
- `Assets/Editor/SceneSetup.cs` — instantiate `ConsentGatePopup` prefab, wire to `_consentGatePopup`

### Key Links
- `GameBootstrapper` → `ConsentGatePresenter.ShouldShow()` + `WaitForAccept()`
- `UnityViewContainer._consentGatePopup` → instantiated prefab in Boot scene
- `ConsentGateView` → `IConsentGateView` (resolved by `UnityViewContainer.Get<IConsentGateView>()`)

## Steps
1. Add `ConsentGate` to `PopupId` enum
2. Write `ConsentGateView` MonoBehaviour — `PopupViewBase` subclass; `[SerializeField] Button _acceptButton`; `[SerializeField] TMP_Text _tosLinkText`, `_privacyLinkText`; wire onClick in Awake; `Application.OpenURL` for links; `SetAcceptInteractable`
3. Add `[SerializeField] private GameObject _consentGatePopup` to `UnityViewContainer`; add `case PopupId.ConsentGate` to `GetPopupObject`
4. Add `CreateConsentGatePresenter` to `UIFactory`
5. Add consent gate block to `GameBootstrapper.Start()` — before PlayFab login, check `ShouldShow`, if true find view via `FindFirstObjectInBootScene<IConsentGateView>()`, show popup, await result, dismiss
6. Update `SceneSetup.CreateBootScene()` to instantiate `ConsentGatePopup` prefab and wire `_consentGatePopup` on `UnityViewContainer`
7. Run `Tools/Setup/Create And Register Scenes` to regenerate Boot scene, verify no errors
8. Verify project compiles clean via Unity console

## Context
- `PlatformLink` popup is NOT in UnityViewContainer — it uses `FindFirstObjectInBootScene` directly in Bootstrapper. ConsentGate follows the same direct-find pattern for the view but IS in UnityViewContainer for the show/hide lifecycle (same as all other popups)
- K007: after changing `[SerializeField]` on `UnityViewContainer`, must run SceneSetup and commit scene file
- The prefab for ConsentGatePopup will be created manually by the user in the Unity Editor (same workflow as existing popup prefabs) — SceneSetup just wires it if present, logs warning if missing
- `ConsentGateView` needs TMP for the link texts (same as other views) — use `TMPro.TMP_Text` fields
