using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Orthographic camera pan controller. Drags the camera XY by following the pointer,
    /// but only when pointer-down does NOT hit a UGUI element.
    ///
    /// Approach: track the delta in screen pixels each frame and convert to world units
    /// via the orthographic size. This avoids the feedback loop that occurs when the
    /// anchor is stored in world space and re-projected through a moving camera.
    ///
    /// Key detail: IsPointerOverGameObject(-1) must be used for mouse input.
    /// The no-argument overload uses pointer ID 0 (touch finger 0) and gives
    /// incorrect results for mouse-driven play.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ── Drag-pan state ─────────────────────────────────────────────────
        private bool    _isPanning;
        private Vector2 _lastScreenPos; // screen-pixel position from the previous frame

        private Camera _camera;

        // ── Auto-tracking state ────────────────────────────────────────────
        [SerializeField] private CameraConfig _config;

        private bool  _isAutoTracking;
        private Vector3 _targetPosition;
        private float _targetOrthoSize;
        private Vector3 _posVelocity;
        private float _sizeVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        // ── Mouse ──────────────────────────────────────────────────────────

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Pass -1 for mouse pointer ID — the no-arg overload uses touch ID 0.
                if (IsOverUI(-1)) return;

                _isPanning     = true;
                _lastScreenPos = Input.mousePosition;
            }

            if (_isPanning && Input.GetMouseButton(0))
            {
                Vector2 current = Input.mousePosition;
                ApplyScreenDelta(current - _lastScreenPos);
                _lastScreenPos = current;
            }

            if (Input.GetMouseButtonUp(0))
                _isPanning = false;
        }

        // ── Touch ──────────────────────────────────────────────────────────

        private void HandleTouch()
        {
            if (Input.touchCount == 0)
            {
                _isPanning = false;
                return;
            }

            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsOverUI(touch.fingerId)) return;

                _isPanning     = true;
                _lastScreenPos = touch.position;
            }
            else if (_isPanning && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
            {
                ApplyScreenDelta(touch.position - _lastScreenPos);
                _lastScreenPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isPanning = false;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a screen-pixel delta to world units and moves the camera in the
        /// opposite direction (drag world under finger).
        /// </summary>
        private void ApplyScreenDelta(Vector2 screenDelta)
        {
            if (_camera == null || Screen.height == 0) return;

            // World units per pixel: orthoSize covers half the screen height.
            float worldPerPixelY = (_camera.orthographicSize * 2f) / Screen.height;
            float worldPerPixelX = worldPerPixelY * ((float)Screen.width / Screen.height)
                                   / ((float)Screen.width / Screen.height);
            // Simplifies to the same scale on both axes for ortho:
            worldPerPixelX = worldPerPixelY;

            Vector3 pos = transform.position;
            pos.x -= screenDelta.x * worldPerPixelX;
            pos.y -= screenDelta.y * worldPerPixelY;
            transform.position = pos;
        }

        private static bool IsOverUI(int pointerId)
            => EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject(pointerId);

        // ── Auto-tracking public API ───────────────────────────────────────

        /// <summary>Inject a CameraConfig at runtime (e.g. from SceneSetup or a presenter).</summary>
        public void SetConfig(CameraConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Point the auto-tracker at a world-space center and desired orthographic size.
        /// Enables auto-tracking; velocity refs are reset so the camera starts smoothly
        /// from its current position each time a new target is set.
        /// </summary>
        public void SetTarget(Vector3 center, float orthoSize)
        {
            // Preserve the camera's current Z depth.
            _targetPosition = new Vector3(center.x, center.y, transform.position.z);

            if (_config != null)
                _targetOrthoSize = Mathf.Clamp(orthoSize, _config.MinZoom, _config.MaxZoom);
            else
                _targetOrthoSize = orthoSize;

            // Reset velocity refs so SmoothDamp starts from a clean state.
            _posVelocity  = Vector3.zero;
            _sizeVelocity = 0f;

            _isAutoTracking = true;

            Debug.Log($"[CameraController] SetTarget center=({center.x},{center.y}) ortho={_targetOrthoSize}");
        }

        /// <summary>True while the camera is auto-tracking toward a target.</summary>
        public bool IsAutoTracking => _isAutoTracking;

        // ── LateUpdate ─────────────────────────────────────────────────────

        private void LateUpdate()
        {
            if (!_isAutoTracking || _config == null || _camera == null) return;

            transform.position = Vector3.SmoothDamp(
                transform.position, _targetPosition, ref _posVelocity, _config.SmoothTime);

            _camera.orthographicSize = Mathf.SmoothDamp(
                _camera.orthographicSize, _targetOrthoSize, ref _sizeVelocity, _config.SmoothTime);
        }
    }
}
