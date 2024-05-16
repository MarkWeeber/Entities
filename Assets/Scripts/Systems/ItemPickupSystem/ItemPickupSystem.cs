using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct ItemPickupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        var job = new ItemPickUpJob
        {
            EntityManager = state.EntityManager
        };
        var jobHandle = job.Schedule(simulation, state.Dependency);
        jobHandle.Complete();
    }

    private partial struct ItemPickUpJob : ITriggerEventsJob
    {
        public EntityManager EntityManager;
        public EntityCommandBuffer ECB;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity playerEntity = Entity.Null;
            Entity itemEntity = Entity.Null;
            if (EntityManager.HasComponent<PlayerTag>(triggerEvent.EntityA))
            {
                playerEntity = triggerEvent.EntityA;
            }
            if (EntityManager.HasComponent<ItemData>(triggerEvent.EntityA))
            {
                itemEntity = triggerEvent.EntityA;
            }
            if (EntityManager.HasComponent<PlayerTag>(triggerEvent.EntityB))
            {
                playerEntity = triggerEvent.EntityB;
            }
            if (EntityManager.HasComponent<ItemData>(triggerEvent.EntityB))
            {
                itemEntity = triggerEvent.EntityB;
            }
            if (itemEntity != Entity.Null && playerEntity != Entity.Null)
            {
                var itemData = EntityManager.GetComponentObject<ItemData>(itemEntity);
                itemData.item.PickUp();
            }
        }
    }
}