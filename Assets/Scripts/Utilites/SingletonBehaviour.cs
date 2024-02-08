using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; protected set; }
    protected bool dontDestroyOnload;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = (T)this;
            if (dontDestroyOnload)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}