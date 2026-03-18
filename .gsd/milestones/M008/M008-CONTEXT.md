# M008: Popup Animation & UI Component Kit

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

Unity 6 mobile puzzle game (Puzzle Tap). All 6 popup types (ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored) currently show/hide instantly via SetActive with no animation. The input blocker is a static full-screen CanvasGroup at sort order 100 with no visual presence. There are no reusable UI component prefabs.

## Why This Milestone

Game feel starts with popup UX. Instant show/hide feels cheap. The dim overlay + animated popup entry/exit is the minimum bar for a shipping mobile game. The prefab component kit is the foundation for visual consistency and future reskinning — building it now while there are only 6 popups is far cheaper than retrofitting later.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Open any popup and see a semi-transparent dim overlay fade in behind it, with the popup itself bouncing up from slightly below center
- Close any popup and see the popup scale+fade out, with the dim overlay fading out simultaneously, and immediately be able to interact with the UI underneath (input unblocks at fade-out start, not fade-out end)
- Observe that all 6 popups use consistent prefab-based window shells, buttons, and text components

### Entry point / environment

- Entry point: Unity Editor play mode, Boot scene
- Environment: Local dev, Unity Editor
- Live dependencies involved: none (all UI, no network/backend)

## Completion Class

- Contract complete means: AnimateIn/AnimateOut wired to all 6 popups, prefab assets exist, SceneSetup regenerates without errors, EditMode tests pass
- Integration complete means: PopupManager orchestrates blocker fade + popup animation correctly, input timing split verified
- Operational complete means: none

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Opening any popup in play mode shows: dim overlay fades in, popup bounces up — and input is blocked immediately
- Closing any popup in play mode shows: dim overlay starts fading out, popup scales down + fades out — and input is immediately restored (before animation finishes)
- All 6 popup window GOs are built from BigPopup or SmallPopup prefab shells with TMP text and prefab buttons

## Risks and Unknowns

- LitMotion's BindToAlpha extension for CanvasGroup — need to verify it's in LitMotion.Extensions or use `.Bind(x => cg.alpha = x)` fallback
- TMP setup in Game asmdef — TextMeshPro package must be added to manifest and `SimpleGame.Game` asmdef must reference it; SceneSetup (Editor asmdef) also needs it
- PopupViewBase as a MonoBehaviour abstract class — all existing View MonoBehaviours must change their base class from `MonoBehaviour` to `PopupViewBase`; existing tests must still compile
- SceneSetup currently uses `UnityEngine.UI.Text` throughout — switching to TMP requires significant CreateText helper changes

## Existing Codebase / Prior Art

- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — orchestrates show/dismiss; needs animation + blocker timing changes
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — needs FadeInAsync/FadeOutAsync or the timing split handled at the orchestration level
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — MonoBehaviour with CanvasGroup; needs alpha animation + timing split
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — ShowPopupAsync/HidePopupAsync currently call SetActive; needs to call AnimateInAsync/AnimateOutAsync
- `Assets/Scripts/Core/MVP/IPopupView.cs` — marker interface; needs AnimateInAsync/AnimateOutAsync added
- `Assets/Editor/SceneSetup.cs` — CreatePopupDialog and CreateButton helpers; major rewrite needed for TMP + prefab-based construction
- `Assets/Scripts/Game/Popup/*View.cs` — 6 view MonoBehaviours that must inherit from PopupViewBase instead of MonoBehaviour
- `Packages/manifest.json` — LitMotion already present; TMP must be added
- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — already references LitMotion + LitMotion.Extensions
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — currently no LitMotion or TMP reference; both must be added
- `Assets/Editor/SimpleGame.Editor.asmdef` — SceneSetup lives here; TMP editor reference must be added

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R079 — animated blocker overlay with visible dim
- R080 — input timing split (block at fade-in start, unblock at fade-out start)
- R081 — IPopupView.AnimateInAsync/OutAsync with PopupViewBase default
- R082 — TMP-based UI prefab kit (BigPopup, SmallPopup, buttons, text)
- R083 — all 6 existing popups wired to prefab components in SceneSetup

## Scope

### In Scope

- IInputBlocker gains async fade methods; UnityInputBlocker animates alpha with LitMotion
- IPopupView gains AnimateInAsync/AnimateOutAsync; PopupViewBase default impl (bounce-up in, scale+fade out)
- PopupManager orchestration: block → fade blocker in + animate popup in; animate popup out + fade blocker out (unblock at fade-out start)
- TMP added to Game and Editor asmdefs
- Prefab assets: BigPopupWindow, SmallPopupWindow, PositiveButton, DestructiveButton, NeutralButton, TitleText, BodyText, ButtonLabel
- All 6 popup views inherit PopupViewBase
- SceneSetup rebuilt to instantiate popup window prefabs and TMP components
- Boot scene regenerated via SceneSetup after all code changes

### Out of Scope / Non-Goals

- Custom per-popup animation overrides (infrastructure ready, not used this milestone)
- Visual art / actual reskin (deferred — milestone creates structural setup only)
- New popup types
- Popup instantiation from prefabs on demand (still pre-instantiated, SetActive)

## Technical Constraints

- LitMotion already in Core asmdef — animation code goes in Core layer where possible
- PopupViewBase must be a MonoBehaviour (Unity serialization/lifecycle)
- All existing EditMode tests must remain green
- SceneSetup (Editor only) must compile after TMP asmdef changes

## Integration Points

- PopupManager (Core, pure C#) — orchestrates IInputBlocker + IPopupContainer flow
- UnityViewContainer (Game) — calls AnimateInAsync/AnimateOutAsync on the view
- UnityInputBlocker (Core Unity) — new FadeInAsync/FadeOutAsync, alpha animation
- Boot scene — regenerated by SceneSetup after all changes

## Open Questions

- BindToAlpha on CanvasGroup — verify LitMotion.Extensions has this; if not, use `.Bind(x => cg.alpha = x)` fallback
- Whether PopupManager should be aware of animation (async show/hide) or delegate entirely to UnityViewContainer — lean toward container owns animation, PopupManager just awaits
