---
id: T01
parent: S01
milestone: M014
provides:
  - GridPlanner.ComputeCells with longest-edge = 1 unit contract
  - BoardClipper.Clip accepts boardExtent param to scale silhouette to match unit-space cells
  - BoardFactory rect+clip path passes boardExtent = max(rows,cols) to clipper
  - GridPlannerTests updated with new formula assertions + NonSquare_LongestEdgeIsOne test cases
  - ContourAssemblerTests.AllContourPointsInBoardSpace updated for unitScale bounds
key_files:
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/BoardClipper.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/BoardFactory.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/GridPlannerTests.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/ContourAssemblerTests.cs
key_decisions:
  - "BoardClipper silhouette now accepts boardExtent float to scale from [0,1] to [0,unitScale] space — rect+clip path passes max(rows,cols)"
  - "Tessellation path unaffected — hex/TriHex cells not produced by GridPlanner"
patterns_established:
  - "GridPlanner contract: unitScale = max(rows,cols); cellW = unitScale/cols; cellH = unitScale/rows"
drill_down_paths:
  - .gsd/milestones/M014/slices/S01/tasks/T01-PLAN.md
duration: 25min
verification_result: pass
completed_at: 2026-03-21T18:30:00Z
---

# T01: GridPlanner Sizing + Test Update

**Changed GridPlanner.ComputeCells to longest-edge = 1 unit; fixed BoardClipper rect+clip path; 264/264 tests pass.**

## What Happened

Changed `GridPlanner.ComputeCells` formula from `1/cols × 1/rows` to `unitScale/cols × unitScale/rows` where `unitScale = max(rows, cols)`. A 4×2 grid now produces cells with width=0.5, height=1.0; a 3×3 grid produces 1×1 cells.

Discovered that `BoardClipper.BuildSilhouette` hardcodes shapes in `[0,1]` space. Added a `boardExtent` parameter to `Clip()` and `BuildSilhouette()` so the silhouette scales to match unit-space cell coordinates. `BoardFactory` rect+clip path passes `boardExtent = Mathf.Max(rows, cols)`.

Updated `GridPlannerTests` with the new formula-based assertions and added `ComputeCells_NonSquare_LongestEdgeIsOne` test cases for non-square grids. Fixed `ContourAssemblerTests.AllContourPointsInBoardSpace` to use `unitScale + margin` as the upper bound. `ContourAssemblerNGonTests` and `BoardClipperTests` (tessellation-path only) were unaffected.

## Deviations

Needed to also fix `BoardClipper` and `BoardFactory` for the rect+shape-clip path — not anticipated in the task plan but straightforward.

## Files Created/Modified

- `Packages/simple-jigsaw/.../GridPlanner.cs` — new sizing formula
- `Packages/simple-jigsaw/.../BoardClipper.cs` — boardExtent parameter on Clip/BuildSilhouette
- `Packages/simple-jigsaw/.../BoardFactory.cs` — passes boardExtent to clipper
- `Packages/simple-jigsaw/.../GridPlannerTests.cs` — updated + new non-square test cases
- `Packages/simple-jigsaw/.../ContourAssemblerTests.cs` — unitScale-relative bounds
