using UnityEngine;

/// <summary>
/// Implements logging functionality for the Tech Support
/// module, placing a prefix in front of every log. 
/// </summary>
[RequireComponent(typeof(KMNeedyModule))]
public sealed class TechSupportLog : MonoBehaviour
{
    private static TechSupportLog instance;
    private KMNeedyModule needyModule;

    // Use this for initialization
    private void Awake()
    {
        instance = this;
        needyModule = GetComponent<KMNeedyModule>();
    }

    /// <summary>
    /// Simple logging functionality with a Tech Support prefix. 
    /// </summary>
    public static void Log(string message)
    {
        Debug.LogFormat("[{0}] {1}", instance.needyModule.ModuleDisplayName, message);
    }

    /// <summary>
    /// Logging functionality using a formatted string. 
    /// </summary>
    public static void LogFormat(string format, params object[] elements)
    {
        Log(string.Format(format, elements));
    }
}
