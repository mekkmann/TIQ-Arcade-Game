using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransientSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T s_instance;
    private static readonly object s_lock = new();

    public static bool InstanceExists
    {
        get
        {
            if (ApplicationUtils.IsQuitting)
            {
                return false;
            }

            return s_instance != null;
        }
    }

    public static T Instance
    {
        get
        {
            // Needed to reset instance when domain reload is disabled on Editor
            ApplicationUtils.RunOnceDuringTheWholeGameLifetime<T>(() => { s_instance = null; });

            if (ApplicationUtils.IsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                    "' already destroyed on application quit. Won't create again.");
                return null;
            }

            lock (s_lock)
            {
                if (s_instance != null)
                {
                    return s_instance;
                }

                return CreateAndSetInstance();
            }
        }
    }

    protected virtual void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private static T CreateAndSetInstance()
    {
        var instances = FindValidInstancesAndDestroyTheOthers().ToList();

        if (instances.Count() > 1)
        {
            string objectNames = string.Join(", ", instances.Select(instance => instance.gameObject.name));
            Debug.LogError(
                $"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopenning the scene might fix it. [type:{typeof(T)}] [names:{objectNames}]");
            return s_instance;
        }

        if (instances.Any())
        {
            s_instance = instances.ElementAt(0);
        }

        if (s_instance == null)
        {
            GameObject singleton = new($"(singleton) {typeof(T)}");
            s_instance = singleton.AddComponent<T>();
            Debug.Log(
                $"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singleton}' was created with DontDestroyOnLoad.");
        }
        else
        {
            Debug.Log($"[Singleton] Using instance already created: {s_instance.gameObject.name}");
        }

        return s_instance;
    }

    protected static IEnumerable<T> FindValidInstancesAndDestroyTheOthers()
    {
        T[] allInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
        if (allInstances.Length == 0)
        {
            return Enumerable.Empty<T>();
        }

        List<T> validInstances = new();
        List<T> instancesToCleanup = new();

        foreach (T instance in allInstances)
        {
            bool wasAlreadyDestroyed = instance == null;
            if (wasAlreadyDestroyed)
            {
                continue;
            }

            bool isFromAnUnloadedScene = !instance.gameObject.scene.IsValid();
            if (isFromAnUnloadedScene)
            {
                instancesToCleanup.Add(instance);
            }
            else
            {
                validInstances.Add(instance);
            }
        }


        if (instancesToCleanup.Count > 0)
        {
            string objectNames = string.Join(", ", allInstances.Select(instance => instance.gameObject.name));
            Debug.Log($"[Singleton] Cleaning up {allInstances.Length} instances of {typeof(T)} - {objectNames}");

            foreach (T instance in instancesToCleanup)
            {
                Destroy(instance.gameObject);
            }
        }

        return validInstances;
    }
}