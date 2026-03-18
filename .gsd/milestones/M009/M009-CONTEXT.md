# M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

Unity puzzle tap game with MVP architecture. Scenes contain SceneControllers. Popups are managed by PopupManager. Navigation between scenes is handled by ScreenManager (scene loading). Boot scene holds shared infrastructure (InputBlocker, TransitionPlayer, ViewContainer).

## Why This Milestone

The game needs richer in-scene navigation (shop, future game modes) without full scene transitions. Popups need to stack (shop over LevelFailed). A coin currency provides meaningful "continue" monetization. The overlay HUD makes currency visible at the right moments without cluttering persistent UI.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Open the shop from the main menu (switches to a Shop screen within the MainMenu scene — no scene load)
- Fail a level, see a Continue option costing 100 coins, tap it
- If they can't afford it: a shop popup opens on top of the LevelFailed popup; buy coins; shop closes; LevelFailed is back on top with updated balance
- If they can afford it: 100 coins deducted, level continues
- See a coin balance overlay appear and disappear contextually (e.g. when LevelFailed is shown)

### Entry point / environment

- Entry point: Unity Play mode — MainMenu scene or InGame scene
- Environment: local dev (Unity Editor)
- Live dependencies: none (fake IAP, no real store)

## Completion Class

- Contract complete means: EditMode tests green; CoinsService unit-tested; stacking sequence unit-tested
- Integration complete means: LevelFailed → shop popup stack works in play mode; coin balance persists across sessions
- Operational complete means: none

## Final Integrated Acceptance

- Fail a level; tap Continue (costs 100 coins, balance is 0); shop popup appears stacked on LevelFailed; buy a coin pack; balance updates; shop closes; LevelFailed Continue button now works; level continues
- Open main menu → tap Shop → Shop screen shown; tap Back → world view restored
- Coin balance overlay appears when LevelFailed is open, disappears after dismiss

## Risks and Unknowns

- **Popup sort order for stacking visual** — when popup B stacks on A, the dim overlay must sit between them. Currently all popups are siblings in PopupCanvas (sort order 300), blocker is at 100. Need per-popup Canvas with OverrideSorting to put bottom popup below blocker and top popup above. `UnityViewContainer.ShowPopupAsync` must set sort order based on stack depth.
- **`_isOperating` guard** — current guard blocks concurrent operations, which is correct. Sequential stacking (show B after A's `WaitForChoice` returns) doesn't hit the guard, so stacking works without removing it. Only need to verify the dim overlay stays visible and the sort order trick works.
- **InSceneScreenManager scope** — this is a new Core abstraction. Must stay generic (no game types). `MainMenuSceneController` wires it with a game-specific `MainMenuScreenId` enum.

## Existing Codebase / Prior Art

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — full scene ScreenManager, generic `TScreenId` pattern. InSceneScreenManager mirrors this but uses `SetActive` instead of scene loading.
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — has `Stack<TPopupId>`, reference-counted blocker. Already supports stacking semantically; `_isOperating` guard only blocks re-entrant concurrent calls (fine).
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — reference-counted Block/Unblock, sort order 100.
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — `IPopupContainer<PopupId>` + `IViewResolver`. Must be extended for sort-order management.
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — exact pattern for CoinsService.
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — add `int coins` field; JsonUtility-serializable.
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs` — existing fake IAP stub; ShopPresenter wraps it for coin packs.
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — wires all services; will need `CoinsService` + overlay wired here.
- `Assets/Scripts/Game/Boot/UIFactory.cs` — presenter factory; will need shop/overlay presenter factory methods.
- `Assets/Editor/SceneSetup.cs` — generates Boot.unity; must add overlay canvas + wire new services.
- `Assets/Editor/PrefabKitSetup.cs` — generates popup prefabs; must add ShopPopup prefab.

## Relevant Requirements

- R084 — In-scene screen switching within a scene (this milestone introduces it)
- R085 — Popup stacking with correct visual layering
- R086 — Coins currency, persisted, separate from golden pieces
- R087 — LevelFailed Continue option costing 100 coins
- R088 — Shop accessible from main menu and LevelFailed; fake IAP coin packs
- R089 — Contextual overlay HUD showing coin balance

## Scope

### In Scope

- `InSceneScreenManager<TScreenId>` in Core (SetActive panel swap, back stack)
- MainMenu gains `MainMenuScreenId` enum (Home, Shop); Shop screen panel added to MainMenu scene
- `CoinsService` + `ICoinsService` (persisted in `MetaSaveData.coins`)
- `LevelFailedChoice.Continue`; LevelFailed popup gets Continue button; deducts 100 coins or opens Shop popup
- `ShopPopup` prefab with 3 coin pack buttons; `ShopPresenter` wraps fake IAP; accessible as popup (from LevelFailed) and as screen (from MainMenu)
- Overlay HUD canvas (sort order between blocker 100 and popups 300); `ICurrencyOverlay`; `UnityOverlay` with LitMotion show/hide; coin balance display
- `UnityViewContainer` extended: sets per-popup Canvas Override Sort Order based on stack depth (bottom popup < blocker 100, top popup > blocker 100)
- New `PopupId.Shop`
- SceneSetup, PrefabKitSetup updates

### Out of Scope / Non-Goals

- Real IAP SDK integration (Apple/Google)
- Animated screen transitions (instant swap only)
- Overlay reacting to golden piece changes (coins only for now)
- Multiple overlay content types (currency only)

## Technical Constraints

- All existing 169 EditMode tests must stay green
- MVP pattern: no backward refs from views to presenters/services
- `InSceneScreenManager` must live in Core with no game-specific types
- Popup sort order scheme: base popups at sort order 50, blocker at 100, stacked popups at 150+ (each +50 per depth)

## Integration Points

- `GameBootstrapper` — constructs `CoinsService`, overlay, wires everything
- `UnityViewContainer` — manages popup sort orders; extended for stack depth awareness
- `InGameSceneController` — uses `CoinsService` for Continue
- `MainMenuSceneController` — wires `InSceneScreenManager`, navigates to Shop screen

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.
