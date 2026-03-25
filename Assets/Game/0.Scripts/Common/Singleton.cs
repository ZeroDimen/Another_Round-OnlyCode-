using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            Debug.Log($"[Singleton<{typeof(T).Name}>] Marked as DontDestroyOnLoad (scene: {gameObject.scene.name})");
            DontDestroyOnLoad(gameObject);
            // 씬 전환시 호출되는 액션 메서드 할당
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);
    protected abstract void OnSceneUnloaded(Scene scene);
}