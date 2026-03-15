using System;
using SimpleGame.Core.PopupManagement;
using UnityEngine;

namespace SimpleGame.Runtime.PopupManagement
{
    /// <summary>
    /// Unity implementation of IInputBlocker. Uses a CanvasGroup's
    /// blocksRaycasts flag to prevent interaction. Reference-counted so
    /// nested Block/Unblock pairs stay balanced — the CanvasGroup only
    /// becomes interactive again when every Block() has been matched by
    /// an Unblock().
    /// </summary>
    public class UnityInputBlocker : MonoBehaviour, IInputBlocker
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private int _blockCount;

        /// <inheritdoc />
        public void Block()
        {
            _blockCount++;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <inheritdoc />
        public void Unblock()
        {
            _blockCount = Math.Max(0, _blockCount - 1);
            _canvasGroup.blocksRaycasts = _blockCount > 0;
        }

        /// <inheritdoc />
        public bool IsBlocked => _blockCount > 0;
    }
}
