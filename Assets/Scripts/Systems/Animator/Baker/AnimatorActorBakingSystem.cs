using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateBefore(typeof(SpawnerSystem))]
public partial struct AnimatorActorBakingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery animatorActorEntities = SystemAPI.QueryBuilder()
            .WithAll<
                AnimatorActorComponent,
                AnimatorActorLayerBuffer,
                AnimatorActorPartBufferComponent>()
            .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
            .Build();

        if (animatorActorEntities.CalculateEntityCount() < 1)
        {
            return;
        }
        if (SystemAPI.TryGetSingletonBuffer<AnimationBlobBuffer>(out DynamicBuffer<AnimationBlobBuffer> animations))
        {
            var layerStateBuffer = SystemAPI.GetSingletonBuffer<LayerStateBuffer>();
            var animationArray = animations.AsNativeArray();

            RegisterAnimationBlobIndexesIntoLayersBuffer(ref layerStateBuffer, animationArray);
            var layerStateArray = layerStateBuffer.AsNativeArray();

            var registerActorLayerJobHandle = new RegisterInitialDataToAnimatorActorLayer
            {
                Layers = layerStateArray
            }.Schedule(animatorActorEntities, state.Dependency);
            registerActorLayerJobHandle.Complete();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();

            var bindPartsJobHandle = new BindPartsWithRootEntity
            {
                Animations = animationArray,
                ParallelWriter = parallelWriter,
            }.Schedule(animatorActorEntities, state.Dependency);
            bindPartsJobHandle.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            animationArray.Dispose();
            layerStateArray.Dispose();
        }

    }

    [BurstCompile]
    private void RegisterAnimationBlobIndexesIntoLayersBuffer(
        ref DynamicBuffer<LayerStateBuffer> layerStateBuffers, NativeArray<AnimationBlobBuffer> animations)
    {
        for (int i = 0; i < layerStateBuffers.Length; i++)
        {
            var layer = layerStateBuffers[i];
            for (int k = 0; k < animations.Length; k++)
            {
                var animation = animations[k];
                if (layer.AnimationClipId == animation.Id)
                {
                    layer.AnimationBlobAssetIndex = k;
                    break;
                }
            }
            layerStateBuffers[i] = layer;
        }
    }

    [BurstCompile]
    private partial struct RegisterInitialDataToAnimatorActorLayer : IJobEntity
    {
        [ReadOnly]
        public NativeArray<LayerStateBuffer> Layers;
        [BurstCompile]
        private void Execute(AnimatorActorComponent animatorActorComponent, ref DynamicBuffer<AnimatorActorLayerBuffer> actorLayers)
        {
            for (int i = 0; i < actorLayers.Length; i++)
            {
                var actorLayer = actorLayers[i];
                foreach (var layer in Layers)
                {
                    if (actorLayer.CurrentAnimationId == layer.AnimationClipId)
                    {
                        actorLayer.CurrentAnimationBlobIndex = layer.AnimationBlobAssetIndex;
                        break;
                    }
                }
                actorLayers[i] = actorLayer;
            }
        }
    }

    [BurstCompile]
    private partial struct BindPartsWithRootEntity : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimationBlobBuffer> Animations;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            RefRO<AnimatorActorComponent> animatorActorComponent,
            DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            DynamicBuffer<AnimatorActorLayerBuffer> layers,
            Entity entity)
        {
            // adding part component and buffers for each of parts
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                foreach (var layer in layers)
                {
                    int animatorInstanceId = animatorActorComponent.ValueRO.AnimatorId;
                    var relevantAnimation = new AnimationBlobBuffer();
                    bool relevantAnimationFound = false;
                    foreach (var animation in Animations)
                    {
                        if (animation.AnimatorInstanceId == animatorInstanceId)
                        {
                            relevantAnimation = animation;
                            relevantAnimationFound = true;
                            break;
                        }
                    }
                    if (relevantAnimationFound)
                    {
                        ref PathDataPool pathDataPool = ref relevantAnimation.PathData.Value;
                        ref var pathsPool = ref pathDataPool.PathData;
                        for (int i = 0; i < pathsPool.Length; i++)
                        {
                            ref var pathData = ref pathsPool[i];
                            if (pathData.Path == part.Path)
                            {
                                ParallelWriter.AddComponent(sortKey, partEntity, new AnimatorPartComponent
                                {
                                    RootEntity = entity,
                                    PathAnimationBlobIndex = i,
                                    PartName = part.Path
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}