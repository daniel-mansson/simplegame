using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

/// <summary>
/// Post-build script that injects <c>NSUserTrackingUsageDescription</c> into the
/// Xcode project's <c>Info.plist</c> after an iOS build.
///
/// Required by Apple for any app that calls ATT (App Tracking Transparency).
/// Builds without this key are rejected by the App Store if the app requests tracking.
///
/// This script runs automatically as part of the Unity iOS build pipeline.
/// No manual Xcode editing is required.
/// </summary>
public static class PostBuildATT
{
    private const string TrackingUsageDescription =
        "We use tracking to serve you personalized ads and measure app performance. " +
        "Your privacy choices are respected.";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
#if UNITY_IOS
        if (buildTarget != BuildTarget.iOS) return;

        var plistPath = Path.Combine(buildPath, "Info.plist");
        if (!File.Exists(plistPath))
        {
            Debug.LogWarning($"[PostBuildATT] Info.plist not found at: {plistPath}");
            return;
        }

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        var root = plist.root;
        const string key = "NSUserTrackingUsageDescription";

        if (!root.values.ContainsKey(key))
        {
            root.SetString(key, TrackingUsageDescription);
            plist.WriteToFile(plistPath);
            Debug.Log($"[PostBuildATT] Injected {key} into Info.plist: \"{TrackingUsageDescription}\"");
        }
        else
        {
            Debug.Log($"[PostBuildATT] {key} already present in Info.plist — skipping injection.");
        }
#endif
    }
}
