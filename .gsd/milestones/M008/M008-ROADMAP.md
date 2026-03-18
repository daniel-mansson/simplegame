# M008: Popup Animation & UI Component Kit

**Vision:** Popup system upgraded with animated dim overlay, LitMotion-driven bounce-in/scale-out tweens, input timing split on dismiss, and a TMP-based UI prefab kit wired into all existing popups.

## Success Criteria

- Opening any popup fades in a dim overlay and bounces the popup up from below
- Closing any popup starts fading the dim overlay and restoring input immediately, while the popup scales down and fades out
- All 6 popup types use BigPopup or SmallPopup window shell prefabs with TMP text and prefab button components
- SceneSetup regenerates the Boot scene cleanly with all wiring intact
- EditMode tests remain green

## Key Risks / Unknowns

- LitMotion.Extensions BindToAlpha for CanvasGroup — may need `.Bind(x => cg.alpha = x)` fallback
- TMP asmdef wiring — Game and Editor asmdefs need TMP reference; SceneSetup CreateText helpers need rewrite
- PopupViewBase base class change — all 6 existing View MonoBehaviours must change their inheritance; tests must compile
- Input timing split changes PopupManager's orchestration contract — blocker unblock must fire before HidePopupAsync awaits

## Proof Strategy

- TMP asmdef risk → retire in S02 by adding TMP to manifest + asmdefs and compiling clean
- PopupViewBase inheritance risk → retire in S01/T01 by changing one view first and confirming tests pass
- Input timing split → retire in S01 by unit test or console-log verification of unblock-before-animation-complete

## Verification Classes

- Contract verification: EditMode tests green; LitMotion motions fire without error; prefab assets load in editor
- Integration verification: PopupManager orchestration — blocker fades with correct input timing; popup animates in/out
- Operational verification: none
- UAT / human verification: Open/close popups in play mode and observe dim + bounce + scale-out behavior

## Milestone Definition of Done

This milestone is complete only when all are true:

- All 6 popup views inherit PopupViewBase and have default AnimateIn/AnimateOut
- UnityInputBlocker fades its alpha with LitMotion and unblocks at fade-out start
- PopupManager passes the animation + timing split contract
- All prefab assets (BigPopup, SmallPopup, 3 buttons, 3 text) exist in Assets/Prefabs/UI/
- SceneSetup regenerates Boot scene using prefab components; no compiler warnings about TMP
- EditMode tests green (existing 169 + any new tests)
- Boot scene committed with SceneSetup-generated wiring

## Requirement Coverage

- Covers: R079, R080, R081, R082, R083
- Partially covers: none
- Leaves for later: R078 (prefab instantiation on demand — still pre-instantiated)
- Orphan risks: none

## Slices

- [x] **S01: Animation Plumbing** `risk:high` `depends:[]`
  > After this: PopupManager, UnityInputBlocker, IPopupView, and UnityViewContainer all wired for async animation. Opening a popup in play mode logs "AnimateIn called" and closing logs "AnimateOut called" with correct input timing.

- [ ] **S02: TMP Prefab Kit** `risk:medium` `depends:[S01]`
  > After this: TMP in asmdefs, 8 prefab assets exist (BigPopup, SmallPopup, 3 buttons, 3 text styles). ConfirmDialog rebuilt from prefabs as proof-of-concept.

- [ ] **S03: Wire All Popups to Prefabs** `risk:low` `depends:[S02]`
  > After this: All 6 popup views use prefab components. SceneSetup regenerates Boot scene cleanly. All 6 popups usable via PopupManager.

- [ ] **S04: Integration Verification** `risk:low` `depends:[S03]`
  > After this: Boot scene regenerated and committed, EditMode tests green, console log confirms blocker timing, UAT script written.

## Boundary Map

### S01 → S02

Produces:
- `IPopupView` — `AnimateInAsync(CancellationToken): UniTask` and `AnimateOutAsync(CancellationToken): UniTask`
- `PopupViewBase` — abstract MonoBehaviour with default LitMotion implementations (bounce-up in, scale+fade out)
- `IInputBlocker` — `FadeInAsync(CancellationToken): UniTask` and `FadeOutAsync(CancellationToken): UniTask`
- `UnityInputBlocker` — implements fade methods; CanvasGroup alpha animated with LitMotion
- `UnityViewContainer.ShowPopupAsync/HidePopupAsync` — calls AnimateIn/AnimateOut on the view + blocker
- `PopupManager` — orchestration: block+fadeIn concurrent with animateIn; animateOut+fadeOut concurrent with unblock-at-start

Consumes:
- nothing (first slice)

### S02 → S03

Produces:
- `Assets/Prefabs/UI/BigPopupWindow.prefab` — full-screen dimmed backdrop + centered panel, TMP title/body slots
- `Assets/Prefabs/UI/SmallPopupWindow.prefab` — smaller centered panel variant
- `Assets/Prefabs/UI/Buttons/PositiveButton.prefab` — green button, TMP label
- `Assets/Prefabs/UI/Buttons/DestructiveButton.prefab` — red button, TMP label
- `Assets/Prefabs/UI/Buttons/NeutralButton.prefab` — grey button, TMP label
- `Assets/Prefabs/UI/Text/TitleText.prefab` — TMP text, large, center-aligned
- `Assets/Prefabs/UI/Text/BodyText.prefab` — TMP text, medium, center-aligned
- `Assets/Prefabs/UI/Text/ButtonLabel.prefab` — TMP text, small, for button children
- `SimpleGame.Game.asmdef` — updated with TMP reference
- `SimpleGame.Editor.asmdef` — updated with TMP editor reference

Consumes from S01:
- `PopupViewBase` — all popup view scripts inherit this base class

### S03 → S04

Produces:
- `SceneSetup.CreateBootScene()` — rebuilt to instantiate prefab components for all 6 popups; TMP fields wired
- All 6 `*View.cs` — inherit `PopupViewBase`, fields typed as `TMP_Text` and `Button`

Consumes from S02:
- All 8 prefab assets
- TMP asmdef references

### S04 (terminal)

Produces:
- Regenerated `Assets/Scenes/Boot.unity` — SceneSetup-generated with full prefab wiring
- `S04-UAT.md` — human test script for open/close popup animation verification

Consumes from S03:
- Updated SceneSetup
- All 6 wired popup views
