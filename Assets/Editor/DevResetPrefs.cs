using UnityEditor;
using UnityEngine;

/// <summary>
/// Dev utility — clears first-launch PlayerPrefs flags so consent popup
/// and platform link prompt show again on next Play. No confirmation dialog.
/// Editor-only, never ships.
/// </summary>
public static class DevResetPrefs
{
    [MenuItem("Tools/Dev/Clear First-Launch Flags")]
    public static void ClearFirstLaunchFlags()
    {
        PlayerPrefs.DeleteKey("ConsentGate_Accepted");
        PlayerPrefs.DeleteKey("PlatformLink_HasSeen");
        PlayerPrefs.Save();
        Debug.Log("[DevResetPrefs] Cleared: ConsentGate_Accepted, PlatformLink_HasSeen.");
    }
}
