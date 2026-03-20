# SimpleGame

## What This Is

A Unity mobile jigsaw puzzle game. Players are presented with a set of slots showing jigsaw pieces drawn from a deck. Tapping a slot attempts to place that piece on the board; it succeeds only if the piece has a placed neighbour. Seed pieces are pre-placed as starting anchors. The game completes when all pieces are placed.

## Core Value

The puzzle placement mechanic — slot-based piece drawing with neighbour-adjacency constraint — must always work correctly and produce solvable puzzles.

## Current State

Core game loop is complete and stable (M012). The jigsaw integration (SimpleJigsaw package), PuzzleModel, PuzzleBoard, Deck, and JigsawLevelFactory are all implemented and tested. M013 is adding a topology-aware shuffle to guarantee solvability by construction.

## Architecture / Key Patterns

- **Domain model** (`SimpleGame.Puzzle`, `noEngineReferences: true`): `PuzzleModel`, `PuzzleBoard`, `Deck`, `IPuzzlePiece` — pure C#, no Unity deps
- **Game layer** (`SimpleGame.Game`): `JigsawLevelFactory` bridges SimpleJigsaw ↔ domain model; `InGameSceneController` drives the game loop
- **Core layer** (`SimpleGame.Core`): UI, popup management, scene transitions
- **SimpleJigsaw package** (`Packages/simple-jigsaw`): board generation and rendering
- Presenter pattern for UI; event-driven model (OnPiecePlaced, OnSlotChanged, OnCompleted, OnRejected)
- GSD branch-per-slice git strategy

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation
- [x] M002: Assembly Restructure — Core/Game Separation
- [x] M003: SceneController Architecture — Async Control Flow
- [x] M004: Game Loop — Meta-Progression, Context Passing, Win/Lose Flow
- [x] M005: Prefab-Based Transitions
- [x] M006: Puzzle Tap Game Skeleton
- [x] M007: Prefab-Based View Management
- [x] M008: Popup Animation & UI Component Kit
- [x] M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD
- [x] M010: Simple Jigsaw Package Integration
- [x] M011: Puzzle Domain Model & API
- [x] M012: Stable Core Game — PuzzleModel Refactor
- [ ] M013: Solvable Deck Shuffle — topology-aware deck generation guaranteeing solvability by construction
