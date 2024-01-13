using System;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerSaveDataSystem : SystemBase, SaveDataSender
{
    private bool localLoadSuccess;
    private bool reverseSet;
    private LocalSaveManager localSaveManager;
    private HealthData _healthData;
    private CollectibleData _collectibleData;

    protected override void OnStartRunning()
    {
        localLoadSuccess = false;
        localSaveManager = LocalSaveManager.Instance;
        _healthData = new HealthData();
        _collectibleData = new CollectibleData();

    }
    protected override void OnStopRunning()
    {
    }

    protected override void OnUpdate()
    {
        if (!localLoadSuccess)
        {
            if (localSaveManager != null && localSaveManager.SaveDataPersists)
            {
                foreach ((RefRW<CollectibleData> collectibleData, RefRW<HealthData> healthData)
                in SystemAPI.Query<RefRW<CollectibleData>, RefRW<HealthData>>().WithAll<PlayerTag>())
                {
                    collectibleData.ValueRW.CoinsCollected = localSaveManager.SaveData.CoinsCollected;
                    healthData.ValueRW.CurrentHealth = localSaveManager.SaveData.CurrentHealth;
                    localLoadSuccess = true;
                }
                if (localLoadSuccess)
                {
                    localSaveManager.OnSaveDataSetEvent += OnSaveDataSetEvent;
                    localSaveManager.OnSystemBaseUpdate += OnSystemBaseUpdate;
                }
            }
        }
        else
        {
            if (reverseSet)
            {
                foreach ((RefRW<CollectibleData> collectibleData, RefRW<HealthData> healthData)
                in SystemAPI.Query<RefRW<CollectibleData>, RefRW<HealthData>>().WithAll<PlayerTag>())
                {
                    collectibleData.ValueRW.CoinsCollected = _collectibleData.CoinsCollected;
                    healthData.ValueRW.CurrentHealth = _healthData.CurrentHealth;
                }
                reverseSet = false;
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
    }

    private void OnSystemBaseUpdate()
    {
        localSaveManager.SetSaveData(this, new SaveData { CoinsCollected = _collectibleData.CoinsCollected, CurrentHealth = _healthData.CurrentHealth});
    }

    private void OnSaveDataSetEvent(SaveDataSender sender, SaveData saveData)
    {
        if (sender != this)
        {
            reverseSet = true;
            _healthData.CurrentHealth = saveData.CurrentHealth;
            _collectibleData.CoinsCollected = saveData.CoinsCollected;
        }
    }
}