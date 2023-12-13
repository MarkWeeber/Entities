using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerSaveDataSystem : SystemBase
{
    private bool loadSuccess;
    private LocalSaveManager saveManager;
    protected override void OnStartRunning()
    {
        loadSuccess = false;
        saveManager = LocalSaveManager.Instance;
    }
    protected override void OnStopRunning()
    {
        if (saveManager != null)
        {
            foreach ((RefRO<CollectibleData> collectibleData, RefRO<HealthData> healthData)
            in SystemAPI.Query<RefRO<CollectibleData>, RefRO<HealthData>>().WithAll<PlayerTag>())
            {
                saveManager.SaveData.CurrentHealth = healthData.ValueRO.CurrentHealth;
                saveManager.SaveData.CoinsCollected = collectibleData.ValueRO.CoinsCollected;
                Debug.Log("System On Destroy");
            }
        }
    }

    protected override void OnUpdate()
    {
        if (!loadSuccess)
        {
            if (saveManager != null)
            {
                foreach ((RefRW<CollectibleData> collectibleData, RefRW<HealthData> healthData)
                in SystemAPI.Query<RefRW<CollectibleData>, RefRW<HealthData>>().WithAll<PlayerTag>())
                {
                    collectibleData.ValueRW.CoinsCollected = saveManager.SaveData.CoinsCollected;
                    healthData.ValueRW.CurrentHealth = saveManager.SaveData.CurrentHealth;
                    loadSuccess = true;
                }
            }
            else
            {
                saveManager = LocalSaveManager.Instance;
            }
        }
    }
}