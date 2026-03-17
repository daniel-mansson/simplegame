using SimpleGame.Game.Meta;
using UnityEditor;
using UnityEngine;

namespace SimpleGame.Editor
{
    /// <summary>
    /// Editor utility to create test meta world data assets.
    /// Creates 2 environments (Garden, Town Square) with 5 total objects
    /// and blocked-by relationships for testing progression logic.
    ///
    /// Run via: Unity menu Tools/Setup/Create Test World Data
    /// Or batchmode: -executeMethod SimpleGame.Editor.CreateTestWorldData.Create
    /// </summary>
    public static class CreateTestWorldData
    {
        private const string DataDir = "Assets/Data";

        [MenuItem("Tools/Setup/Create Test World Data")]
        public static void Create()
        {
            EnsureDirectory(DataDir);

            // --- Garden environment: 3 objects ---
            // Fountain: no blockers, 3 steps, 1 gold each
            var fountain = CreateObject("Fountain", totalSteps: 3, costPerStep: 1, blockedBy: null);

            // Bench: no blockers, 2 steps, 1 gold each
            var bench = CreateObject("Bench", totalSteps: 2, costPerStep: 1, blockedBy: null);

            // Gazebo: blocked by Fountain, 4 steps, 2 gold each
            var gazebo = CreateObject("Gazebo", totalSteps: 4, costPerStep: 2, blockedBy: new[] { fountain });

            var garden = CreateEnvironment("Garden", new[] { fountain, bench, gazebo });

            // --- Town Square environment: 2 objects ---
            // Clock Tower: no blockers, 5 steps, 2 gold each
            var clockTower = CreateObject("ClockTower", totalSteps: 5, costPerStep: 2, blockedBy: null);

            // Statue: blocked by Clock Tower, 3 steps, 3 gold each
            var statue = CreateObject("Statue", totalSteps: 3, costPerStep: 3, blockedBy: new[] { clockTower });

            var townSquare = CreateEnvironment("TownSquare", new[] { clockTower, statue });

            // --- World Data ---
            var worldData = ScriptableObject.CreateInstance<WorldData>();
            worldData.environments = new[] { garden, townSquare };
            AssetDatabase.CreateAsset(worldData, $"{DataDir}/WorldData.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateTestWorldData] Test world data created in Assets/Data/");
        }

        private static RestorableObjectData CreateObject(string displayName, int totalSteps, int costPerStep,
            RestorableObjectData[] blockedBy)
        {
            var obj = ScriptableObject.CreateInstance<RestorableObjectData>();
            obj.displayName = displayName;
            obj.totalSteps = totalSteps;
            obj.costPerStep = costPerStep;
            obj.blockedBy = blockedBy ?? new RestorableObjectData[0];
            AssetDatabase.CreateAsset(obj, $"{DataDir}/{displayName}.asset");
            return obj;
        }

        private static EnvironmentData CreateEnvironment(string envName, RestorableObjectData[] objects)
        {
            var env = ScriptableObject.CreateInstance<EnvironmentData>();
            env.environmentName = envName;
            env.objects = objects;
            AssetDatabase.CreateAsset(env, $"{DataDir}/{envName}.asset");
            return env;
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
