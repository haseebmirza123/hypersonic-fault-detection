using UnityEngine;

namespace AztechGames
{
    /// <summary>
    /// Generic Singleton class for MonoBehaviour components (Non-Persistent)
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // ✅ Fixed Unity 2023+ Deprecation Warning
                    var objs = FindObjectsByType<T>(FindObjectsSortMode.None);
                    if (objs != null && objs.Length > 0)
                    {
                        _instance = objs[0]; // Assign the first found instance
                    }

                    // ✅ Prevents unnecessary GameObject creation
                    if (_instance == null)
                    {
                        Debug.LogWarning($"⚠️ No existing instance of {typeof(T).Name} found. Ensure it exists in the scene!");
                    }
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// Singleton class that persists across scenes (Persistent Singleton)
    /// </summary>
    public class SingletonPersistent<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;

                // ✅ Ensure it only applies to root GameObjects to prevent Unity warnings
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Debug.LogWarning($"⚠️ {typeof(T).Name} is not a root GameObject. It may not persist correctly.");
                }
            }
            else
            {
                Destroy(gameObject); // Destroy duplicate instances
            }
        }
    }
}
