using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerSaveDataSystem : SystemBase
{
    private bool loadSuccess;
    protected override void OnCreate()
    {
        loadSuccess = false;
    }
    protected override void OnStopRunning()
    {
        SaveData saveData = LocalSaveManager.instance.SaveData;
        foreach ((RefRO<CollectibleData> collectibleData, RefRO<HealthData> healthData)
            in SystemAPI.Query<RefRO<CollectibleData>, RefRO<HealthData>>().WithAll<PlayerTag>())
        {
            saveData.CurrentHealth = healthData.ValueRO.CurrentHealth;
            saveData.CoinsCollected = collectibleData.ValueRO.CoinsCollected;
            return;
        }
    }

    protected override void OnUpdate()
    {
        if (!loadSuccess)
        {
            SaveData saveData = LocalSaveManager.instance.SaveData;
            if (saveData != null)
            {
                foreach ((RefRW<CollectibleData> collectibleData, RefRW<HealthData> healthData)
                in SystemAPI.Query<RefRW<CollectibleData>, RefRW<HealthData>>().WithAll<PlayerTag>())
                {
                    collectibleData.ValueRW.CoinsCollected = saveData.CoinsCollected;
                    healthData.ValueRW.CurrentHealth = saveData.CurrentHealth;
                }
                loadSuccess = true;
            }
        }
    }
}