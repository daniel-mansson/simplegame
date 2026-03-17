using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IMainMenuView.
    /// Text-stub UI for the main screen with meta world.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Text _environmentNameText;
        [SerializeField] private Text _balanceText;
        [SerializeField] private Text _levelDisplayText;
        [SerializeField] private Text _objectsText;

        public event Action OnSettingsClicked;
        public event Action OnPlayClicked;
        public event Action<int> OnObjectTapped;

        private ObjectDisplayData[] _currentObjects;

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        }

        public void UpdateEnvironmentName(string text) => _environmentNameText.text = text;
        public void UpdateBalance(string text) => _balanceText.text = text;
        public void UpdateLevelDisplay(string text) => _levelDisplayText.text = text;

        public void UpdateObjects(ObjectDisplayData[] objects)
        {
            _currentObjects = objects;
            // Text-stub: render objects as a simple text list
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                var status = obj.IsComplete ? "[DONE]"
                    : obj.IsBlocked ? "[BLOCKED]"
                    : $"[{obj.Progress}] (tap: {obj.CostPerStep}gp)";
                sb.AppendLine($"{obj.Name} {status}");
            }
            _objectsText.text = sb.ToString();
        }

        /// <summary>
        /// Call from UI button to simulate tapping an object at the given index.
        /// In real UI, each object would have its own button.
        /// For stub UI, this can be called from a debug button or test.
        /// </summary>
        public void TapObject(int index) => OnObjectTapped?.Invoke(index);
    }
}
