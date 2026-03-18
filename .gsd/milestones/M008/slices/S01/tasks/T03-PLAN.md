# T03: UnityViewContainer Animation Wiring + View Migration

**Slice:** S01
**Milestone:** M008

## Goal

Update `UnityViewContainer` to call `AnimateInAsync`/`AnimateOutAsync` on popup views, and migrate all 6 existing `*View.cs` files to inherit `PopupViewBase` instead of `MonoBehaviour`.

## Must-Haves

### Truths
- `UnityViewContainer.ShowPopupAsync`: calls `SetActive(true)` then `await view.AnimateInAsync(ct)`
- `UnityViewContainer.HidePopupAsync`: calls `await view.AnimateOutAsync(ct)` then `SetActive(false)`
- All 6 `*View.cs` files inherit `PopupViewBase` instead of `MonoBehaviour`
- `PopupViewBase` fields `_canvasGroup` and `_panel` are `[SerializeField]` — SceneSetup must wire them (that's S03; for now the views compile and default anim uses Debug.LogWarning if refs null)
- Project compiles with no errors
- EditMode tests: 169 green

### Artifacts
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — updated ShowPopupAsync/HidePopupAsync
- `Assets/Scripts/Game/Popup/ConfirmDialogView.cs` — inherits PopupViewBase
- `Assets/Scripts/Game/Popup/LevelCompleteView.cs` — inherits PopupViewBase
- `Assets/Scripts/Game/Popup/LevelFailedView.cs` — inherits PopupViewBase
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — inherits PopupViewBase
- `Assets/Scripts/Game/Popup/IAPPurchaseView.cs` — inherits PopupViewBase
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs` — inherits PopupViewBase

### Key Links
- `UnityViewContainer.GetPopupObject()` returns `GameObject` — view accessed via `GetComponent<IPopupView>()` or `GetComponentInChildren<IPopupView>(true)` to call animate methods
- `PopupViewBase` is in Core; `SimpleGame.Game.asmdef` references Core — no circular dependency

## Steps

1. Read `UnityViewContainer.cs` — update `ShowPopupAsync`: get `IPopupView` from popup GO via `GetComponentInChildren<IPopupView>(true)`, call `SetActive(true)`, await `AnimateInAsync`
2. Update `HidePopupAsync`: get `IPopupView`, await `AnimateOutAsync`, then `SetActive(false)`
3. Add null check: if `IPopupView` component not found, log warning and proceed without animation
4. Update `ConfirmDialogView.cs`: change `: MonoBehaviour, IConfirmDialogView` to `: PopupViewBase, IConfirmDialogView`; add using for Core.MVP namespace
5. Repeat for all 5 remaining view files
6. Run LSP diagnostics to catch any compile errors

## Context
- `SimpleGame.Game.asmdef` references `SimpleGame.Core` — `PopupViewBase` is accessible
- `PopupViewBase` serialized fields (`_canvasGroup`, `_panel`) won't be wired until S03 SceneSetup rewrite; default animations will fail gracefully (null check + Debug.LogWarning) for now
- The `Awake()` method in each view subscribes button listeners — `PopupViewBase` must NOT define its own `Awake()` or views must call `base.Awake()`. Safest: `PopupViewBase` uses no Awake, relies on `Start()` or lazy init.
- Views must call `using SimpleGame.Core.MVP;` to resolve `PopupViewBase`
