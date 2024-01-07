using System;
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

public class DLog
{
    public static void Log(string message)
    {
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