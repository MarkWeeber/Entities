using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorDB>();
        state.RequireForUpdate<AnimatorActorComponent>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonBuffer<AnimatorDB>(out DynamicBuffer<AnimatorDB> animatorDB))
        {
            NativeArray<AnimatorDB> animators = animatorDB.AsNativeArray();
            NativeArray<AnimationDB> animations = SystemAPI.GetSingletonBuffer<AnimationDB>().AsNativeArray();
            NativeArray<AnimatorLayerDB> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerDB>().AsNativeArray();
            NativeArray<LayerStateDB> states = SystemAPI.GetSingletonBuffer<LayerStateDB>().AsNativeArray();
            NativeArray<StateTransitionDB> transitions = SystemAPI.GetSingletonBuffer<StateTransitionDB>().AsNativeArray();
            NativeArray<TransitionCondtionDB> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionDB>().AsNativeArray();
            NativeArray<AnimatorParametersDB> parameters = SystemAPI.GetSingletonBuffer<AnimatorParametersDB>().AsNativeArray();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            EntityQuery actorsQuery = SystemAPI.QueryBuilder()
                .WithAll<AnimatorActorComponent, AnimatorActorParametersComponent, AnimatorLayerData>()
                .Build();

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            animators.Dispose();
            animations.Dispose();
            layers.Dispose();
            states.Dispose();
            transitions.Dispose();
            conditions.Dispose();
            parameters.Dispose();
        }
    }

    [BurstCompile]
    public partial struct AnimateActorJob : IJobChunk
    {
        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            throw new System.NotImplementedException();
        }
    }
}