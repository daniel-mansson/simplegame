---
id: S01
milestone: M014
provides:
  - GridPlanner.ComputeCells with longest-edge = 1 unit contract
  - BoardClipper scales silhouette to match board extent (rect+clip path)
  - All 264 package EditMode tests pass
key_files:
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/BoardClipper.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/BoardFactory.cs
  - Packages/simple-jigsaw/Assets/SimpleJigsaw/Tests/EditMode/GridPlannerTests.cs
key_decisions:
  - "unitScale = max(rows,cols); cellW = unitScale/cols; cellH = unitScale/rows"
  - "BoardClipper.Clip now accepts optional boardExtent float"
completed_at: 2026-03-21T18:30:00Z
---

# S01: Package — Unit-Scale Pieces

**GridPlanner now produces pieces with longest edge = 1 unit. 264/264 tests pass.**

Single task: changed GridPlanner formula, fixed BoardClipper for rect+clip path, updated GridPlannerTests and ContourAssemblerTests.
