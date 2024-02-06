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
    private BufferLookup<AnimatorStateData> animatorStateDataLookup;
    //private 
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
        state.RequireForUpdate<AnimatorControllerBase>();
        parametersLookup = state.GetBufferLookup<AnimatorParameter>(true);
        animatorLayersLookup = state.GetBufferLookup<AnimatorLayersData>(true);
        animatorStateDataLookup = state.GetBufferLookup<AnimatorStateData>(true);
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
            animatorStateDataLookup.Update(ref state);

            state.Dependency = new OrganizeAnimatorActorsJob
            {
                AnimatorBase = animatorsArray,
                ParametersLookup = parametersLookup,
                AnimatorLayersLookup = animatorLayersLookup,
                AnimatorStateDataLookup = animatorStateDataLookup,
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
        [ReadOnly]
        public BufferLookup<AnimatorStateData> AnimatorStateDataLookup;
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
                    ParallelWriter.AddBuffer<AnimatorActorTransitionComponent>(sortKey, actorEntity);
                    foreach (AnimatorLayersData item in animatorLayers)
                    {
                        int defaulStateIndex = -1;
                        if (AnimatorStateDataLookup.TryGetBuffer(item.LayerEntity, out DynamicBuffer<AnimatorStateData> statesBuffer))
                        {
                            foreach (AnimatorStateData stateData in statesBuffer)
                            {
                                if (stateData.DefaultState)
                                {
                                    defaulStateIndex = stateData.StateIndex;
                                }
                            }
                        }
                        AnimatorActorLayerComponent animatorActorLayerComponent = new AnimatorActorLayerComponent();
                        animatorActorLayerComponent.AnimationTime = 0f;
                        animatorActorLayerComponent.LayerIndex = item.LayerIndex;
                        animatorActorLayerComponent.CurrentStateIndex = item.DefaultLayerState;
                        animatorActorLayerComponent.LayerEntity = item.LayerEntity;
                        ParallelWriter.AppendToBuffer<AnimatorActorLayerComponent>(sortKey, actorEntity, animatorActorLayerComponent);
                        AnimatorActorTransitionComponent animatorActorTransitionComponent =  new AnimatorActorTransitionComponent
                        {
                            LayerIndex = item.LayerIndex,
                            Running = false,
                            HasExitTime = false,
                            TransitionTimer = 0f,
                            TransitionDuration = 0f,
                            OffsetTimeDuration = 0f,
                            CurrentStateIndex = defaulStateIndex,
                            NextStateIndex = -1,
                            ExitTimeDuration = 0f,
                        };
                        ParallelWriter.AppendToBuffer<AnimatorActorTransitionComponent>(sortKey, actorEntity, animatorActorTransitionComponent);
                    }
                }
            }
        }
    }
}