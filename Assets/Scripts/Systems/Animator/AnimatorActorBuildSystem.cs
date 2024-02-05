using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AnimatorActorBuildSystem : ISystem
{
    private EntityQuery actorsQuery;
    private BufferLookup<AnimatorParameter> parametersLookup;
    private BufferLookup<AnimatorLayersData> animatorLayersLookup;
    //private 
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
        state.RequireForUpdate<AnimatorControllerBase>();
        parametersLookup = state.GetBufferLookup<AnimatorParameter>(true);
        animatorLayersLookup = state.GetBufferLookup<AnimatorLayersData>(true);
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
            actorsQuery = SystemAPI.QueryBuilder().WithAll<AnimatorActorComponent>().Build();

            parametersLookup.Update(ref state);
            animatorLayersLookup.Update(ref state);

            state.Dependency = new OrganizeAnimatorActorsJob
            {
                AnimatorBase = animatorsArray,
                ParametersLookup = parametersLookup,
                AnimatorLayersLookup = animatorLayersLookup,
                ParallelWriter = parallelWriter,
            }.ScheduleParallel(actorsQuery, state.Dependency);
            animatorsArray.Dispose();
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct OrganizeAnimatorActorsJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimatorControllerBase> AnimatorBase;
        [ReadOnly]
        public BufferLookup<AnimatorParameter> ParametersLookup;
        [ReadOnly]
        public BufferLookup<AnimatorLayersData> AnimatorLayersLookup;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
                [ChunkIndexInQuery] int sortKey,
                RefRW<AnimatorActorComponent> animatorComponent,
                Entity actorEntity
            )
        {
            ParallelWriter.SetComponentEnabled<AnimatorActorComponent>(sortKey, actorEntity, false);
            // searching for related animator
            bool animatorFound = false;
            for (int i = 0; i < AnimatorBase.Length; i++)
            {
                FixedString32Bytes animatorName = AnimatorBase[i].AnimatorName;
                if (animatorComponent.ValueRO.AnimatorControllerName == animatorName)
                {
                    animatorComponent.ValueRW.AnimatorControllerEntity = AnimatorBase[i].AnimatorControllerEntity;
                    animatorFound = true;
                    break;
                }
            }
            if (animatorFound)
            {
                // creating actor animator parameters
                if (ParametersLookup.TryGetBuffer(animatorComponent.ValueRO.AnimatorControllerEntity, out DynamicBuffer<AnimatorParameter> animatorParameters))
                {
                    ParallelWriter.AddBuffer<AnimatorActorParametersComponent>(sortKey, actorEntity);
                    foreach (AnimatorParameter item in animatorParameters)
                    {
                        AnimatorActorParametersComponent animatorActorParametersComponent = new AnimatorActorParametersComponent();
                        animatorActorParametersComponent.ParameterName = item.ParameterName;
                        animatorActorParametersComponent.Type = item.Type;
                        switch (item.Type)
                        {
                            case AnimatorControllerParameterType.Float:
                                animatorActorParametersComponent.Value = item.DefaultFloat;
                                break;
                            case AnimatorControllerParameterType.Int:
                                animatorActorParametersComponent.Value = item.DefaultInt;
                                break;
                            case AnimatorControllerParameterType.Bool:
                                animatorActorParametersComponent.Value = (item.DefaultBool)? 1f: -1f;
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                animatorActorParametersComponent.Value = 0f;
                                break;
                            default:
                                break;
                        }
                        ParallelWriter.AppendToBuffer<AnimatorActorParametersComponent>(sortKey, actorEntity, animatorActorParametersComponent);
                    }
                }
                // creating actor animator layers
                if (AnimatorLayersLookup.TryGetBuffer(animatorComponent.ValueRO.AnimatorControllerEntity, out DynamicBuffer<AnimatorLayersData> animatorLayers))
                {
                    ParallelWriter.AddBuffer<AnimatorActorLayerComponent>(sortKey, actorEntity);
                    foreach (AnimatorLayersData item in animatorLayers)
                    {
                        AnimatorActorLayerComponent animatorActorLayerComponent = new AnimatorActorLayerComponent();
                        animatorActorLayerComponent.AnimationTime = 0f;
                        animatorActorLayerComponent.LayerNumber = item.LayerIndex;
                        animatorActorLayerComponent.CurrentStateIndex = item.DefaultLayerState;
                        animatorActorLayerComponent.LayerEntity = item.LayerEntity;
                        ParallelWriter.AppendToBuffer<AnimatorActorLayerComponent>(sortKey, actorEntity, animatorActorLayerComponent);
                    }
                }
            }
        }
    }
}