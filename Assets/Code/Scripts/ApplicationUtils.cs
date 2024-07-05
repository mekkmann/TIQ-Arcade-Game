using System;
using System.Collections.Generic;
using UnityEngine;

public static class ApplicationUtils
{
    private static readonly HashSet<Type> s_ranOnApplicationStart = new();
    public static bool IsQuitting { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void SetupApplicationQuitting()
    {
        s_ranOnApplicationStart.Clear();
        IsQuitting = false;

        // This is needed to avoid the event being added multiple times when
        // domain reload is disabled on Editor
        Application.quitting -= OnApplicationQuit;
        Application.quitting += OnApplicationQuit;
    }

    public static void RunOnceDuringTheWholeGameLifetime<T>(Action action)
    {
        if (s_ranOnApplicationStart.Contains(typeof(T)))
        {
            return;
        }

        action();
        s_ranOnApplicationStart.Add(typeof(T));
    }

    private static void OnApplicationQuit()
    {
        IsQuitting = true;
    }
}
