using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Zenject;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial class ItemPickupSystemBase : SystemBase
{
    private InventoryManager inventoryManager;
    private bool injected;
    private bool playerInjected;
    [Inject]
    private void Init(InventoryManager inventoryManager)
    {
        inventoryManager.EntityManager = EntityManager;
        this.inventoryManager = inventoryManager;
        injected = true;

    }
    protected override void OnUpdate()
    {
        if (!injected)
        {
            return;
        }
        else if (!playerInjected)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entities = SystemAPI.QueryBuilder().WithAll<PlayerTag>().Build();
            if (entities.CalculateEntityCount() > 0)
            {
                ecb.AddComponentObject(entities, new PlayerInventoryData
                {
                    InventoryManager = inventoryManager
                });
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
            playerInjected = true;
        }
        else
        {
            SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var items = new NativeList<Entity>(Allocator.TempJob);
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var job = new ItemPickUpJob
            {
                EntityManager = EntityManager,
                ECB = ecb,
                Items = items,
            };
            var jobHandle = job.Schedule(simulation, Dependency);
            jobHandle.Complete();
            foreach (var itemEntity in items)
            {
                var itemData = EntityManager.GetComponentObject<ItemData>(itemEntity);
                var item = itemData.Item;
                foreach (var itemAction in item.ItemActions)
                {
                    itemAction.EntityManager = EntityManager;
                    itemAction.Entity = itemData.TargetEntity;
                }
                inventoryManager.TryAddItem(itemData.Item);
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
            items.Dispose();
        }
    }

    private partial struct ItemPickUpJob : ITriggerEventsJob
    {
        public EntityManager EntityManager;
        public EntityCommandBuffer ECB;
        public NativeList<Entity> Items;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity playerEntity = Entity.Null;
            Entity itemEntity = Entity.Null;
            if (EntityManager.HasComponent<PlayerInventoryData>(triggerEvent.EntityA))
            {
                playerEntity = triggerEvent.EntityA;
            }
            if (EntityManager.HasComponent<ItemData>(triggerEvent.EntityA))
            {
                itemEntity = triggerEvent.EntityA;
            }
            if (EntityManager.HasComponent<PlayerInventoryData>(triggerEvent.EntityB))
            {
                playerEntity = triggerEvent.EntityB;
            }
            if (EntityManager.HasComponent<ItemData>(triggerEvent.EntityB))
            {
                itemEntity = triggerEvent.EntityB;
            }
            if (itemEntity != Entity.Null && playerEntity != Entity.Null)
            {
                var itemData = EntityManager.GetComponentData<ItemData>(itemEntity);
                itemData.TargetEntity = playerEntity;
                Items.Add(itemEntity);
                ECB.DestroyEntity(itemEntity);
            }
        }
    }
}