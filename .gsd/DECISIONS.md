# Decisions Register

<!-- Append-only. Never edit or remove existing rows.
     To reverse a decision, add a new row that supersedes it.
     Read this file at the start of any planning or research phase. -->

| # | When | Scope | Decision | Choice | Rationale | Revisable? |
|---|------|-------|----------|--------|-----------|------------|
| D001 | M001 | arch | UI pattern | MVP with strict separation | User requirement — views independent, presenters plain C#, models injected | No |
| D002 | M001 | arch | Dependency injection approach | Constructor/init injection, no DI framework | User requirement — explicit wiring, no magic, traceable dependencies | No |
| D003 | M001 | constraint | Static state | No static fields holding state | User requirement — must support domain reload disabled in editor | No |
| D004 | M001 | arch | UI system | Legacy uGUI (Canvas/GameObjects) | User decision — battle-tested, well-understood, suits MVP pattern | No |
| D005 | M001 | arch | Scene management | Hybrid — persistent scene + additive loading | User decision — persistent scene for shared UI, additive for screen isolation | Yes — if performance requires full scene switching |
| D006 | M001 | arch | Popup model | Stack-based, multiple popups allowed | User decision — most recent gets focus, dismiss reveals below | Yes — if UX requires single-popup |
| D007 | M001 | arch | Async model | UniTask (Cysharp.Threading.Tasks) | User decision — zero-allocation, CancellationToken support, Unity-native | No |
| D008 | M001 | arch | Transition style | Fade-to-black | User decision — simple, universally understood, good starting point | Yes — if richer transitions needed later |
| D009 | M001 | arch | Input blocking | Full-screen raycast blocker (CanvasGroup) | User decision — simple, covers all cases | Yes — if per-element granularity needed |
| D010 | M001 | arch | Initialization flow | Boot scene → main scene | User decision — clean separation of bootstrap vs runtime | No |
| D011 | M001 | arch | Presenter construction | Central UIFactory, one factory for all presenters | User decision — single wiring point, receives all dependencies | Yes — if per-feature factories needed at scale |
| D012 | M001 | convention | Testing approach | Edit-mode preferred, play-mode for views | User decision — fast feedback, views can use play-mode later | Yes — if play-mode coverage needed |
| D013 | M001 | arch | View backward references | None — views expose interface only, no refs to presenters/models/services | User requirement — enables future view preview tool | No |
