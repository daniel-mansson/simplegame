using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Manages a stack of open popups. Blocks input while any popup is open
    /// and unblocks it once the stack is empty. Guards against concurrent
    /// operations with the _isOperating flag (no-op on re-entrant calls).
    /// </summary>
    public class PopupManager<TPopupId> where TPopupId : struct, System.Enum
    {
        private readonly IPopupContainer<TPopupId> _container;
        private readonly IInputBlocker _inputBlocker;
        private readonly Stack<TPopupId> _stack = new Stack<TPopupId>();
        private bool _isOperating;

        /// <summary>The popup at the top of the stack, or null if none are open.</summary>
        public TPopupId? TopPopup => _stack.Count > 0 ? _stack.Peek() : (TPopupId?)null;

        /// <summary>Number of popups currently on the stack.</summary>
        public int PopupCount => _stack.Count;

        /// <summary>True when at least one popup is open.</summary>
        public bool HasActivePopup => _stack.Count > 0;

        public PopupManager(IPopupContainer<TPopupId> container, IInputBlocker inputBlocker)
        {
            _container = container;
            _inputBlocker = inputBlocker;
        }

        /// <summary>
        /// Shows a popup, blocking input and pushing it onto the stack.
        /// No-ops if an operation is already in progress.
        /// </summary>
        public async UniTask ShowPopupAsync(TPopupId popupId, CancellationToken ct = default)
        {
            if (_isOperating)
                return;

            _isOperating = true;
            try
            {
                _inputBlocker.Block();
                await _container.ShowPopupAsync(popupId, ct);
                _stack.Push(popupId);
            }
            finally
            {
                _isOperating = false;
            }
        }

        /// <summary>
        /// Dismisses the top popup. Unblocks input when the stack becomes empty.
        /// No-ops if an operation is in progress or the stack is empty.
        /// </summary>
        public async UniTask DismissPopupAsync(CancellationToken ct = default)
        {
            if (_isOperating || _stack.Count == 0)
                return;

            _isOperating = true;
            try
            {
                var popupId = _stack.Pop();
                await _container.HidePopupAsync(popupId, ct);

                if (_stack.Count == 0)
                    _inputBlocker.Unblock();
            }
            finally
            {
                _isOperating = false;
            }
        }

        /// <summary>
        /// Dismisses all open popups in LIFO order, unblocking input once per popup.
        /// No-ops if an operation is in progress or the stack is empty.
        /// </summary>
        public async UniTask DismissAllAsync(CancellationToken ct = default)
        {
            if (_isOperating || _stack.Count == 0)
                return;

            _isOperating = true;
            try
            {
                while (_stack.Count > 0)
                {
                    var popupId = _stack.Pop();
                    await _container.HidePopupAsync(popupId, ct);
                    _inputBlocker.Unblock();
                }
            }
            finally
            {
                _isOperating = false;
            }
        }
    }
}
