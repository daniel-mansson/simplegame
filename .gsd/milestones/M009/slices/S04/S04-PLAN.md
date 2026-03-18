---
id: S04
milestone: M009
title: Overlay HUD
risk: low
depends: [S03]
---

# S04: Overlay HUD

**Goal:** `UnityCurrencyOverlay` MonoBehaviour with LitMotion show/hide on its own canvas (sort order 120); shows coin balance; appears when LevelFailed is shown; hides after dismiss.

**Demo:** Fail a level → overlay HUD appears with coin balance. Dismiss popup → overlay disappears. Buy coins in shop → balance updates on overlay.

## Must-Haves

- `ICurrencyOverlay` interface: `ShowAsync(ct)`, `HideAsync(ct)`, `UpdateBalance(string)`
- `UnityCurrencyOverlay` MonoBehaviour: Canvas with sort order 120; CanvasGroup alpha LitMotion fade (0→1 show, 1→0 hide); TMP_Text coin display
- `GameBootstrapper` constructs and wires overlay; passes to InGameSceneController
- `InGameSceneController` shows overlay when entering LevelFailed popup, updates balance after any coin change, hides after popup dismissed
- SceneSetup creates overlay canvas in Boot scene
- All 192 existing tests remain green

## Tasks

- [ ] **T01: ICurrencyOverlay and UnityCurrencyOverlay**
  Create interface and MonoBehaviour. SceneSetup creates overlay canvas. GameBootstrapper wires it.

- [ ] **T02: InGameSceneController overlay integration**
  Show overlay before LevelFailed popup, update balance after shop purchase, hide after dismiss.

## Files Likely Touched

- `Assets/Scripts/Core/IOverlay/ICurrencyOverlay.cs` (new, in Core)
- `Assets/Scripts/Core/Unity/UnityCurrencyOverlay.cs` (new)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Editor/SceneSetup.cs`
