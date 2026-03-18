using System.Collections.Generic;
using UnityEngine;

namespace SimpleGame.Core.ScreenManagement
{
    /// <summary>
    /// Plain C# implementation of <see cref="IInSceneScreenManager{TScreenId}"/>.
    /// Manages in-scene screens by toggling pre-existing panel GameObjects via SetActive.
    ///
    /// Construct with a mapping of screen IDs to GameObjects. All panels should
    /// be inactive initially; the caller activates the initial screen by calling
    /// <see cref="ShowScreen"/> once at setup, or by passing an optional initial
    /// screen to the constructor.
    ///
    /// Usage:
    ///   var manager = new InSceneScreenManager&lt;MyScreenId&gt;(panels);
    ///   manager.ShowScreen(MyScreenId.Home);
    /// </summary>
    public class InSceneScreenManager<TScreenId> : IInSceneScreenManager<TScreenId>
        where TScreenId : struct, System.Enum
    {
        private readonly Dictionary<TScreenId, GameObject> _panels;
        private readonly Stack<TScreenId> _history = new Stack<TScreenId>();
        private TScreenId? _current;

        /// <inheritdoc/>
        public TScreenId? CurrentScreen => _current;

        /// <inheritdoc/>
        public bool CanGoBack => _history.Count > 0;

        /// <param name="panels">Map of screen ID → panel GameObject. All should start inactive.</param>
        public InSceneScreenManager(Dictionary<TScreenId, GameObject> panels)
        {
            _panels = panels;
        }

        /// <inheritdoc/>
        public void ShowScreen(TScreenId screenId)
        {
            // No-op if already on this screen
            if (_current.HasValue && EqualityComparer<TScreenId>.Default.Equals(_current.Value, screenId))
                return;

            // Deactivate current panel and push to history
            if (_current.HasValue)
            {
                SetPanelActive(_current.Value, false);
                _history.Push(_current.Value);
            }

            // Activate new panel
            _current = screenId;
            SetPanelActive(screenId, true);
        }

        /// <inheritdoc/>
        public void GoBack()
        {
            if (_history.Count == 0)
                return;

            // Deactivate current
            if (_current.HasValue)
                SetPanelActive(_current.Value, false);

            // Restore previous
            var previous = _history.Pop();
            _current = previous;
            SetPanelActive(previous, true);
        }

        private void SetPanelActive(TScreenId screenId, bool active)
        {
            if (_panels.TryGetValue(screenId, out var panel) && panel != null)
                panel.SetActive(active);
            else
                Debug.LogWarning($"[InSceneScreenManager] No panel registered for screen: {screenId}");
        }
    }
}
