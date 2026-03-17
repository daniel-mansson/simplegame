using SimpleGame.Game.Meta;
using UnityEditor;
using UnityEngine;

namespace SimpleGame.Editor
{
    /// <summary>
    /// Editor utility to create test meta world data assets.
    /// Creates 4 environments with varied objects and blocked-by
    /// relationships for testing progression logic.
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

            // --- 1. Garden: 3 objects ---
            var fountain = CreateObject("Fountain", totalSteps: 3, costPerStep: 1, blockedBy: null);
            var bench = CreateObject("Bench", totalSteps: 2, costPerStep: 1, blockedBy: null);
            var gazebo = CreateObject("Gazebo", totalSteps: 4, costPerStep: 2, blockedBy: new[] { fountain });
            var garden = CreateEnvironment("Garden", new[] { fountain, bench, gazebo });

            // --- 2. House: 3 objects ---
            var mailbox = CreateObject("Mailbox", totalSteps: 2, costPerStep: 1, blockedBy: null);
            var frontDoor = CreateObject("FrontDoor", totalSteps: 3, costPerStep: 2, blockedBy: null);
            var rooftop = CreateObject("Rooftop", totalSteps: 5, costPerStep: 3, blockedBy: new[] { frontDoor });
            var house = CreateEnvironment("House", new[] { mailbox, frontDoor, rooftop });

            // --- 3. Town Square: 3 objects ---
            var clockTower = CreateObject("ClockTower", totalSteps: 5, costPerStep: 2, blockedBy: null);
            var statue = CreateObject("Statue", totalSteps: 3, costPerStep: 3, blockedBy: new[] { clockTower });
            var bandstand = CreateObject("Bandstand", totalSteps: 4, costPerStep: 2, blockedBy: null);
            var townSquare = CreateEnvironment("TownSquare", new[] { clockTower, statue, bandstand });

            // --- 4. Harbor: 3 objects ---
            var lighthouse = CreateObject("Lighthouse", totalSteps: 6, costPerStep: 3, blockedBy: null);
            var fishingBoat = CreateObject("FishingBoat", totalSteps: 4, costPerStep: 2, blockedBy: null);
            var pier = CreateObject("Pier", totalSteps: 5, costPerStep: 4, blockedBy: new[] { lighthouse, fishingBoat });
            var harbor = CreateEnvironment("Harbor", new[] { lighthouse, fishingBoat, pier });

            // --- World Data ---
            var worldData = ScriptableObject.CreateInstance<WorldData>();
            worldData.environments = new[] { garden, house, townSquare, harbor };
            AssetDatabase.CreateAsset(worldData, $"{DataDir}/WorldData.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateTestWorldData] Test world data created: Garden, House, Town Square, Harbor");
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
