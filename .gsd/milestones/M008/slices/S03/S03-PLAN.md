# S03: Wire All Popups to Prefabs

**Goal:** Rewrite SceneSetup.CreateBootScene to build all 6 popups from prefab components. Update view fields from UnityEngine.UI.Text to TMP_Text. Wire PopupViewBase._canvasGroup and ._panel on each view.

**Demo:** SceneSetup runs clean, Boot scene regenerated with all 6 popups using prefab-based window shells and TMP text. AnimateIn/AnimateOut work (refs now wired).

## Must-Haves

- SceneSetup `CreateBootScene` builds all 6 popups by instantiating BigPopupWindow or SmallPopupWindow prefabs
- PopupWindowShell removed from window prefabs (it would conflict with the concrete view component added by SceneSetup)
- All 6 `*View.cs` files: button fields remain `Button`, text fields changed from `Text` to `TMP_Text`
- SceneSetup runs via MCP with no errors
- Boot scene saved with all wiring intact
- EditMode tests still green (169)

## Tasks

- [ ] **T01: Remove PopupWindowShell from window prefabs + update SceneSetup approach**
  Update PrefabKitSetup to not add PopupWindowShell to window prefabs. Regenerate prefabs. Update SceneSetup strategy: instantiate window prefab, add view component on root, wire _canvasGroup/_panel + TMP text fields.

- [ ] **T02: Update view fields from Text to TMP_Text**
  Change all 6 `*View.cs` text fields from `UnityEngine.UI.Text` to `TMPro.TMP_Text`. Update the view interface methods that take text to still work (they use `.text =` which TMP_Text supports).

- [ ] **T03: Rewrite SceneSetup.CreateBootScene**
  Rebuild CreateBootScene using window prefab instantiation + prefab button placement + TMP text. Wire all fields including _canvasGroup/_panel on view. Run SceneSetup via MCP, verify Boot scene saved.

## Files Likely Touched

- `Assets/Editor/PrefabKitSetup.cs` — remove PopupWindowShell from window creation
- `Assets/Prefabs/UI/BigPopupWindow.prefab` — regenerated without PopupWindowShell
- `Assets/Prefabs/UI/SmallPopupWindow.prefab` — regenerated without PopupWindowShell
- `Assets/Scripts/Game/Popup/ConfirmDialogView.cs` — Text → TMP_Text
- `Assets/Scripts/Game/Popup/LevelCompleteView.cs` — Text → TMP_Text
- `Assets/Scripts/Game/Popup/LevelFailedView.cs` — Text → TMP_Text
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — Text → TMP_Text
- `Assets/Scripts/Game/Popup/IAPPurchaseView.cs` — Text → TMP_Text
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs` — Text → TMP_Text
- `Assets/Editor/SceneSetup.cs` — CreateBootScene rewrite
- `Assets/Scenes/Boot.unity` — regenerated
