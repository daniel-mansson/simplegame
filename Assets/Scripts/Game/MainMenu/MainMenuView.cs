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
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _shopBackButton;
        [SerializeField] private Text _environmentNameText;
        [SerializeField] private Text _balanceText;
        [SerializeField] private Text _levelDisplayText;
        [SerializeField] private RectTransform _objectsContainer;

        public event Action OnSettingsClicked;
        public event Action OnPlayClicked;
        public event Action OnResetProgressClicked;
        public event Action OnNextEnvironmentClicked;
        public event Action OnShopClicked;
        public event Action OnShopBackClicked;
        public event Action OnDebugRewardedClicked;
        public event Action OnDebugInterstitialClicked;
        public event Action OnDebugBannerClicked;
        public event Action<int> OnObjectTapped;

        private readonly List<GameObject> _objectButtons = new List<GameObject>();
        private GameObject _debugPanel;
        private Text _debugStatusText;

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            _resetProgressButton.onClick.AddListener(() => OnResetProgressClicked?.Invoke());
            if (_nextEnvironmentButton != null)
                _nextEnvironmentButton.onClick.AddListener(() => OnNextEnvironmentClicked?.Invoke());
            if (_shopButton != null)
                _shopButton.onClick.AddListener(() => OnShopClicked?.Invoke());
            if (_shopBackButton != null)
                _shopBackButton.onClick.AddListener(() => OnShopBackClicked?.Invoke());

            CreateDebugAdsPanel();
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

        public void SetDebugAdsVisible(bool visible)
        {
            if (_debugPanel != null)
                _debugPanel.SetActive(visible);
        }

        public void UpdateDebugStatus(string text)
        {
            if (_debugStatusText != null)
                _debugStatusText.text = text;
        }

        private void CreateDebugAdsPanel()
        {
            // Build a small panel anchored to the bottom-right with 3 debug ad buttons + status label.
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            _debugPanel = new GameObject("DebugAdsPanel", typeof(RectTransform));
            _debugPanel.transform.SetParent(canvas.transform, false);

            var panelRect = _debugPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(1, 0);
            panelRect.anchoredPosition = new Vector2(-20, 20);
            panelRect.sizeDelta = new Vector2(660, 560);

            var panelImage = _debugPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            var layout = _debugPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Title
            CreateDebugLabel(_debugPanel.transform, "— Debug Ads —", 48, Color.yellow);

            // Buttons
            CreateDebugButton(_debugPanel.transform, "Rewarded Ad", new Color(0.2f, 0.6f, 0.2f),
                () => OnDebugRewardedClicked?.Invoke());
            CreateDebugButton(_debugPanel.transform, "Interstitial Ad", new Color(0.6f, 0.4f, 0.1f),
                () => OnDebugInterstitialClicked?.Invoke());
            CreateDebugButton(_debugPanel.transform, "Banner Ad", new Color(0.3f, 0.3f, 0.7f),
                () => OnDebugBannerClicked?.Invoke());

            // Status label
            var statusGO = CreateDebugLabel(_debugPanel.transform, "", 36, Color.white);
            _debugStatusText = statusGO.GetComponent<Text>();

            // Hidden by default — shown when IAdService is available
            _debugPanel.SetActive(false);
        }

        private void CreateDebugButton(Transform parent, string label, Color color, Action onClick)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 110;

            var image = go.AddComponent<Image>();
            image.color = color;

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

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
            text.fontSize = 48;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private GameObject CreateDebugLabel(Transform parent, string content, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 60;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = fontSize;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return go;
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
