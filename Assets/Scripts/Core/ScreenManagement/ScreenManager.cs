using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.ScreenManagement
{
    /// <summary>
    /// Plain C# class (not MonoBehaviour) that manages screen navigation using
    /// additive scene loading. Tracks history for back navigation and guards
    /// against concurrent navigation requests.
    /// </summary>
    public class ScreenManager
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly Stack<ScreenId> _history = new Stack<ScreenId>();
        private ScreenId? _currentScreen;
        private bool _isNavigating;

        public ScreenId? CurrentScreen => _currentScreen;
        public bool CanGoBack => _history.Count > 0;

        public ScreenManager(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        /// <summary>
        /// Navigates to the specified screen. Unloads the current screen (pushing
        /// it onto the history stack) before loading the new one. No-ops if a
        /// navigation is already in progress.
        /// </summary>
        public async UniTask ShowScreenAsync(ScreenId screenId, CancellationToken ct = default)
        {
            if (_isNavigating)
                return;

            _isNavigating = true;
            try
            {
                if (_currentScreen.HasValue)
                {
                    var previous = _currentScreen.Value;
                    _history.Push(previous);
                    await _sceneLoader.UnloadSceneAsync(previous.ToString(), ct);
                }

                await _sceneLoader.LoadSceneAdditiveAsync(screenId.ToString(), ct);
                _currentScreen = screenId;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Navigates back to the previous screen. No-ops if history is empty.
        /// </summary>
        public async UniTask GoBackAsync(CancellationToken ct = default)
        {
            if (_history.Count == 0)
                return;

            if (_isNavigating)
                return;

            _isNavigating = true;
            try
            {
                var previous = _history.Pop();

                if (_currentScreen.HasValue)
                    await _sceneLoader.UnloadSceneAsync(_currentScreen.Value.ToString(), ct);

                await _sceneLoader.LoadSceneAdditiveAsync(previous.ToString(), ct);
                _currentScreen = previous;
            }
            finally
            {
                _isNavigating = false;
            }
        }
    }
}
