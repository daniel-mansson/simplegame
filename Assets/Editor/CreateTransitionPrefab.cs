using SimpleGame.Core.Unity.TransitionManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Editor
{
    /// <summary>
    /// Editor utility to create the TransitionOverlay prefab asset programmatically.
    /// Run via: Unity batchmode -executeMethod SimpleGame.Editor.CreateTransitionPrefab.Create
    /// </summary>
    public static class CreateTransitionPrefab
    {
        [MenuItem("Tools/Setup/Create Transition Prefab")]
        public static void Create()
        {
            const string prefabPath = "Assets/Prefabs/TransitionOverlay.prefab";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Build the GameObject hierarchy
            var root = new GameObject("TransitionOverlay");

            // Canvas — high sort order, overlay render mode
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            // CanvasScaler for consistent sizing
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // GraphicRaycaster (required for Canvas but we don't use it for input)
            root.AddComponent<GraphicRaycaster>();

            // CanvasGroup — starts transparent, no raycasts
            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Full-screen black Image
            var image = root.AddComponent<Image>();
            image.color = Color.black;
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;

            // UnityTransitionPlayer component — wired to the CanvasGroup
            var transitionPlayer = root.AddComponent<UnityTransitionPlayer>();
            WireSerializedField(transitionPlayer, "_canvasGroup", canvasGroup);

            // Start inactive (transition is not in progress at boot)
            root.SetActive(false);

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[CreateTransitionPrefab] Prefab saved to {prefabPath}");
        }

        private static void WireSerializedField(Component component, string fieldName, Object target)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = target;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogError($"[CreateTransitionPrefab] Could not find serialized field '{fieldName}' on {component.GetType().Name}");
            }
        }
    }
}
