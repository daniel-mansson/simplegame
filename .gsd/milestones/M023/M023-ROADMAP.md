# M023: In-Game Camera Movement

## Vision
An auto-tracking camera system for the in-game puzzle scene that smoothly pans and zooms to frame valid placement positions after each piece is placed. The camera respects configurable zoom limits and board boundary clamping. Manual override via drag-pan and pinch/scroll-to-zoom is supported, with auto-tracking resuming on the next piece placement. Level start shows a full board overview before zooming to the first valid area.

## Slice Overview
| ID | Slice | Risk | Depends | Done | After this |
|----|-------|------|---------|------|------------|
| S01 | Auto-Tracking Camera Core | high | — | ⬜ | After this: camera smoothly pans and zooms to frame all valid placement positions after each piece is placed. CameraConfig ScriptableObject controls speed and zoom limits. |
| S02 | Manual Input & Boundary Enforcement | medium | S01 | ⬜ | After this: player can drag to pan and pinch/scroll to zoom, overriding auto-track. Zoom clamped to configured limits. Camera can't drift beyond board bounds + margin. Next placement resumes auto-tracking. |
| S03 | Level Start Sequence & Polish | low | S01, S02 | ⬜ | After this: level begins with full board overview, then smoothly zooms into the first valid placement area. Edge cases (extreme aspect ratios, very large boards) handled gracefully. |
