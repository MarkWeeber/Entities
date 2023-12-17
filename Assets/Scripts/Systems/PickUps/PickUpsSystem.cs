using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
public partial struct PickUpsSystem : ISystem
{
    private ComponentLookup<HealthData> healthLookup;
    private ComponentLookup<HealthPickupData> healthDataLookup;
    private ComponentLookup<AbilityPickUpData> abilityDataLookup;
    private ComponentLookup<PlayerTag> playerTagLookup;
    private ComponentLookup<FireAbilityData> fireAbilityDataLookup;
    private ComponentLookup<CoinPickUpData> coinPickUpDataLookup;
    private ComponentLookup<CollectibleData> collectibleDataLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        healthLookup = state.GetComponentLookup<HealthData>(false);
        healthDataLookup = state.GetComponentLookup<HealthPickupData>(true);
        abilityDataLookup = state.GetComponentLookup<AbilityPickUpData>(true);
        playerTagLookup = state.GetComponentLookup<PlayerTag>(true);
        fireAbilityDataLookup = state.GetComponentLookup<FireAbilityData>(false);
        coinPickUpDataLookup = state.GetComponentLookup<CoinPickUpData>(true);
        collectibleDataLookup = state.GetComponentLookup<CollectibleData>(false);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        healthLookup.Update(ref state);
        healthDataLookup.Update(ref state);
        playerTagLookup.Update(ref state);
        abilityDataLookup.Update(ref state);
        fireAbilityDataLookup.Update(ref state);
        coinPickUpDataLookup.Update(ref state);
        collectibleDataLookup.Update(ref state);
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        PickUpJob pickUpJob = new PickUpJob
        {
            HealthLookup = healthLookup,
            HealthDataLookup = healthDataLookup,
            AbilityDataLookup = abilityDataLookup,
            PlayerTagLookup = playerTagLookup,
            FireAbilityDataLookup = fireAbilityDataLookup,
            CoinPickupDataLookup = coinPickUpDataLookup,
            CollectibleDataLookup = collectibleDataLookup,
            EntityCommandBuffer = entityCommandBuffer
        };
        JobHandle jobHandle = pickUpJob.Schedule(simulation, state.Dependency);
        jobHandle.Complete();
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    [BurstCompile]
    private partial struct PickUpJob : ITriggerEventsJob
    {
        public ComponentLookup<HealthData> HealthLookup;
        [ReadOnly] public ComponentLookup<HealthPickupData> HealthDataLookup;
        [ReadOnly] public ComponentLookup<AbilityPickUpData> AbilityDataLookup;
        [ReadOnly] public ComponentLookup<PlayerTag> PlayerTagLookup;
        public ComponentLookup<FireAbilityData> FireAbilityDataLookup;
        [ReadOnly] public ComponentLookup<CoinPickUpData> CoinPickupDataLookup;
        public ComponentLookup<CollectibleData> CollectibleDataLookup;
        public EntityCommandBuffer EntityCommandBuffer;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity playerEntity = Entity.Null;
            Entity healthPickUpEntity = Entity.Null;
            Entity abilityPickUpEntity = Entity.Null;
            Entity coinPickUpEntity = Entity.Null;
            // getting necessary entities
            if (PlayerTagLookup.HasComponent(triggerEvent.EntityA))
            {
                playerEntity = triggerEvent.EntityA;
            }
            if (PlayerTagLookup.HasComponent(triggerEvent.EntityB))
            {
                playerEntity = triggerEvent.EntityB;
            }
            if (HealthDataLookup.HasComponent(triggerEvent.EntityA))
            {
                healthPickUpEntity = triggerEvent.EntityA;
            }
            if (HealthDataLookup.HasComponent(triggerEvent.EntityB))
            {
                healthPickUpEntity = triggerEvent.EntityB;
            }
            if (AbilityDataLookup.HasComponent(triggerEvent.EntityA))
            {
                abilityPickUpEntity = triggerEvent.EntityA;
            }
            if (AbilityDataLookup.HasComponent(triggerEvent.EntityB))
            {
                abilityPickUpEntity = triggerEvent.EntityB;
            }
            if (CoinPickupDataLookup.HasComponent(triggerEvent.EntityA))
            {
                coinPickUpEntity = triggerEvent.EntityA;
            }
            if (CoinPickupDataLookup.HasComponent(triggerEvent.EntityB))
            {
                coinPickUpEntity = triggerEvent.EntityB;
            }
            // healing pickup events
            if (playerEntity != Entity.Null && healthPickUpEntity != Entity.Null)
            {
                if (HealthLookup.HasComponent(playerEntity) && HealthLookup.IsComponentEnabled(playerEntity))
                {
                    RefRW<HealthData> healthData = HealthLookup.GetRefRW(playerEntity);
                    RefRO<HealthPickupData> healthPickUpData = HealthDataLookup.GetRefRO(healthPickUpEntity);
                    float healAmmount = math.clamp
                        (
                            healthData.ValueRO.MaxHealth - healthData.ValueRO.CurrentHealth,
                            0f,
                            healthPickUpData.ValueRO.HealAmmount
                        );
                    if (healAmmount > 0f)
                    {
                        healthData.ValueRW.CurrentHealth += healAmmount;
                        EntityCommandBuffer.DestroyEntity(healthPickUpEntity);
                    }
                }
            }
            // ability pickup events
            if (playerEntity != Entity.Null && abilityPickUpEntity != Entity.Null)
            {
                if (FireAbilityDataLookup.HasComponent(playerEntity))
                {
                    RefRW<FireAbilityData> fireAbilityData = FireAbilityDataLookup.GetRefRW(playerEntity);
                    fireAbilityData.ValueRW.SpecialFireSwitch = true;
                    EntityCommandBuffer.DestroyEntity(abilityPickUpEntity);
                }
            }
            // coins pickup events
            if (playerEntity != Entity.Null && coinPickUpEntity != Entity.Null)
            {
                if (CollectibleDataLookup.HasComponent(playerEntity))
                {
                    RefRW<CollectibleData> collectibleData = CollectibleDataLookup.GetRefRW(playerEntity);
                    RefRO<CoinPickUpData> coinPickUpData = CoinPickupDataLookup.GetRefRO(coinPickUpEntity);
                    collectibleData.ValueRW.CoinsCollected += coinPickUpData.ValueRO.Value;
                    EntityCommandBuffer.DestroyEntity(coinPickUpEntity);
                }
            }
        }
    }
}