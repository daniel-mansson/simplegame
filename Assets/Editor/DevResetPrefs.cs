using UnityEditor;
using UnityEngine;

/// <summary>
/// Temporary dev utility to reset first-launch flags for testing.
/// Safe to leave in the project — editor-only, never ships.
/// </summary>
public static class DevResetPrefs
{
    [MenuItem("Tools/Dev/Reset First-Launch Flags")]
    public static void ResetFirstLaunchFlags()
    {
        PlayerPrefs.DeleteKey("ConsentGate_Accepted");
        PlayerPrefs.DeleteKey("PlatformLink_HasSeen");
        PlayerPrefs.Save();
        Debug.Log("[DevResetPrefs] ConsentGate_Accepted and PlatformLink_HasSeen cleared. Next Play will show consent popup.");
    }
}
