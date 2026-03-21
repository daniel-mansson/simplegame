# T01: GridPlanner Sizing + Test Update

**Slice:** S01
**Milestone:** M014

## Goal

Change `GridPlanner.ComputeCells` so each cell's longest edge = 1 unit. Update `GridPlannerTests` to assert the new contract. Confirm all other package EditMode tests still pass.

## Must-Haves

### Truths
- For a 4×2 grid: `cells[r,c].width ≈ 0.5f`, `cells[r,c].height ≈ 1.0f` for all r, c
- For a 2×4 grid: `cells[r,c].width ≈ 1.0f`, `cells[r,c].height ≈ 0.5f` for all r, c
- For a 3×3 grid: `cells[r,c].width ≈ 1.0f`, `cells[r,c].height ≈ 1.0f` for all r, c
- For a 1×1 grid: `cells[0,0].width ≈ 1.0f`, `cells[0,0].height ≈ 1.0f`
- `Mathf.Max(cell.width, cell.height) ≈ 1.0f` for every cell in every grid
- No-gap invariant still holds: adjacent cells share edges exactly
- Board total extent: `cols * cellW = unitScale`, `rows * cellH = unitScale` where `unitScale = Mathf.Max(rows, cols)`
- All GridPlannerTests pass
- All package EditMode tests pass (no regressions)

### Artifacts
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs` — updated formula, updated doc comment
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/GridPlannerTests.cs` — all test assertions updated for new sizing contract

### Key Links
- `BoardFactory.Generate(GridLayoutConfig, seed)` calls `GridPlanner.ComputeCells` → cell rects flow into `SolvedPosition` and `ContourAssembler.AssembleFromPresampled` — no changes needed in BoardFactory, sizes propagate automatically

## Steps

1. Read `GridPlanner.cs` and note the current formula (`cellW = 1f / cols`, `cellH = 1f / rows`)
2. Compute new formula: `longestNorm = Mathf.Max(1f / cols, 1f / rows)`, `unitScale = 1f / longestNorm` (= `Mathf.Max(rows, cols)`), `cellW = unitScale / cols`, `cellH = unitScale / rows`
3. Update `GridPlanner.ComputeCells` with the new formula. Update the XML doc comment to describe the new contract (longest edge = 1 unit, board extent = max(rows,cols) × max(rows,cols))
4. Read `GridPlannerTests.cs` in full — identify every assertion that references `1f / cols` or `1f / rows`
5. Update `ComputeCells_CellsHaveCorrectSize` — replace `expectedWidth = 1f/cols` / `expectedHeight = 1f/rows` with the new formula
6. Update `ComputeCells_CellsCoverUnitSquare` — board no longer covers the unit square; top-right cell now reaches `(unitScale, unitScale)` not `(1, 1)`. Update assertions accordingly
7. Update `ComputeCells_OriginIsBottomLeft` — origin is still (0,0), no change needed there
8. Check `ComputeCells_CellsHaveNoGaps` — gap check is relative (adjacent edges equal), not absolute — likely no change needed, verify
9. Check `BoardFactoryTests` for any hardcoded `SolvedPosition` or extent assertions — update if found
10. Run all package EditMode tests mentally to identify any other assertions that assume normalized 0–1 space
11. Verify: `Mathf.Max(cellW, cellH) == 1f` for each test case

## Context

- Formula derivation: for a 4×2 grid, normalized longest edge = max(1/4, 1/2) = 0.5. To make 0.5 → 1: multiply by 2. So unitScale = 2. cellW = 2/4 = 0.5, cellH = 2/2 = 1.0. Board = 2.0 × 2.0. ✓
- `ContourAssembler.AssembleFromPresampled(Rect cellBounds, ...)` uses `cellBounds.xMin/xMax/yMin/yMax` — these values change but the assembler is coordinate-agnostic, no change needed
- `HintSurfaceBuilder` default thickness `0.003f` is normalized-space. After this change, callers should use ~`0.02f`. No change needed in S01 — wiring happens in S02
- Tessellation path (`TessellationPlanner`, hex/TriHex) does NOT use `GridPlanner` — unaffected
- Key file to read first: `GridPlannerTests.cs` — it drives the full list of assertions to update
