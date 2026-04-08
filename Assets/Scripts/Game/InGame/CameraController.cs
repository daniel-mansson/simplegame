using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Perspective camera controller with free pan (XY) and Z-based zoom.
    /// FOV is fixed; zoom moves the camera along Z.
    ///
    /// Public API:
    ///   SetTarget(center, z)  — smooth-animate to a world position + zoom level
    ///   SnapTo(center, z)     — teleport instantly
    ///   SetBoardBounds(rect)  — set XY clamp region
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ── Drag-pan state ─────────────────────────────────────────────────
        private bool    _isPanning;
        private Vector2 _lastScreenPos;

        private Camera _camera;

        // ── Auto-tracking state ────────────────────────────────────────────
        [SerializeField] private CameraConfig _config;

        private bool    _isAutoTracking;
        private Vector3 _targetPosition;   // includes target Z
        private Vector3 _posVelocity;

        // ── Board bounds state (kept for gizmo display only) ───────────────
        private Rect _boardRect;
        private bool _hasBoardRect;

        // ── Debug state (populated by callers, drawn by OnDrawGizmos) ──────
        private List<Vector3> _debugPlaceablePositions = new();
        private Vector3 _debugFramingCenter;
        private float _debugFramingZ;
        private bool _debugHasFraming;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Start()
        {
            if (_camera != null && _config != null)
            {
                _camera.orthographic = false;
                _camera.fieldOfView = _config.FieldOfView;
            }
        }

        // ── Mouse ──────────────────────────────────────────────────────────

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsOverUI(-1)) return;
                _isPanning      = true;
                _isAutoTracking = false;
                _lastScreenPos  = Input.mousePosition;
            }

            if (_isPanning && Input.GetMouseButton(0))
            {
                Vector2 current = Input.mousePosition;
                ApplyScreenDelta(current - _lastScreenPos);
                _lastScreenPos = current;
            }

            if (Input.GetMouseButtonUp(0))
                _isPanning = false;

            // Scroll-wheel zoom
            if (_config != null && _camera != null)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _isAutoTracking = false;
                    ApplyZoom(-scroll * _config.ZoomSpeed * Time.deltaTime);
                }
            }
        }

        // ── Touch ──────────────────────────────────────────────────────────

        private void HandleTouch()
        {
            if (Input.touchCount == 0)
            {
                _isPanning = false;
                return;
            }

            if (Input.touchCount >= 2)
            {
                _isAutoTracking = false;
                _isPanning = false;

                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 prev0 = t0.position - t0.deltaPosition;
                Vector2 prev1 = t1.position - t1.deltaPosition;
                float prevDist = (prev0 - prev1).magnitude;
                float currDist = (t0.position - t1.position).magnitude;
                float delta    = prevDist - currDist;

                if (_config != null && _camera != null)
                    ApplyZoom(delta * _config.ZoomSpeed * 0.01f);

                return;
            }

            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsOverUI(touch.fingerId)) return;
                _isPanning      = true;
                _isAutoTracking = false;
                _lastScreenPos  = touch.position;
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

        private void ApplyScreenDelta(Vector2 screenDelta)
        {
            if (_camera == null || Screen.height == 0) return;

            float z = Mathf.Abs(transform.position.z);
            float worldHeight = 2f * CameraMath.FrustumHalfHeight(z, _camera.fieldOfView);
            float worldPerPixel = worldHeight / Screen.height;

            Vector3 pos = transform.position;
            pos.x -= screenDelta.x * worldPerPixel;
            pos.y -= screenDelta.y * worldPerPixel;
            transform.position = pos;
        }

        private void ApplyZoom(float delta)
        {
            if (_config == null) return;

            Vector3 pos = transform.position;
            float z = Mathf.Abs(pos.z) + delta;
            z = Mathf.Clamp(z, _config.MinZ, _config.MaxZ);
            pos.z = -z;
            transform.position = pos;
        }

        private static bool IsOverUI(int pointerId)
            => EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject(pointerId);

        // ── Auto-tracking public API ───────────────────────────────────────

        public void SetConfig(CameraConfig config)
        {
            _config = config;
            if (_camera != null && config != null)
            {
                _camera.orthographic = false;
                _camera.fieldOfView = config.FieldOfView;
            }
        }

        public void SetTarget(Vector3 center, float z)
        {
            if (_config != null)
                z = Mathf.Clamp(z, _config.MinZ, _config.MaxZ);

            float yOffset = _config != null ? _config.TargetYOffset : 0f;
            _targetPosition = new Vector3(center.x, center.y + yOffset, -z);
            _posVelocity    = Vector3.zero;
            _isAutoTracking = true;
        }

        public void SnapTo(Vector3 center, float z)
        {
            if (_config != null)
                z = Mathf.Clamp(z, _config.MinZ, _config.MaxZ);

            float yOffset = _config != null ? _config.TargetYOffset : 0f;
            transform.position = new Vector3(center.x, center.y + yOffset, -z);

            _isAutoTracking = false;
            _posVelocity    = Vector3.zero;
        }

        public void SetBoardBounds(Rect boardRect)
        {
            _boardRect    = boardRect;
            _hasBoardRect = true;
        }

        /// <summary>True while the camera is auto-tracking toward a target.</summary>
        public bool IsAutoTracking => _isAutoTracking;

        /// <summary>The active CameraConfig (may be null if not yet assigned).</summary>
        public CameraConfig Config => _config;

        // ── Debug API — call from presenters to feed gizmo data ────────────

        /// <summary>
        /// Feed the latest placeable positions and computed framing target for gizmo drawing.
        /// Called by presenters after computing camera framing.
        /// </summary>
        public void SetDebugFraming(List<Vector3> placeablePositions, Vector3 framingCenter, float framingZ)
        {
            _debugPlaceablePositions.Clear();
            if (placeablePositions != null)
                _debugPlaceablePositions.AddRange(placeablePositions);
            _debugFramingCenter = framingCenter;
            _debugFramingZ = framingZ;
            _debugHasFraming = true;
        }

        // ── LateUpdate ─────────────────────────────────────────────────────

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouse();
#else
            HandleTouch();
#endif

            if (_isAutoTracking && _config != null)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position, _targetPosition, ref _posVelocity, _config.SmoothTime);

                if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
                {
                    transform.position = _targetPosition;
                    _isAutoTracking = false;
                }
            }
        }

        // ── Gizmos ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float camZ = Mathf.Abs(transform.position.z);
            float fov  = _camera != null ? _camera.fieldOfView : (_config != null ? _config.FieldOfView : 60f);
            float aspect = _camera != null ? _camera.aspect : 1.78f;

            // ── Current viewport frustum at board plane (Z=0) ──────────────
            float halfH = CameraMath.FrustumHalfHeight(camZ, fov);
            float halfW = halfH * aspect;
            Vector3 camXY = new Vector3(transform.position.x, transform.position.y, 0f);

            Gizmos.color = Color.white;
            DrawRectAtZ0(camXY, halfW, halfH);

            // ── Board bounds ───────────────────────────────────────────────
            if (_hasBoardRect)
            {
                Gizmos.color = Color.green;
                Vector3 boardCenter = new Vector3(_boardRect.center.x, _boardRect.center.y, 0f);
                DrawRectAtZ0(boardCenter, _boardRect.width * 0.5f, _boardRect.height * 0.5f);
            }

            // ── Auto-tracking target ───────────────────────────────────────
            if (_isAutoTracking)
            {
                float targetZ = Mathf.Abs(_targetPosition.z);
                float targetHalfH = CameraMath.FrustumHalfHeight(targetZ, fov);
                float targetHalfW = targetHalfH * aspect;
                Vector3 targetXY = new Vector3(_targetPosition.x, _targetPosition.y, 0f);

                // Target viewport
                Gizmos.color = Color.cyan;
                DrawRectAtZ0(targetXY, targetHalfW, targetHalfH);

                // Target center crosshair
                Gizmos.color = Color.cyan;
                float cross = 0.05f;
                Gizmos.DrawLine(targetXY - Vector3.right * cross, targetXY + Vector3.right * cross);
                Gizmos.DrawLine(targetXY - Vector3.up * cross, targetXY + Vector3.up * cross);

                // Line from current to target
                Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
                Gizmos.DrawLine(camXY, targetXY);
            }

            // ── Placeable piece positions ──────────────────────────────────
            if (_debugPlaceablePositions != null && _debugPlaceablePositions.Count > 0)
            {
                Gizmos.color = Color.yellow;
                foreach (var pos in _debugPlaceablePositions)
                {
                    Vector3 p = new Vector3(pos.x, pos.y, 0f);
                    Gizmos.DrawWireSphere(p, 0.03f);
                }
            }

            // ── Debug framing (computed target area) ───────────────────────
            if (_debugHasFraming)
            {
                float fz = _debugFramingZ;
                float fHalfH = CameraMath.FrustumHalfHeight(fz, fov);
                float fHalfW = fHalfH * aspect;
                Vector3 fc = new Vector3(_debugFramingCenter.x, _debugFramingCenter.y, 0f);

                Gizmos.color = Color.magenta;
                DrawRectAtZ0(fc, fHalfW, fHalfH);

                // Framing center dot
                float cross = 0.04f;
                Gizmos.DrawLine(fc - Vector3.right * cross, fc + Vector3.right * cross);
                Gizmos.DrawLine(fc - Vector3.up * cross, fc + Vector3.up * cross);
            }

            // ── Camera center on board plane ───────────────────────────────
            Gizmos.color = Color.red;
            float c = 0.03f;
            Gizmos.DrawLine(camXY - Vector3.right * c, camXY + Vector3.right * c);
            Gizmos.DrawLine(camXY - Vector3.up * c, camXY + Vector3.up * c);

            // ── Z range indicators (MinZ / MaxZ frustum) ───────────────────
            if (_config != null)
            {
                // MinZ frustum (most zoomed in)
                float minHalfH = CameraMath.FrustumHalfHeight(_config.MinZ, fov);
                float minHalfW = minHalfH * aspect;
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // dim orange
                DrawRectAtZ0(camXY, minHalfW, minHalfH);

                // MaxZ frustum (most zoomed out)
                float maxHalfH = CameraMath.FrustumHalfHeight(_config.MaxZ, fov);
                float maxHalfW = maxHalfH * aspect;
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f); // dimmer orange
                DrawRectAtZ0(camXY, maxHalfW, maxHalfH);
            }
        }

        private static void DrawRectAtZ0(Vector3 center, float halfW, float halfH)
        {
            var tl = center + new Vector3(-halfW,  halfH, 0f);
            var tr = center + new Vector3( halfW,  halfH, 0f);
            var br = center + new Vector3( halfW, -halfH, 0f);
            var bl = center + new Vector3(-halfW, -halfH, 0f);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }
#endif
    }
}
