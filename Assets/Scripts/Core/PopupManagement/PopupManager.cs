using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Manages a stack of open popups. Orchestrates input blocking and visual
    /// overlay fade in coordination with popup animations.
    ///
    /// Show sequence:
    ///   1. Block() — input blocked immediately
    ///   2. FadeInAsync + ShowPopupAsync (AnimateInAsync) run concurrently
    ///   3. Push popup onto stack
    ///
    /// Dismiss sequence:
    ///   1. Pop popup from stack
    ///   2. Unblock() — input restored immediately (before animation)
    ///   3. FadeOutAsync fired and forgotten (overlay fades in background)
    ///   4. HidePopupAsync (AnimateOutAsync) awaited
    ///
    /// Guards against concurrent operations with _isOperating (no-op on re-entrant calls).
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
        /// Shows a popup: blocks input immediately, then fades the overlay in and
        /// animates the popup in concurrently. No-ops if an operation is in progress.
        /// </summary>
        public async UniTask ShowPopupAsync(TPopupId popupId, CancellationToken ct = default)
        {
            if (_isOperating)
                return;

            _isOperating = true;
            try
            {
                _inputBlocker.Block();

                await UniTask.WhenAll(
                    _inputBlocker.FadeInAsync(ct),
                    _container.ShowPopupAsync(popupId, ct)
                );

                _stack.Push(popupId);
            }
            finally
            {
                _isOperating = false;
            }
        }

        /// <summary>
        /// Dismisses the top popup. Unblocks input immediately (before animation),
        /// fires the overlay fade-out in the background, then awaits the popup exit animation.
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

                if (_stack.Count == 0)
                {
                    // Last popup — unblock immediately and fire fade-out in background
                    _inputBlocker.Unblock();
                    _inputBlocker.FadeOutAsync(ct).Forget();
                }

                await _container.HidePopupAsync(popupId, ct);
            }
            finally
            {
                _isOperating = false;
            }
        }

        /// <summary>
        /// Dismisses all open popups in LIFO order. Unblocks input per popup;
        /// fires the overlay fade-out when the stack becomes empty (not awaited).
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
                    _inputBlocker.Unblock();

                    if (_stack.Count == 0)
                    {
                        // Last popup — fire overlay fade-out in background before await
                        _inputBlocker.FadeOutAsync(ct).Forget();
                    }

                    await _container.HidePopupAsync(popupId, ct);
                }
            }
            finally
            {
                _isOperating = false;
            }
        }
    }
}
