using SimpleGame.Game.Services;
using UnityEditor;
using UnityEngine;

namespace SimpleGame.Editor
{
    /// <summary>
    /// Creates the two IAP ScriptableObject assets needed for M019 if they do not exist.
    ///
    /// Run via: Unity menu Tools/Setup/Create IAP Assets
    ///
    /// Creates:
    ///   Assets/Resources/IAPMockConfig.asset    — default outcome: Success, 500 coins
    ///   Assets/Resources/IAPProductCatalog.asset — three coin packs (500 / 1200 / 2500)
    ///
    /// Product IDs use placeholder values. Update them to match the real store listings
    /// and PlayFab catalog ItemIds before submitting to the store.
    /// </summary>
    public static class CreateIAPAssets
    {
        private const string ResourcesDir = "Assets/Resources";
        private const string MockConfigPath = ResourcesDir + "/IAPMockConfig.asset";
        private const string CatalogPath = ResourcesDir + "/IAPProductCatalog.asset";

        [MenuItem("Tools/Setup/Create IAP Assets")]
        public static void Create()
        {
            EnsureDirectory(ResourcesDir);
            CreateMockConfig();
            CreateProductCatalog();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateIAPAssets] IAP assets created/verified at Assets/Resources/.");
        }

        private static void CreateMockConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<IAPMockConfig>(MockConfigPath) != null)
            {
                Debug.Log("[CreateIAPAssets] IAPMockConfig already exists — skipping.");
                return;
            }

            var config = ScriptableObject.CreateInstance<IAPMockConfig>();
            config.MockOutcome = IAPOutcome.Success;
            config.CoinsGranted = 500;
            AssetDatabase.CreateAsset(config, MockConfigPath);
            Debug.Log($"[CreateIAPAssets] Created {MockConfigPath}");
        }

        private static void CreateProductCatalog()
        {
            if (AssetDatabase.LoadAssetAtPath<IAPProductCatalog>(CatalogPath) != null)
            {
                Debug.Log("[CreateIAPAssets] IAPProductCatalog already exists — skipping.");
                return;
            }

            var catalog = ScriptableObject.CreateInstance<IAPProductCatalog>();

            // Placeholder product IDs — update to match App Store / Google Play / PlayFab catalog.
            // Convention: com.<bundleid>.coins.<amount>
            catalog.Products = new IAPProductDefinition[]
            {
                new IAPProductDefinition
                {
                    ProductId   = "com.simplegame.coins.500",
                    CoinsAmount = 500,
                    DisplayName = "500 Coins",
                },
                new IAPProductDefinition
                {
                    ProductId   = "com.simplegame.coins.1200",
                    CoinsAmount = 1200,
                    DisplayName = "1200 Coins",
                },
                new IAPProductDefinition
                {
                    ProductId   = "com.simplegame.coins.2500",
                    CoinsAmount = 2500,
                    DisplayName = "2500 Coins",
                },
            };

            AssetDatabase.CreateAsset(catalog, CatalogPath);
            Debug.Log($"[CreateIAPAssets] Created {CatalogPath}");
        }

        private static void EnsureDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
    }
}
