using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.TransitionManagement;

namespace SimpleGame.Core.ScreenManagement
{
    /// <summary>
    /// Plain C# class (not MonoBehaviour) that manages screen navigation using
    /// additive scene loading. Tracks history for back navigation and guards
    /// against concurrent navigation requests.
    ///
    /// When <see cref="ITransitionPlayer"/> is injected, navigation is bracketed by
    /// fade-out / fade-in animations with input blocked for the duration. When null,
    /// behavior is identical to the original implementation.
    /// </summary>
    public class ScreenManager
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ITransitionPlayer _transitionPlayer;
        private readonly IInputBlocker _inputBlocker;
        private readonly Stack<ScreenId> _history = new Stack<ScreenId>();
        private ScreenId? _currentScreen;
        private bool _isNavigating;

        public ScreenId? CurrentScreen => _currentScreen;
        public bool CanGoBack => _history.Count > 0;

        public ScreenManager(ISceneLoader sceneLoader,
                             ITransitionPlayer transitionPlayer = null,
                             IInputBlocker inputBlocker = null)
        {
            _sceneLoader = sceneLoader;
            _transitionPlayer = transitionPlayer;
            _inputBlocker = inputBlocker;
        }

        /// <summary>
        /// Navigates to the specified screen. Unloads the current screen (pushing
        /// it onto the history stack) before loading the new one. No-ops if a
        /// navigation is already in progress.
        ///
        /// When a transition player is present: blocks input, plays fade-out,
        /// performs the scene swap, plays fade-in, then unblocks input (in finally).
        /// </summary>
        public async UniTask ShowScreenAsync(ScreenId screenId, CancellationToken ct = default)
        {
            if (_isNavigating)
                return;

            _isNavigating = true;

            if (_transitionPlayer != null)
                _inputBlocker?.Block();

            try
            {
                if (_transitionPlayer != null)
                    await _transitionPlayer.FadeOutAsync(ct);

                if (_currentScreen.HasValue)
                {
                    var previous = _currentScreen.Value;
                    _history.Push(previous);
                    await _sceneLoader.UnloadSceneAsync(previous.ToString(), ct);
                }

                await _sceneLoader.LoadSceneAdditiveAsync(screenId.ToString(), ct);
                _currentScreen = screenId;

                if (_transitionPlayer != null)
                    await _transitionPlayer.FadeInAsync(ct);
            }
            finally
            {
                _isNavigating = false;
                if (_transitionPlayer != null)
                    _inputBlocker?.Unblock();
            }
        }

        /// <summary>
        /// Navigates back to the previous screen. No-ops if history is empty.
        ///
        /// When a transition player is present: blocks input, plays fade-out,
        /// performs the scene swap, plays fade-in, then unblocks input (in finally).
        /// </summary>
        public async UniTask GoBackAsync(CancellationToken ct = default)
        {
            if (_history.Count == 0)
                return;

            if (_isNavigating)
                return;

            _isNavigating = true;

            if (_transitionPlayer != null)
                _inputBlocker?.Block();

            try
            {
                var previous = _history.Pop();

                if (_transitionPlayer != null)
                    await _transitionPlayer.FadeOutAsync(ct);

                if (_currentScreen.HasValue)
                    await _sceneLoader.UnloadSceneAsync(_currentScreen.Value.ToString(), ct);

                await _sceneLoader.LoadSceneAdditiveAsync(previous.ToString(), ct);
                _currentScreen = previous;

                if (_transitionPlayer != null)
                    await _transitionPlayer.FadeInAsync(ct);
            }
            finally
            {
                _isNavigating = false;
                if (_transitionPlayer != null)
                    _inputBlocker?.Unblock();
            }
        }
    }
}
