using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AnimatorControllerBuildSystem : ISystem
{
    private BufferLookup<LinkedEntityGroup> linkedEntitiesLookup;
    private EntityCommandBuffer ECB;
    public void OnCreate(ref SystemState state)
    {
        linkedEntitiesLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        ECB = new EntityCommandBuffer(Allocator.Temp);
        foreach((AnimatorBaseControllerComponent animatorControllerComponent, Entity entity) in SystemAPI.Query<AnimatorBaseControllerComponent>().WithEntityAccess())
        {
            Organize(entity, animatorControllerComponent);
        }
        ECB.Playback(state.EntityManager);
        ECB.Dispose();
    }

    private void Organize(Entity entity, AnimatorBaseControllerComponent animatorControllerComponent)
    {
        ECB.SetComponentEnabled<AnimatorBaseControllerComponent>(entity, false);
        Entity emtpyEntity = animatorControllerComponent.EmptyEntity;
        Entity rootEntity = ECB.Instantiate(emtpyEntity);
        FixedString64Bytes rootEntityName = (FixedString64Bytes)"AnimatorControllerBase";
        ECB.SetName(rootEntity, in rootEntityName);
        ECB.AddBuffer<AnimatorControllerBase>(rootEntity);
        ECB.AddBuffer<LinkedEntityGroup>(rootEntity);
        ECB.AppendToBuffer(rootEntity, new LinkedEntityGroup { Value = rootEntity });
        foreach (AnimatorController animatorController in animatorControllerComponent.Value) // each animator
        {
            Entity animatorEntitiy = CreateChildEntity<AnimatorTag>("Animator", rootEntity, rootEntity, emtpyEntity);
            ECB.AppendToBuffer(rootEntity, new AnimatorControllerBase
            {
                AnimatorControllerEntity = animatorEntitiy,
                AnimatorName = (FixedString32Bytes)animatorController.name
            });
            ECB.AddComponent(animatorEntitiy, new AnimatorControllerData
            {
                AnimatorName = (FixedString32Bytes)animatorController.name
            });
            ECB.AddBuffer<AnimatorParameter>(animatorEntitiy);
            foreach (AnimatorControllerParameter animatorControllerParameter in animatorController.parameters) // params dynamic buffer
            {
                ECB.AppendToBuffer(animatorEntitiy, new AnimatorParameter
                {
                    ParameterName = (FixedString32Bytes)animatorControllerParameter.name,
                    Type = animatorControllerParameter.type,
                    DefaultFloat = animatorControllerParameter.defaultFloat,
                    DefaultInt = animatorControllerParameter.defaultInt,
                    DefaultBool = animatorControllerParameter.defaultBool
                });
            }
            int animationClipIndex = 0;
            ECB.AddBuffer<AnimationClipData>(animatorEntitiy);
            foreach (AnimationClip animationClip in animatorController.animationClips) // animation clips
            {
                Entity animationEntitiy = CreateChildEntity<AnimationClipTag>("Animation", animatorEntitiy, rootEntity, emtpyEntity);
                ECB.AddBuffer<AnimationClipPathPropery>(animationEntitiy);
#pragma warning disable CS0618 // Тип или член устарел
                foreach (AnimationClipCurveData clipCurveData in AnimationUtility.GetAllCurves(animationClip, includeCurveData: true)) // curve data
                {
                    Entity animationClipKeysEntity = CreateChildEntity("Keys", animationEntitiy, rootEntity, emtpyEntity); // keys holding entity
                    ECB.AppendToBuffer(animationEntitiy, new AnimationClipPathPropery
                    {
                        Path = (FixedString512Bytes)clipCurveData.path,
                        PropertyName = (FixedString32Bytes)clipCurveData.propertyName,
                        KeysEntity = animationClipKeysEntity
                    });
                    ECB.AddBuffer<AnimationClipKeyFrame>(animationClipKeysEntity);
                    foreach (Keyframe keyframe in clipCurveData.curve.keys)
                    {
                        ECB.AppendToBuffer(animationClipKeysEntity, new AnimationClipKeyFrame
                        {
                            Time = keyframe.time,
                            Value = keyframe.value
                        });
                    }
                }
#pragma warning restore CS0618 // Тип или член устарел
                ECB.AppendToBuffer<AnimationClipData>(animatorEntitiy, new AnimationClipData
                {
                    AnimationClipIndex = animationClipIndex,
                    AnimationClipName = (FixedString32Bytes)animationClip.name,
                    AnimationClipHoldingEntity = animationEntitiy
                });
                animationClipIndex++;
            }
            ECB.AddBuffer<AnimatorLayersData>(animatorEntitiy);
            int layerIndex = 0;
            foreach (AnimatorControllerLayer layer in animatorController.layers) // layers
            {
                Entity layerEntity = CreateChildEntity("Layer", animatorEntitiy, rootEntity, emtpyEntity);
                ECB.AddComponent(layerEntity, new AnimatorLayer
                {
                    LayerName = (FixedString32Bytes)layer.name,
                    Weight = layer.defaultWeight,
                    LayerBlendingMode = layer.blendingMode,
                });
                ECB.AddBuffer<AnimatorStateData>(layerEntity);
                int defaultStateIndex = 0;
                int stateIndex = 0;
                foreach (ChildAnimatorState state in layer.stateMachine.states) // states
                {
                    Entity transitionsHoldingEntity = CreateChildEntity("Transitions", layerEntity, rootEntity, emtpyEntity);
                    bool defaultState = layer.stateMachine.defaultState == state.state;
                    ECB.AppendToBuffer(layerEntity, new AnimatorStateData
                    {
                        StateIndex = stateIndex,
                        DefaultState = defaultState,
                        AnimatorStateName = state.state.name,
                        AnimationClipName = state.state.motion.name,
                        Speed = state.state.speed,
                        TransitionsHoldingEntity = transitionsHoldingEntity,
                    });
                    if (defaultState)
                    {
                        defaultStateIndex = stateIndex;
                    }
                    stateIndex++;
                    ECB.AddBuffer<AnimatorStateTransitionData>(transitionsHoldingEntity);
                    foreach (AnimatorStateTransition stateTransition in state.state.transitions)
                    {
                        Entity conditionsHoldingEntity = CreateChildEntity("Conditions", transitionsHoldingEntity, rootEntity, emtpyEntity);
                        ECB.AppendToBuffer(transitionsHoldingEntity, new AnimatorStateTransitionData
                        {
                            DestinationStateName = (FixedString32Bytes)stateTransition.destinationState.name,
                            HasExitTime = stateTransition.hasExitTime,
                            ExitTime = stateTransition.exitTime,
                            HasFixedDuration = stateTransition.hasFixedDuration,
                            Duration = stateTransition.duration,
                            Offset = stateTransition.offset,
                            InterruptionSource = stateTransition.interruptionSource,
                            OrderedInterruption = stateTransition.orderedInterruption,
                            ConditionsHoldingEntity = conditionsHoldingEntity,
                        });
                        ECB.AddBuffer<AnimatorStateTransitionCondition>(conditionsHoldingEntity);
                        foreach (AnimatorCondition animatorCondition in stateTransition.conditions)
                        {
                            ECB.AppendToBuffer(conditionsHoldingEntity, new AnimatorStateTransitionCondition
                            {
                                Mode = animatorCondition.mode,
                                Parameter = animatorCondition.parameter,
                                Treshold = animatorCondition.threshold
                            });
                        }
                    }
                }
                ECB.AppendToBuffer<AnimatorLayersData>(animatorEntitiy, new AnimatorLayersData
                {
                    DefaultLayerState = defaultStateIndex,
                    LayerIndex = layerIndex,
                    LayerEntity = layerEntity
                });
                layerIndex++;
            }
        }
    }
    private Entity CreateChildEntity(string entityName, Entity parentEntity, Entity mainEntity, Entity emptyEntity)
    {
        Entity createdEntity = ECB.Instantiate(emptyEntity);
        FixedString64Bytes animatorEntityName = (FixedString64Bytes)entityName;
        ECB.SetName(createdEntity, in animatorEntityName);
        ECB.AddComponent(createdEntity, new Parent { Value = parentEntity });
        ECB.AppendToBuffer(mainEntity, new LinkedEntityGroup { Value = createdEntity });
        return createdEntity;
    }
    private Entity CreateChildEntity<T>(string entityName, Entity parentEntity, Entity mainEntity, Entity emptyEntity) where T : unmanaged, IComponentData
    {
        Entity createdEntity = CreateChildEntity(entityName, parentEntity, mainEntity, emptyEntity);
        ECB.AddComponent<T>(createdEntity);
        return createdEntity;
    }
}

public struct AnimatorControllerBase : IBufferElementData
{
    public FixedString32Bytes AnimatorName;
    public Entity AnimatorControllerEntity;
}

public struct AnimatorControllerData : IComponentData
{
    public FixedString32Bytes AnimatorName;
}

public struct AnimatorLayersData : IBufferElementData
{
    public int LayerIndex;
    public Entity LayerEntity;
    public int DefaultLayerState;
}

public struct AnimatorParameter : IBufferElementData
{
    public FixedString32Bytes ParameterName;
    public AnimatorControllerParameterType Type;
    public float DefaultFloat;
    public int DefaultInt;
    public bool DefaultBool;
}

public struct AnimationClipData : IBufferElementData
{
    public int AnimationClipIndex;
    public FixedString32Bytes AnimationClipName;
    public Entity AnimationClipHoldingEntity;
}

public struct AnimationClipPathPropery : IBufferElementData
{
    public FixedString512Bytes Path;
    public FixedString32Bytes PropertyName;
    public Entity KeysEntity;
}

public struct AnimationClipKeyFrame : IBufferElementData
{
    public float Time;
    public float Value;
}

public struct AnimatorTag : IComponentData { }

public struct AnimationClipTag : IComponentData { }

public struct AnimatorLayer : IComponentData
{
    public FixedString32Bytes LayerName;
    public float Weight;
    public AnimatorLayerBlendingMode LayerBlendingMode;
}

public struct AnimatorStateData : IBufferElementData
{
    public int StateIndex;
    public bool DefaultState;
    public FixedString32Bytes AnimatorStateName;
    public FixedString32Bytes AnimationClipName;
    public float Speed;
    public Entity TransitionsHoldingEntity;
}

public struct AnimatorStateTransitionData : IBufferElementData
{
    public FixedString32Bytes DestinationStateName;
    public bool HasExitTime;
    public float ExitTime;
    public bool HasFixedDuration;
    public float Duration;
    public  float Offset;
    public TransitionInterruptionSource InterruptionSource;
    public bool OrderedInterruption;
    public Entity ConditionsHoldingEntity;
}

public struct AnimatorStateTransitionCondition : IBufferElementData
{
    public AnimatorConditionMode Mode;
    public FixedString32Bytes Parameter;
    public float Treshold;
}

