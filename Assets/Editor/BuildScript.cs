// Assets/Editor/BuildScript.cs
//
// Unity CLI build entry points for Fastlane.
//
// Invoked via:
//   Unity -quit -batchmode -nographics -projectPath <path> \
//         -buildTarget <iOS|Android> \
//         -executeMethod BuildScript.BuildIOS \
//         -outputPath <path> \
//         -logFile <path>
//
// Android additionally accepts:
//   -keystoreName <path>
//   -keystorePass <password>
//   -keyaliasName <alias>
//   -keyaliasPass <password>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SimpleGame.Editor
{
    public static class BuildScript
    {
        // -----------------------------------------------------------------------
        // BuildIOS
        // Exports a Unity iOS Xcode project to the specified output directory.
        // -----------------------------------------------------------------------
        public static void BuildIOS()
        {
            string outputPath = GetArg("-outputPath", required: true);

            string[] scenes = GetEnabledScenes();
            Debug.Log($"[BuildScript] BuildIOS: {scenes.Length} scenes → {outputPath}");

            var options = new BuildPlayerOptions
            {
                scenes         = scenes,
                locationPathName = outputPath,
                target         = BuildTarget.iOS,
                options        = BuildOptions.None
            };

            Build(options);
        }

        // -----------------------------------------------------------------------
        // BuildAndroid
        // Builds a signed .aab using the keystore credentials from CLI args.
        // -----------------------------------------------------------------------
        public static void BuildAndroid()
        {
            string outputPath    = GetArg("-outputPath", required: true);
            string keystoreName  = GetArg("-keystoreName");
            string keystorePass  = GetArg("-keystorePass");
            string keyaliasName  = GetArg("-keyaliasName");
            string keyaliasPass  = GetArg("-keyaliasPass");

            // Apply keystore settings if provided
            if (!string.IsNullOrEmpty(keystoreName))
            {
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.keystorePass = keystorePass ?? string.Empty;
                PlayerSettings.Android.keyaliasName = keyaliasName ?? string.Empty;
                PlayerSettings.Android.keyaliasPass = keyaliasPass ?? string.Empty;
                Debug.Log($"[BuildScript] Keystore: {keystoreName} alias: {keyaliasName}");
            }
            else
            {
                Debug.LogWarning("[BuildScript] No keystore args — using project PlayerSettings keystore.");
            }

            // Enable AAB output
            EditorUserBuildSettings.buildAppBundle = true;
            Debug.Log("[BuildScript] BuildAndroid: AAB mode enabled");

            string[] scenes = GetEnabledScenes();
            Debug.Log($"[BuildScript] BuildAndroid: {scenes.Length} scenes → {outputPath}");

            var options = new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = outputPath,
                target           = BuildTarget.Android,
                options          = BuildOptions.None
            };

            Build(options);
        }

        // -----------------------------------------------------------------------
        // Shared build execution
        // -----------------------------------------------------------------------
        private static void Build(BuildPlayerOptions options)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Debug.Log($"[BuildScript] Build result: {summary.result}");
            Debug.Log($"[BuildScript] Total errors: {summary.totalErrors}");
            Debug.Log($"[BuildScript] Total warnings: {summary.totalWarnings}");
            Debug.Log($"[BuildScript] Output: {summary.outputPath}");
            Debug.Log($"[BuildScript] Size: {summary.totalSize / 1024 / 1024} MB");

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildScript] Build FAILED with result: {summary.result}");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("[BuildScript] Build SUCCEEDED.");
                EditorApplication.Exit(0);
            }
        }

        // -----------------------------------------------------------------------
        // Get enabled scenes from EditorBuildSettings
        // -----------------------------------------------------------------------
        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        // -----------------------------------------------------------------------
        // Parse a named argument from Environment.GetCommandLineArgs()
        // -----------------------------------------------------------------------
        private static string GetArg(string name, bool required = false)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }

            if (required)
            {
                Debug.LogError($"[BuildScript] Missing required argument: {name}");
                EditorApplication.Exit(1);
            }

            return null;
        }
    }
}
