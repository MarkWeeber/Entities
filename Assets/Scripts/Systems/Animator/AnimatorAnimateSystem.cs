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
        state.RequireForUpdate<AnimatorBuffer>();
        state.RequireForUpdate<AnimatorActorComponent>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonBuffer<AnimatorBuffer>(out DynamicBuffer<AnimatorBuffer> animatorDB))
        {
            NativeArray<AnimatorBuffer> animators = animatorDB.AsNativeArray();
            NativeArray<AnimationBuffer> animations = SystemAPI.GetSingletonBuffer<AnimationBuffer>().AsNativeArray();
            NativeArray<AnimatorLayerBuffer> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerBuffer>().AsNativeArray();
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();
            NativeArray<AnimatorParametersBuffer> parameters = SystemAPI.GetSingletonBuffer<AnimatorParametersBuffer>().AsNativeArray();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            EntityQuery actorsQuery = SystemAPI.QueryBuilder()
                .WithAll<AnimatorActorComponent, AnimatorActorParametersComponent>()
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