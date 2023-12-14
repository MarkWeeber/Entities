using UnityEngine;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine.Events;
using System.Collections.Generic;


public class SceneLoadChecker : MonoBehaviour
{
    [SerializeField] private List<SubScene> subScenes;

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
        foreach (SubScene subScene in subScenes)
        {
            Entity sceneEntity = SceneSystem.GetSceneEntity(unmanagedWorld, subScene.SceneGUID);
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
        if (subScenes != null && !allSceneLoaded)
        {
            CheckIfSceneLoaded();
        }
    }
}
