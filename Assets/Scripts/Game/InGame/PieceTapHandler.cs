using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Thin tap handler attached to each puzzle piece GameObject.
    /// Single responsibility: forward the tap with the piece ID to the InGame view.
    /// Contains no game logic, no services, no presenter references.
    ///
    /// Uses <see cref="OnMouseDown"/> which bypasses uGUI raycasts entirely, so the
    /// uGUI-based <c>UnityInputBlocker</c> overlay cannot suppress it. Instead, a
    /// <c>isInputBlocked</c> predicate is injected at initialisation and checked
    /// before every tap is forwarded.
    /// </summary>
    public class PieceTapHandler : MonoBehaviour
    {
        private int _pieceId;
        private InGameView _view;
        private System.Func<bool> _isInputBlocked;

        /// <summary>
        /// Called by InGameSceneController after spawning the piece GameObject.
        /// <paramref name="isInputBlocked"/> returns true when taps should be suppressed
        /// (e.g. while a popup is open). Pass <c>null</c> to never suppress.
        /// </summary>
        public void Initialize(int pieceId, InGameView view, System.Func<bool> isInputBlocked = null)
        {
            _pieceId = pieceId;
            _view = view;
            _isInputBlocked = isInputBlocked;
        }

        private void OnMouseDown()
        {
            if (_isInputBlocked != null && _isInputBlocked())
                return;

            Debug.Log($"[PieceTapHandler] Tapped piece {_pieceId}");
            _view?.NotifyPieceTapped(_pieceId);
        }
    }
}
