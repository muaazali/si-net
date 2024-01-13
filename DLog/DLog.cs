using System;
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

public class DLog
{
    public static bool showDebugLogs = false;

    public static void Log(string message)
    {
        if (!showDebugLogs) return;
        Console.WriteLine(message);
        #if UNITY_2017_1_OR_NEWER
        Debug.Log(message);
        #endif
    }

    public static void LogError(string message)
    {
        Console.Error.WriteLine(message);
        #if UNITY_2017_1_OR_NEWER
        Debug.LogError(message);
        #endif
    }
}