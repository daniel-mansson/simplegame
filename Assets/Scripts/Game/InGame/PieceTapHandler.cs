using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Thin tap handler attached to each puzzle piece GameObject.
    /// Single responsibility: forward the tap with the piece ID to the InGame view.
    /// Contains no game logic, no services, no presenter references.
    /// </summary>
    public class PieceTapHandler : MonoBehaviour
    {
        private int _pieceId;
        private InGameView _view;

        /// <summary>
        /// Called by InGameSceneController after spawning the piece GameObject.
        /// </summary>
        public void Initialize(int pieceId, InGameView view)
        {
            _pieceId = pieceId;
            _view = view;
        }

        private void OnMouseDown()
        {
            _view?.NotifyPieceTapped(_pieceId);
        }
    }
}
