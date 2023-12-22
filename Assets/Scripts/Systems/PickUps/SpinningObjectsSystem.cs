using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SpinningObjectsSystem : ISystem
{
    private ComponentLookup<ConstantSpinningData> constantSpinningDataLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        constantSpinningDataLookup = state.GetComponentLookup<ConstantSpinningData>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        constantSpinningDataLookup.Update(ref state);
        double elapsedTime = SystemAPI.Time.ElapsedTime;
        float deltatime = SystemAPI.Time.DeltaTime;
        new SpinningEntityJob
        {
            ConstantSpinningDataLookup = constantSpinningDataLookup,
            DeltaTime = deltatime,
            ElapsedTime = elapsedTime
        }.ScheduleParallel();
        //JobHandle jobHandle = job.ScheduleParallel(state.Dependency);
        //jobHandle.Complete();
    }
    [BurstCompile]
    private partial struct SpinningEntityJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<ConstantSpinningData> ConstantSpinningDataLookup;
        public double ElapsedTime;
        public float DeltaTime;
        [BurstCompile]
        private void Execute(
            Entity entity,
            RefRW<LocalTransform> localTransform,
            RefRO<ConstantSpinningData> constantSpinningData
            )
        {
            if (ConstantSpinningDataLookup.IsComponentEnabled(entity))
            {
                quaternion newRot = quaternion.RotateY(constantSpinningData.ValueRO.YSpinAngle * DeltaTime);
                localTransform.ValueRW.Rotation = math.mul(newRot, localTransform.ValueRO.Rotation);
                //localTransform.ValueRW.RotateY(constantSpinningData.ValueRO.YSpinAngle * Mathf.Deg2Rad * DeltaTime);
                localTransform.ValueRW.Position.y += 
                    (float)math.sin(ElapsedTime * constantSpinningData.ValueRO.HeightPhaseSpeed) * constantSpinningData.ValueRO.HeightPhase * DeltaTime;
            }
        }
    }
}