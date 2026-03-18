---
id: S02
milestone: M009
title: Popup Stack Visual Layering
risk: medium
depends: []
---

# S02: Popup Stack Visual Layering

**Goal:** `UnityViewContainer` assigns Canvas sort orders per stack depth so bottom popup is dimmed by blocker overlay and top popup is above it. `PopupId.Shop` and `IShopView`/`ShopView` created.

**Demo:** Two popups shown in sequence — bottom popup renders below blocker (visually dimmed), top popup renders above blocker (visible and interactive). EditMode test confirms sort order assignment.

## Must-Haves

- `UnityViewContainer.ShowPopupAsync` adds a `Canvas` component with `overrideSorting=true` to each popup root when shown; sort order = 50 + (stackDepth * 100) where stackDepth = number of popups already on stack at time of show (0-indexed: first popup = 50, second = 150)
- `UnityViewContainer` tracks an internal depth counter; increments on Show, decrements on Hide
- Sort order reset (remove Canvas override or set to 0) when popup hidden — or simply leave Canvas at 0 on hide since the GO is inactive
- `PopupId.Shop` added to enum
- `IShopView` interface — 3 pack buttons + cancel, inherits `IPopupView`; `ShopView` MonoBehaviour inherits `PopupViewBase`
- `ShopPopup.prefab` created in `Assets/Prefabs/Game/Popups/` (via PrefabKitSetup or manually)
- `UnityViewContainer` has `[SerializeField] private GameObject _shopPopup` field wired in Boot scene
- EditMode test: `UnityViewContainer` assigns sort orders correctly for stacked shows
- All 180 existing tests remain green

## Tasks

- [ ] **T01: Sort-order scheme in UnityViewContainer**
  Add Canvas override sort order logic to `ShowPopupAsync`/`HidePopupAsync`. Add `PopupId.Shop`. Add `_shopPopup` field. Add EditMode test. Update `SceneSetup` to wire Shop popup.

- [ ] **T02: IShopView, ShopView, ShopPopup prefab**
  Create `IShopView`, `ShopView` MonoBehaviour. Add to PrefabKitSetup. Create prefab. Wire into Boot scene.

## Files Likely Touched

- `Assets/Scripts/Game/PopupId.cs`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs`
- `Assets/Scripts/Game/Popup/IShopView.cs` (new)
- `Assets/Scripts/Game/Popup/ShopView.cs` (new)
- `Assets/Editor/SceneSetup.cs`
- `Assets/Editor/PrefabKitSetup.cs`
- `Assets/Tests/EditMode/Game/ViewContainerTests.cs`
