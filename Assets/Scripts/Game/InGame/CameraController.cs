using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Orthographic camera pan controller. Translates the camera XY by pointer drag,
    /// but only when the pointer-down does NOT hit a UGUI element.
    ///
    /// Attach to the Main Camera in the InGame scene. Works for both mouse and touch.
    /// UGUI Screen Space Overlay elements block pointer events for the tray/HUD areas;
    /// drags on open board space pan the camera freely.
    ///
    /// Key detail: IsPointerOverGameObject(-1) must be used for mouse input.
    /// The no-argument overload uses pointer ID 0 (touch finger 0), which returns
    /// incorrect results for mouse-driven play and breaks pan in the editor.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary>Whether panning is currently active.</summary>
        private bool _isPanning;

        /// <summary>World-space position where the drag started (projected to z=0 plane).</summary>
        private Vector3 _panStartWorld;

        /// <summary>Camera position at drag start.</summary>
        private Vector3 _cameraStartPos;

        private Camera _camera;

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
                // Pass -1 for mouse pointer ID — the no-arg overload uses touch ID 0 which
                // gives incorrect results for mouse input and always returns false.
                if (IsOverUI(-1)) return;

                _isPanning      = true;
                _panStartWorld  = ScreenToWorldXY(Input.mousePosition);
                _cameraStartPos = transform.position;
            }

            if (_isPanning && Input.GetMouseButton(0))
            {
                ApplyPan(ScreenToWorldXY(Input.mousePosition));
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

                _isPanning      = true;
                _panStartWorld  = ScreenToWorldXY(touch.position);
                _cameraStartPos = transform.position;
            }
            else if (_isPanning && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
            {
                ApplyPan(ScreenToWorldXY(touch.position));
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isPanning = false;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void ApplyPan(Vector3 currentWorld)
        {
            Vector3 delta      = _panStartWorld - currentWorld;
            transform.position = new Vector3(
                _cameraStartPos.x + delta.x,
                _cameraStartPos.y + delta.y,
                _cameraStartPos.z);
        }

        private static bool IsOverUI(int pointerId)
        {
            return EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        /// <summary>
        /// Projects a screen position to world XY at the board plane (z=0).
        /// </summary>
        private Vector3 ScreenToWorldXY(Vector3 screenPos)
        {
            if (_camera == null) return Vector3.zero;
            screenPos.z = -_camera.transform.position.z;
            return _camera.ScreenToWorldPoint(screenPos);
        }
    }
}
