using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerSaveDataSystem : SystemBase
{
    private bool loadSuccess;
    //private LocalSaveManager localSaveManager;
    private HealthData _healthData;
    private CollectibleData _collectibleData;

    protected override void OnStartRunning()
    {
        loadSuccess = false;
        //localSaveManager = LocalSaveManager.Instance;
        _healthData = new HealthData();
        _collectibleData = new CollectibleData();

    }
    protected override void OnStopRunning()
    {
    }

    protected override void OnUpdate()
    {
        if (!loadSuccess)
        {
            if (LocalSaveManager.Instance.Active)
            {
                foreach ((RefRW<CollectibleData> collectibleData, RefRW<HealthData> healthData)
                in SystemAPI.Query<RefRW<CollectibleData>, RefRW<HealthData>>().WithAll<PlayerTag>())
                {
                    collectibleData.ValueRW.CoinsCollected = LocalSaveManager.Instance.SaveData.CoinsCollected;
                    healthData.ValueRW.CurrentHealth = LocalSaveManager.Instance.SaveData.CurrentHealth;
                    loadSuccess = true;
                }
                if (loadSuccess)
                {
                    LocalSaveManager.Instance.OnDestroyEvent += OnLocalSaveManagerDestroyEvent;
                }
            }
        }
        else
        {
            foreach ((RefRO<CollectibleData> collectibleData, RefRO<HealthData> healthData)
            in SystemAPI.Query<RefRO<CollectibleData>, RefRO<HealthData>>().WithAll<PlayerTag>())
            {
                _healthData.CurrentHealth = healthData.ValueRO.CurrentHealth;
                _collectibleData.CoinsCollected = collectibleData.ValueRO.CoinsCollected;
            }
        }
    }

    private void OnLocalSaveManagerDestroyEvent()
    {
        LocalSaveManager.Instance.SetSaveData(_collectibleData.CoinsCollected, _healthData.CurrentHealth);
    }
}