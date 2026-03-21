# S01: Package — Unit-Scale Pieces

**Goal:** Change GridPlanner to produce cells where the longest edge = 1 unit. Update all dependent tests.

**Demo:** EditMode test asserts a 4×2 board has cellW=0.5, cellH=1.0 and board extent = 2×2. A 3×3 board has cellW=cellH=1.0.

## Must-Haves

- `GridPlanner.ComputeCells` for a 4×2 config produces cells with width=0.5, height=1.0
- `GridPlanner.ComputeCells` for a 3×3 config produces cells with width=1.0, height=1.0
- `GridPlanner.ComputeCells` for a 2×4 config produces cells with width=1.0, height=0.5
- Board total extent = `unitScale × unitScale` where `unitScale = max(rows, cols)`
- All `GridPlannerTests` pass with updated assertions
- All other package EditMode tests pass (BoardFactory, FullIntegration, etc.)
- No change to tessellation path (hex/TriHex uses PolygonCell corners directly, not GridPlanner)

## Tasks

- [x] **T01: GridPlanner sizing + test update**
  Change the sizing formula in GridPlanner.ComputeCells. Update GridPlannerTests to assert the new contract. Verify all other package tests still pass.

## Files Likely Touched

- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs`
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/GridPlannerTests.cs`
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/BoardFactoryTests.cs` (SolvedPosition assertions if any)
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/FullIntegrationTests.cs` (structural checks only, likely no change)
