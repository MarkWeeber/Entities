using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AnimatorActorBuildSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorDB>();
        state.RequireForUpdate<AnimatorActorComponent>();
        state.RequireForUpdate<AnimatorActorBuilderComponent>();
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
            NativeArray<AnimatorLayerDB> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerDB>().AsNativeArray();
            NativeArray<LayerStateDB> states = SystemAPI.GetSingletonBuffer<LayerStateDB>().AsNativeArray();
            NativeArray<AnimatorParametersDB> parameters = SystemAPI.GetSingletonBuffer<AnimatorParametersDB>().AsNativeArray();
            
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            EntityQuery actorsQuery = SystemAPI.QueryBuilder().WithAll<AnimatorActorComponent, AnimatorActorBuilderComponent>().Build();

            state.Dependency = new OrganizeActorsJob
            {
                Animators = animators,
                Layers = layers,
                Parameters = parameters,
                States = states,
                ParallelWriter = parallelWriter
            }.ScheduleParallel(actorsQuery, state.Dependency);
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            animators.Dispose();
            layers.Dispose();
            states.Dispose();
            parameters.Dispose();
        }
    }

    [BurstCompile]
    public partial struct OrganizeActorsJob : IJobEntity
    {
        [ReadOnly] public NativeArray<AnimatorDB> Animators;
        [ReadOnly] public NativeArray<AnimatorLayerDB> Layers;
        [ReadOnly] public NativeArray<AnimatorParametersDB> Parameters;
        [ReadOnly] public NativeArray<LayerStateDB> States;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute
            (
                [ChunkIndexInQuery] int sortKey,
                RefRW<AnimatorActorComponent> animatorComponent,
                Entity actorEntity
            )
        {
            ParallelWriter.SetComponentEnabled<AnimatorActorBuilderComponent>(sortKey, actorEntity, false);
            // search appropriate animator
            foreach (var animator in Animators)
            {
                if (animator.Name == animatorComponent.ValueRO.AnimatorControllerName)
                {
                    // set animator Id
                    animatorComponent.ValueRW.AnimatorId = animator.Id;
                    // create parameters
                    ParallelWriter.AddBuffer<AnimatorActorParametersComponent>(sortKey, actorEntity);
                    foreach (var parameter in Parameters)
                    {
                        if (parameter.AnimatorId != animator.Id)
                        {
                            continue;
                        }
                        var parameterItem = new AnimatorActorParametersComponent();
                        parameterItem.ParameterName = parameter.ParameterName;
                        parameterItem.Type = parameter.Type;
                        switch (parameter.Type)
                        {
                            case AnimatorControllerParameterType.Float:
                                parameterItem.Value = parameter.DefaultFloat;
                                break;
                            case AnimatorControllerParameterType.Int:
                                parameterItem.Value = parameter.DefaultInt;
                                break;
                            case AnimatorControllerParameterType.Bool:
                                parameterItem.Value = (parameter.DefaultBool) ? 1f : -1f;
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                parameterItem.Value = 0f;
                                break;
                            default:
                                break;
                        }
                        ParallelWriter.AppendToBuffer(sortKey, actorEntity, parameterItem);
                    }
                    // create layers
                    ParallelWriter.AddBuffer<AnimatorLayerData>(sortKey, actorEntity);
                    foreach (var layer in Layers)
                    {
                        if (layer.AnimatorId != animator.Id)
                        {
                            continue;
                        }
                        foreach (var state in States)
                        {
                            if (state.LayerId == layer.Id && state.AnimatorId == animator.Id && state.DefaultState)
                            {
                                AnimatorLayerData animatorLayerData = new AnimatorLayerData
                                {
                                    AnimatorId = layer.AnimatorId,
                                    LayerId = layer.Id,
                                    LayerTimer = 0f,
                                    CurrentStateId = state.Id,
                                    TransitionRunning = false,
                                    HasExitTime = false,
                                    TransitionTimer = 0f,
                                    TransitionDuration = 0f,
                                    ExitTimeDuration = 0f,
                                    OffsetTimeDuration = 0f,
                                    NextStateId = state.Id
                                };
                                ParallelWriter.AppendToBuffer(sortKey, actorEntity, animatorLayerData);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}

public partial struct AnimatorLayerData : IBufferElementData
{
    public int AnimatorId;
    public int LayerId;
    public float LayerTimer;
    public int CurrentStateId;
    public bool TransitionRunning;
    public bool HasExitTime;
    public float TransitionTimer;
    public float TransitionDuration;
    public float ExitTimeDuration;
    public float OffsetTimeDuration;
    public int NextStateId;

}