---
estimated_steps: 18
estimated_files: 1
skills_used: []
---

# T02: Extend CameraController with SmoothDamp auto-tracking

Add auto-tracking state and SmoothDamp update loop to the existing CameraController MonoBehaviour.

**New state fields:**
- `[SerializeField] private CameraConfig _config;` — assigned at runtime or via SceneSetup
- `private bool _isAutoTracking;`
- `private Vector3 _targetPosition;` — target camera world position
- `private float _targetOrthoSize;` — target orthographic size
- `private Vector3 _posVelocity;` — SmoothDamp velocity ref for position
- `private float _sizeVelocity;` — SmoothDamp velocity ref for ortho size

**New public API:**
- `public void SetConfig(CameraConfig config)` — runtime config injection
- `public void SetTarget(Vector3 center, float orthoSize)` — sets _targetPosition (preserving current Z), _targetOrthoSize (clamped to config min/max), enables _isAutoTracking, resets velocities
- `public bool IsAutoTracking => _isAutoTracking;`

**LateUpdate addition:** After the existing Update() handles input, add a LateUpdate() method that runs SmoothDamp when `_isAutoTracking` is true:
- `transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _posVelocity, config.SmoothTime)`
- `_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _targetOrthoSize, ref _sizeVelocity, config.SmoothTime)`
- If both position and size are within a small epsilon of target, auto-tracking can remain enabled (it won't overshoot due to SmoothDamp)

**Important:** Do NOT modify the existing Update() input handling. S02 will add `_isPanning` → `_isAutoTracking = false` logic. For now, auto-tracking coexists with input — if the user drags, both apply (S02 fixes the override). Keep the existing HandleMouse/HandleTouch methods unchanged.

**Debug logging:** Log `[CameraController] SetTarget center=({x},{y}) ortho={orthoSize}` on each SetTarget call.

## Inputs

- ``Assets/Scripts/Game/InGame/CameraController.cs` — existing drag-pan controller to extend`
- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — config SO created in T01`

## Expected Output

- ``Assets/Scripts/Game/InGame/CameraController.cs` — extended with SetTarget API, SmoothDamp LateUpdate, CameraConfig reference`

## Verification

rg "SetTarget|SmoothDamp|_isAutoTracking|LateUpdate" Assets/Scripts/Game/InGame/CameraController.cs
