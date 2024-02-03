using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AnimatorControllerBuildSystem : ISystem
{
    private BufferLookup<LinkedEntityGroup> linkedEntitiesLookup;
    public void OnCreate(ref SystemState state)
    {
        linkedEntitiesLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityQuery animators = SystemAPI.QueryBuilder().WithAll<AnimatorControllerComponent>().Build();
        linkedEntitiesLookup.Update(ref state);
        JobHandle jobHandle = new OrganizeAnimatorBase
        {
            ECB = ecb,
            LinkedEntitiesLookup = linkedEntitiesLookup
        }.Schedule(animators, state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    public partial struct OrganizeAnimatorBase : IJobEntity
    {
        [ReadOnly]
        public BufferLookup<LinkedEntityGroup> LinkedEntitiesLookup;
        public EntityCommandBuffer ECB;
        private void Execute(Entity entity, AnimatorControllerComponent animatorControllerComponent)
        {
            ECB.SetComponentEnabled<AnimatorControllerComponent>(entity, false);
            Entity child = ECB.Instantiate(animatorControllerComponent.EmptyEntity);
            if (!LinkedEntitiesLookup.HasBuffer(entity))
            {
                ECB.AddBuffer<LinkedEntityGroup>(entity);
                ECB.AppendToBuffer(entity, new LinkedEntityGroup { Value = entity });
            }
            ECB.AppendToBuffer(entity, new LinkedEntityGroup { Value = child });
            foreach(var item in animatorControllerComponent.Value)
            {

            }
        }
    }
}