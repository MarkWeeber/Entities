using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor.Animations;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private BufferLookup<AnimatorStateData> animatorStateDataLookup;
    private BufferLookup<AnimatorStateTransitionData> animatorStateTransitionLookup;
    private BufferLookup<AnimatorStateTransitionCondition> animatorStateTransitionConditionLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorControllerBase>();
        state.RequireForUpdate<RootTag>();
        state.RequireForUpdate<AnimatorActorLayerComponent>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<AnimatorActorPartComponent>();
        animatorStateDataLookup = state.GetBufferLookup<AnimatorStateData>(true);
        animatorStateTransitionLookup = state.GetBufferLookup<AnimatorStateTransitionData>(true);
        animatorStateTransitionConditionLookup = state.GetBufferLookup<AnimatorStateTransitionCondition>(true);
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
                .WithAll<
                    RootTag,
                    AnimatorActorLayerComponent,
                    LocalTransform,
                    AnimatorActorPartComponent,
                    AnimatorActorParametersComponent,
                    AnimatorActorTransitionComponent>()
                .Build();


            animatorStateDataLookup.Update(ref state);
            animatorStateTransitionLookup.Update(ref state);
            animatorStateTransitionConditionLookup.Update(ref state);

            state.Dependency = new AnimateActorJob
            {
                AnimatorsArray = animatorsArray,
                AnimatorStateDataLookup = animatorStateDataLookup,
                AnimatorStateTransitionLookup = animatorStateTransitionLookup,
                AnimatorStateTransitionConditionLookup = animatorStateTransitionConditionLookup,
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
        public BufferLookup<AnimatorStateData> AnimatorStateDataLookup;
        [ReadOnly]
        public BufferLookup<AnimatorStateTransitionData> AnimatorStateTransitionLookup;
        [ReadOnly]
        public BufferLookup<AnimatorStateTransitionCondition> AnimatorStateTransitionConditionLookup;
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
                [ChunkIndexInQuery] int sortKey,
                DynamicBuffer<AnimatorActorLayerComponent> actorAnimatorLayers,
                DynamicBuffer<AnimatorActorParametersComponent> actorParameters,
                DynamicBuffer<AnimatorActorTransitionComponent> animatorActorTransitionComponent,
                Entity actorEntity
            )
        {
            
            for (int i = 0; i < actorAnimatorLayers.Length; i++) // each layer is computed separately
            {
                // get current state
                AnimatorActorLayerComponent actorLayer = actorAnimatorLayers[i];
                int currentStateIndex = actorLayer.CurrentStateIndex;
                int layerIndex = actorLayer.LayerIndex;
                CheckForTransitions(currentStateIndex, layerIndex, actorLayer, actorParameters, animatorActorTransitionComponent);
                // check parameters against rulesets
                // check for any current transitions
                // animate with or without transitions    
                actorLayer.AnimationTime += DeltaTime;
                actorAnimatorLayers[i] = actorLayer;
                Entity layerEntity = actorLayer.LayerEntity;

            }
            

        }

        private void CheckForTransitions(
            int currentStateIndex,
            int layerIndex,
            AnimatorActorLayerComponent actorLayer,
            DynamicBuffer<AnimatorActorParametersComponent> actorParameters,
            DynamicBuffer<AnimatorActorTransitionComponent> animatorActorTransitionComponent)
        {
            Entity layerEntity =  actorLayer.LayerEntity;
            if (AnimatorStateDataLookup.TryGetBuffer(layerEntity, out DynamicBuffer<AnimatorStateData> animatorStateDataBuffer))
            {
                foreach (AnimatorStateData animatorState in animatorStateDataBuffer)
                {
                    if (actorLayer.CurrentStateIndex == animatorState.StateIndex)
                    {
                        Entity transitionsEnity = animatorState.TransitionsHoldingEntity;
                        if (AnimatorStateTransitionLookup.TryGetBuffer(
                            transitionsEnity,
                            out DynamicBuffer<AnimatorStateTransitionData> animatorStateTransitionDataBuffer
                            ))
                        {
                            foreach(AnimatorStateTransitionData animatorStateTransitionData in animatorStateTransitionDataBuffer)
                            {
                                if(CheckConditionsMatching(animatorStateTransitionData, actorParameters))
                                {
                                    // if match register transition
                                    // acquire current transition component
                                    for (int i = 0; i < animatorActorTransitionComponent.Length; i++)
                                    {
                                        if (animatorActorTransitionComponent[i].LayerIndex == layerIndex)
                                        {
                                            // register transition
                                            int nextStateIndex = animatorStateTransitionData.DestinationStateIndex;
                                            AnimatorActorTransitionComponent transitionData =  animatorActorTransitionComponent[i];
                                            if (transitionData.NextStateIndex == -1) // first ever transition
                                            {
                                                transitionData.NextStateIndex = nextStateIndex;
                                            }
                                            else
                                            {
                                                transitionData.CurrentStateIndex = transitionData.NextStateIndex;
                                                transitionData.NextStateIndex = nextStateIndex;
                                            }
                                            transitionData.Running = true;
                                            transitionData.HasExitTime = animatorStateTransitionData.HasExitTime;
                                            transitionData.TransitionDuration = animatorStateTransitionData.Duration;
                                            transitionData.TransitionTimer = animatorStateTransitionData.Duration;
                                            transitionData.ExitTimeDuration = animatorStateTransitionData.ExitTime;
                                            transitionData.OffsetTimeDuration = animatorStateTransitionData.Offset;
                                            animatorActorTransitionComponent[i] = transitionData;
                                            actorLayer.CurrentStateIndex = nextStateIndex;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        private bool CheckConditionsMatching(
            AnimatorStateTransitionData animatorStateTransitionData,
            DynamicBuffer<AnimatorActorParametersComponent> actorParameters)
        {
            bool result = false;
            Entity conditionsHoldingEntity = animatorStateTransitionData.ConditionsHoldingEntity;
            if (AnimatorStateTransitionConditionLookup.TryGetBuffer(conditionsHoldingEntity, out DynamicBuffer<AnimatorStateTransitionCondition> conditions))
            {
                bool fullmatch = true;
                for (int i = 0; i < actorParameters.Length; i++)
                {
                    foreach (AnimatorStateTransitionCondition condition in conditions)
                    {
                        if (actorParameters[i].ParameterName == condition.Parameter)
                        {
                            bool conditionMatched = false;
                            float currentValue = actorParameters[i].Value;
                            float treshold = condition.Treshold;
                            AnimatorConditionMode mode = condition.Mode;
                            // numeric
                            if (
                                actorParameters[i].Type == AnimatorControllerParameterType.Float
                                || actorParameters[i].Type == AnimatorControllerParameterType.Int)
                            {
                                switch (mode)
                                {
                                    case AnimatorConditionMode.Greater:
                                        conditionMatched = currentValue > treshold;
                                        break;
                                    case AnimatorConditionMode.Less:
                                        conditionMatched = currentValue < treshold;
                                        break;
                                    case AnimatorConditionMode.Equals:
                                        conditionMatched = currentValue == treshold;
                                        break;
                                    case AnimatorConditionMode.NotEqual:
                                        conditionMatched = currentValue != treshold;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            // bool
                            if (actorParameters[i].Type == AnimatorControllerParameterType.Bool)
                            {
                                switch (mode)
                                {
                                    case AnimatorConditionMode.If:
                                        conditionMatched = currentValue == 1;
                                        break;
                                    case AnimatorConditionMode.IfNot:
                                        conditionMatched = currentValue == -1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            // trigger
                            if (actorParameters[i].Type == AnimatorControllerParameterType.Trigger)
                            {
                                if (currentValue > 0)
                                {
                                    conditionMatched = true;
                                    AnimatorActorParametersComponent parametersComponent = actorParameters[i];
                                    parametersComponent.Value = 0f;
                                    actorParameters[i] = parametersComponent;
                                }
                            }
                            if (!conditionMatched)
                            {
                                fullmatch = false;
                            }
                        }
                    }
                }
                if (fullmatch)
                {
                    result = true;
                }

            }
            return result;
        }

        private void AnimatePart()
        {
            
        }
    }
}