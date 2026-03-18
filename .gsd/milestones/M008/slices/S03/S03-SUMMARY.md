---
id: S03
milestone: M008
provides:
  - All 6 popup views use TMP_Text for text fields (ConfirmDialogView, LevelCompleteView, LevelFailedView, RewardedAdView, IAPPurchaseView, ObjectRestoredView)
  - SceneSetup.CreateBootScene rebuilt using InstantiateWindowPrefab + TMP text + prefab buttons
  - All 6 popups have _canvasGroup and _panel wired — PopupViewBase animations now active
  - InputBlocker CanvasGroup alpha starts at 0 (corrected from 1)
  - Boot scene regenerated with 22 TMP references
  - PrefabKitSetup updated to not add PopupWindowShell to window prefabs
  - Window prefabs are now clean structural templates (CanvasGroup root + Panel child, no view component)
requires:
  - slice: S02
    provides: TMP asmdefs, prefab assets
  - slice: S01
    provides: PopupViewBase, animation contracts
affects: [S04]
key_files:
  - Assets/Editor/SceneSetup.cs
  - Assets/Editor/PrefabKitSetup.cs
  - Assets/Scenes/Boot.unity
  - Assets/Scripts/Game/Popup/ConfirmDialogView.cs
  - Assets/Scripts/Game/Popup/LevelCompleteView.cs
  - Assets/Scripts/Game/Popup/LevelFailedView.cs
  - Assets/Scripts/Game/Popup/RewardedAdView.cs
  - Assets/Scripts/Game/Popup/IAPPurchaseView.cs
  - Assets/Scripts/Game/Popup/ObjectRestoredView.cs
  - Assets/Prefabs/UI/BigPopupWindow.prefab
  - Assets/Prefabs/UI/SmallPopupWindow.prefab
key_decisions:
  - "Window prefabs are structural templates — view MonoBehaviour added by SceneSetup on root"
  - "PopupWindowShell removed from window prefabs — would conflict with concrete view as IPopupView"
  - "InputBlocker CanvasGroup alpha initialized to 0 (was 1) — LitMotion fade starts from correct state"
  - "PrefabUtility.UnpackPrefabInstance used to break prefab connection after instantiation in scene"
patterns_established:
  - "InstantiateWindowPrefab + unpack + add view component on root — pattern for all popup creation in SceneSetup"
  - "CreateTMPText helper for consistent TMP text creation in SceneSetup"
  - "InstantiateButton helper for prefab button creation with anchor positioning"
drill_down_paths:
  - .gsd/milestones/M008/slices/S03/S03-PLAN.md
duration: 40min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S03: Wire All Popups to Prefabs

**All 6 popup views updated to TMP_Text; SceneSetup rebuilt with prefab-based construction; Boot scene regenerated; _canvasGroup/_panel wired; 169 tests green.**

## What Happened

**T01:** Removed `PopupWindowShell` from `PrefabKitSetup.CreateWindowPrefab` — window prefabs are now clean structural templates (CanvasGroup + Image on root, Panel child). Regenerated both window prefabs.

**T02:** Updated all 6 view files to use `TMP_Text` instead of `UnityEngine.UI.Text`. Added `using TMPro;` to each.

**T03:** Rewrote `SceneSetup.CreateBootScene`. The old `CreatePopupDialog` helper is replaced by `InstantiateWindowPrefab` (which instantiates the prefab and unpacks it), `InstantiateButton` (which instantiates a button prefab with label and anchors), and `CreateTMPText` (creates a TextMeshProUGUI child). Each popup: instantiate window prefab → add view component on root → wire `_canvasGroup` (root) + `_panel` (Panel child) → wire text and button fields.

Also fixed: `InputBlocker` CanvasGroup `alpha` was `1f` in old SceneSetup — now correctly `0f` (LitMotion FadeIn starts from 0).

SceneSetup ran successfully. Boot.unity has 22 TMP component references and all 6 popup names confirmed.

## Files Created/Modified

- `Assets/Editor/SceneSetup.cs` — CreateBootScene rewrite, new helpers
- `Assets/Editor/PrefabKitSetup.cs` — PopupWindowShell removed from window creation
- All 6 `*View.cs` — Text → TMP_Text fields
- `Assets/Prefabs/UI/BigPopupWindow.prefab` — regenerated (clean, no PopupWindowShell)
- `Assets/Prefabs/UI/SmallPopupWindow.prefab` — regenerated (clean)
- `Assets/Scenes/Boot.unity` — regenerated with TMP wiring
