using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Orthographic camera pan controller. Translates the camera XY by pointer drag,
    /// but only when the pointer-down does NOT hit a UGUI element.
    ///
    /// Attach to the Main Camera in the InGame scene. Works for both mouse and touch.
    /// UGUI Screen Space Overlay elements naturally block pointer events before they
    /// reach this handler, so slot buttons and board drag never conflict.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary>Whether panning is currently active.</summary>
        private bool _isPanning;

        /// <summary>World-space position where the drag started (at z=0 plane).</summary>
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
            HandlePointer();
        }

        private void HandlePointer()
        {
            bool pointerDown  = Input.GetMouseButtonDown(0);
            bool pointerHeld  = Input.GetMouseButton(0);
            bool pointerUp    = Input.GetMouseButtonUp(0);

            if (pointerDown)
            {
                // Only start pan when the pointer is NOT over a UGUI element
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                _isPanning       = true;
                _panStartWorld   = ScreenToWorldXY(Input.mousePosition);
                _cameraStartPos  = transform.position;
            }

            if (_isPanning && pointerHeld)
            {
                Vector3 currentWorld = ScreenToWorldXY(Input.mousePosition);
                Vector3 delta        = _panStartWorld - currentWorld;
                transform.position   = new Vector3(
                    _cameraStartPos.x + delta.x,
                    _cameraStartPos.y + delta.y,
                    _cameraStartPos.z);
            }

            if (pointerUp)
            {
                _isPanning = false;
            }
        }

        /// <summary>
        /// Projects a screen position to world XY at z=0 using the orthographic camera.
        /// </summary>
        private Vector3 ScreenToWorldXY(Vector3 screenPos)
        {
            if (_camera == null) return Vector3.zero;
            screenPos.z = -_camera.transform.position.z;
            return _camera.ScreenToWorldPoint(screenPos);
        }
    }
}
