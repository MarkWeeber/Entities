using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial struct HealthDamageAreaSystem : ISystem
{
    ComponentLookup<HealthData> healthDataLookup;
    ComponentLookup<AreaDamagerData> areaDamagerLookup;
    private float timer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        timer = 0f;
        state.RequireForUpdate<HealthData>();
        state.RequireForUpdate<AreaDamagerData>();
        healthDataLookup = state.GetComponentLookup<HealthData>(false);
        areaDamagerLookup = state.GetComponentLookup<AreaDamagerData>(false);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        if (SystemAPI.TryGetSingleton<SystemControllerData>(out SystemControllerData systemControllerData))
        {
            if(!systemControllerData.AreaEffector)
            {
                return;
            }
            else if(systemControllerData.AreaEffectorRate > 0f)
            {
                if (timer < 0f)
                {
                    timer = systemControllerData.AreaEffectorRate;
                }
                else
                {
                    timer -= deltaTime;
                    return;
                }
            }
        }
        else
        {
            return;
        }
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        healthDataLookup.Update(ref state);
        areaDamagerLookup.Update(ref state);
        HealthDamageEvents healthDamageEvents = new HealthDamageEvents
        {
            AreaDamagerLookup = areaDamagerLookup,
            HealthLookup = healthDataLookup,
            Deltatime = deltaTime
        };
        JobHandle jobHandle = healthDamageEvents.Schedule(simulation, state.Dependency);
        jobHandle.Complete();
    }

    [BurstCompile]
    public partial struct HealthDamageEvents : ITriggerEventsJob
    {
        public ComponentLookup<AreaDamagerData> AreaDamagerLookup;
        public ComponentLookup<HealthData> HealthLookup;
        public float Deltatime;
        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent)
        {
            RefRW<HealthData> heathRW;
            RefRW<AreaDamagerData> areaDamagerRW;
            if (HealthLookup.HasComponent(triggerEvent.EntityA))
            {
                if (!HealthLookup.IsComponentEnabled(triggerEvent.EntityA))
                {
                    return;
                }
                if (AreaDamagerLookup.HasComponent(triggerEvent.EntityB))
                {
                    heathRW = HealthLookup.GetRefRW(triggerEvent.EntityA);
                    areaDamagerRW = AreaDamagerLookup.GetRefRW(triggerEvent.EntityB);
                    DealDamage(heathRW, areaDamagerRW, triggerEvent.EntityA, Deltatime);
                }
            }
            else if (HealthLookup.HasComponent(triggerEvent.EntityB))
            {
                if (!HealthLookup.IsComponentEnabled(triggerEvent.EntityB))
                {
                    return;
                }
                if (AreaDamagerLookup.HasComponent(triggerEvent.EntityA))
                {
                    heathRW = HealthLookup.GetRefRW(triggerEvent.EntityB);
                    areaDamagerRW = AreaDamagerLookup.GetRefRW(triggerEvent.EntityA);
                    DealDamage(heathRW, areaDamagerRW, triggerEvent.EntityB, Deltatime);
                }
            }
        }

        private void DealDamage(RefRW<HealthData> healthData, RefRW<AreaDamagerData> areaDamagerData, Entity healthEntity, float deltaTime)
        {
            if (areaDamagerData.ValueRO.DamageTimer > 0f)
            {
                areaDamagerData.ValueRW.DamageTimer -= deltaTime;
            }
            else if (areaDamagerData.ValueRO.DamageTimer <= 0f)
            {
                healthData.ValueRW.CurrentHealth -= areaDamagerData.ValueRO.DamageValue;
                if (healthData.ValueRO.CurrentHealth <= 0f)
                {
                    HealthLookup.SetComponentEnabled(healthEntity, false);
                }
                areaDamagerData.ValueRW.DamageTimer = areaDamagerData.ValueRO.DamageTime;
            }
        }
    }
}