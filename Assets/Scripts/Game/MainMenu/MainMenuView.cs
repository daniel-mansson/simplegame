using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// Unity MonoBehaviour implementation of IMainMenuView.
    /// Dynamically creates tappable buttons for each restorable object.
    /// Has zero references to presenters, services, or managers.
    /// </summary>
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _resetProgressButton;
        [SerializeField] private Button _nextEnvironmentButton;
        [SerializeField] private Text _environmentNameText;
        [SerializeField] private Text _balanceText;
        [SerializeField] private Text _levelDisplayText;
        [SerializeField] private RectTransform _objectsContainer;

        public event Action OnSettingsClicked;
        public event Action OnPlayClicked;
        public event Action OnResetProgressClicked;
        public event Action OnNextEnvironmentClicked;
        public event Action<int> OnObjectTapped;

        private readonly List<GameObject> _objectButtons = new List<GameObject>();

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            _resetProgressButton.onClick.AddListener(() => OnResetProgressClicked?.Invoke());
            if (_nextEnvironmentButton != null)
                _nextEnvironmentButton.onClick.AddListener(() => OnNextEnvironmentClicked?.Invoke());
        }

        public void UpdateEnvironmentName(string text) => _environmentNameText.text = text;
        public void UpdateBalance(string text) => _balanceText.text = text;
        public void UpdateLevelDisplay(string text) => _levelDisplayText.text = text;

        public void UpdateObjects(ObjectDisplayData[] objects)
        {
            // Clear existing buttons
            foreach (var go in _objectButtons)
                Destroy(go);
            _objectButtons.Clear();

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                var index = i; // capture for closure

                var label = obj.IsComplete ? $"{obj.Name} [DONE]"
                    : obj.IsBlocked ? $"{obj.Name} [BLOCKED]"
                    : $"{obj.Name} [{obj.Progress}] — {obj.CostPerStep}gp";

                var btnGO = CreateObjectButton(label, obj.IsComplete || obj.IsBlocked);
                btnGO.GetComponent<Button>().onClick.AddListener(() => OnObjectTapped?.Invoke(index));
                _objectButtons.Add(btnGO);
            }
        }

        /// <summary>
        /// Call from UI or test to simulate tapping an object at the given index.
        /// </summary>
        public void TapObject(int index) => OnObjectTapped?.Invoke(index);

        public void SetNextEnvironmentVisible(bool visible)
        {
            if (_nextEnvironmentButton != null)
                _nextEnvironmentButton.gameObject.SetActive(visible);
        }

        private GameObject CreateObjectButton(string label, bool disabled)
        {
            var go = new GameObject("ObjectButton", typeof(RectTransform));
            go.transform.SetParent(_objectsContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            var image = go.AddComponent<Image>();
            image.color = disabled
                ? new Color(0.2f, 0.2f, 0.2f, 0.6f)
                : new Color(0.2f, 0.5f, 0.3f, 0.9f);

            var btn = go.AddComponent<Button>();
            if (disabled)
                btn.interactable = false;

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 22;
            text.color = disabled ? Color.gray : Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return go;
        }
    }
}
