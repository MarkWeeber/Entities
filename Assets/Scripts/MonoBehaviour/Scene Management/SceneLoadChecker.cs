using UnityEngine;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine.Events;
using System.Collections.Generic;


public class SceneLoadChecker : MonoBehaviour
{
    [SerializeField] private List<string> scenePaths;

    public UnityEvent onScenesLoaded = new UnityEvent();
    public UnityEvent onStart = new UnityEvent();

    private bool allSceneLoaded;

    private void Start()
    {
        onStart?.Invoke();
    }
    private void CheckIfSceneLoaded()
    {
        WorldUnmanaged unmanagedWorld = World.DefaultGameObjectInjectionWorld.Unmanaged;
        bool check = true;
        foreach (string scenePath in scenePaths)
        {
            var guid = SceneSystem.GetSceneGUID(ref unmanagedWorld.GetExistingSystemState<SceneSystem>(), scenePath);
            Entity sceneEntity = SceneSystem.GetSceneEntity(unmanagedWorld, guid);
            if (!SceneSystem.IsSceneLoaded(unmanagedWorld, sceneEntity))
            {
                check = false;
                break;
            }
        }
        if (check)
        {
            allSceneLoaded = true;
            onScenesLoaded?.Invoke();
        }
    }

    private void LateUpdate()
    {
        if (scenePaths != null && !allSceneLoaded)
        {
            CheckIfSceneLoaded();
        }
    }
}
