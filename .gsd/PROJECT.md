# SimpleGame

## What This Is

A mobile-style puzzle game built in Unity. Players restore a meta-world by completing jigsaw-style puzzle levels. The game loop is: tap a piece from a deck, the model validates placement by neighbor-presence, a heart is lost on wrong attempts, all pieces placed wins the level.

## Core Value

The puzzle placement model — pure domain logic that is board-shape agnostic, testable in plain C#, and completely decoupled from any rendering concern.

## Current State

- Full MVP architecture foundation in place (M001–M003)
- Core/Game assembly split, ScreenManager, PopupManager, transitions (M002–M005)
- Meta world: environments, restorable objects, golden pieces, progression (M006)
- Prefab-based view management, popup stack, animated popups (M007–M008)
- In-scene screen switching, coins, currency overlay HUD (M009)
- `simple-jigsaw` added as git submodule; JigsawDemo scene proves pipeline renders (M010)
- InGame scene exists with stub gameplay (hearts + correct/incorrect counter); no real puzzle model yet

## Architecture / Key Patterns

- MVP strict separation: Views are MonoBehaviours exposing interfaces; Presenters are plain C#; Models are domain services
- Presenters expose awaitable result methods (`WaitForAction()` etc.); SceneControllers compose control flow via async/await
- Views signal intent via `event Action`; no backward references to presenters or services
- `UIFactory` is the single wiring point for presenter construction
- `ScreenManager<TScreenId>` for scene-based navigation; `PopupManager<TPopupId>` for layered popups
- `IViewResolver` for resolving popup views across scenes
- `simple-jigsaw` package at `Packages/simple-jigsaw/` (git submodule)
- Assembly structure: `SimpleGame.Core` (no game deps), `SimpleGame.Game` (references Core), `SimpleGame.Puzzle` (pure C#, no Unity — planned M011)

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — Screen management, presenter pattern, boot flow
- [x] M002: Assembly Restructure — Core/Game separation, asmdef isolation
- [x] M003: SceneController Architecture — Async control flow, awaitable presenters
- [x] M004: Game Loop — Meta-progression, context passing, win/lose flow
- [x] M005: Prefab-Based Transitions — LitMotion fade transitions, swappable prefab
- [x] M006: Puzzle Tap Game Skeleton — Stub InGame with hearts and piece counter
- [x] M007: Prefab-Based View Management — IViewResolver, popup prefabs, container rename
- [x] M008: Popup Animation & UI Component Kit — PopupViewBase, blocker fade, TMP kit
- [x] M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD — InSceneScreenManager, coin balance, stacked popups
- [x] M010: Simple Jigsaw Package Integration — git submodule, local UPM package, JigsawDemo scene
- [x] M011: Puzzle Domain Model & API — Pure puzzle model, jigsaw adapter, InGame wired with real placement logic and rendered tappable pieces
- [ ] M012: Stable Core Game — PuzzleModel Refactor — Replace buggy PuzzleSession + tray-window logic with PuzzleModel owning board, shared deck, and N explicitly-tracked slots
