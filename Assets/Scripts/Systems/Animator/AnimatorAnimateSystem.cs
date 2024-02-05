using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private BufferLookup<Child> childLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorControllerBase>();
        state.RequireForUpdate<RootTag>();
        state.RequireForUpdate<AnimatorActorLayerComponent>();
        state.RequireForUpdate<LocalTransform>();
        childLookup = state.GetBufferLookup<Child>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonBuffer<AnimatorControllerBase>(out DynamicBuffer<AnimatorControllerBase> animatorBase))
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            NativeArray<AnimatorControllerBase> animatorsArray = animatorBase.AsNativeArray();
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            EntityQuery actorsQuery = SystemAPI.QueryBuilder()
                .WithAll<RootTag, AnimatorActorLayerComponent, LocalTransform>()
                .Build();

            childLookup.Update(ref state);

            state.Dependency = new AnimateActorJob
            {
                AnimatorsArray = animatorsArray,
                ChildLookup = childLookup,
                DeltaTime = SystemAPI.Time.DeltaTime,
                ParallelWriter = parallelWriter
            }.ScheduleParallel(actorsQuery, state.Dependency);

            animatorsArray.Dispose();
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct AnimateActorJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimatorControllerBase> AnimatorsArray;
        [ReadOnly]
        public BufferLookup<Child> ChildLookup;
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
                [ChunkIndexInQuery] int sortKey,
                DynamicBuffer<AnimatorActorLayerComponent> actorAnimatorLayers,
                Entity actorEntity
            )
        {
            for (int i = 0; i < actorAnimatorLayers.Length; i++)
            {
                AnimatorActorLayerComponent animatorActorLayerComponent = actorAnimatorLayers[i];
                animatorActorLayerComponent.AnimationTime += DeltaTime;
                actorAnimatorLayers[i] = animatorActorLayerComponent;
            }
        }

        private void AnimatePart()
        {

        }
    }
}