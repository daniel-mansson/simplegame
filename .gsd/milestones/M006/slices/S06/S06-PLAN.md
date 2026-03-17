# S06: Environment progression and full flow integration

**Goal:** Wire all services in GameBootstrapper, update SceneSetup.cs for all new UI, environment progression in MetaProgressionService (1-3 available).
**Demo:** Full flow navigable in play mode after SceneSetup regeneration.

## Must-Haves

- GameBootstrapper constructs and injects all M006 services
- SceneSetup.cs updated for all new view types and fields
- GameBootstrapper wires WorldData via SerializeField
- All code compiles with new types

## Tasks

- [x] **T01: Update GameBootstrapper, SceneSetup.cs** `est:30m`
  - Do: Wire all services, update scene creation for new views
  - Done when: Code compiles, all types referenced correctly

## Files Likely Touched

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
