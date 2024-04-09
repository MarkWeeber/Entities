using Unity.Entities;
using UnityEngine;
using Zenject;

public partial class GameSettingSystemBase : SystemBase
{
    private PlayerConfig playerConfig;
    private bool injected;

    [Inject]
    private void Init(PlayerConfig playerConfig)
    {
        this.playerConfig = playerConfig;
        injected = true;
    }
    protected override void OnCreate()
    {
        if (injected)
        {
            Debug.Log("Inject successA");
        }
        else
        {
            Debug.Log("Inject not success");
        }
    }
    protected override void OnStopRunning()
    { }

    protected override void OnUpdate()
    {
        if (injected)
        {
            Debug.Log("Inject success!");
            Enabled = false;
        }
    }
}