using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(AnimatorControllerBuildSystemObsolete))]
public partial struct AnimatorDataBaseBuildSystem : ISystem
{
    private EntityCommandBuffer ECB;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorBaseControllerComponent>();
    }
    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        AnimatorBaseControllerComponent animatorSource = SystemAPI.ManagedAPI.GetSingleton<AnimatorBaseControllerComponent>();
        if (animatorSource.Updated)
        {
            ECB = new EntityCommandBuffer(Allocator.Temp);
            BuildAnimatorDataBase(animatorSource);
            ECB.Playback(state.EntityManager);
            ECB.Dispose();
            animatorSource.Updated = false;
        }
    }

    private void BuildAnimatorDataBase(AnimatorBaseControllerComponent animatorControllerComponent)
    {
        // prepare new entity for database
        Entity databaseEntity = ECB.Instantiate(animatorControllerComponent.EmptyEntity);
        FixedString64Bytes rootEntityName = (FixedString64Bytes)"AnimatorControllerDataBase";
        ECB.SetName(databaseEntity, in rootEntityName);
        // adding necessary buffers
        ECB.AddBuffer<AnimatorDB>(databaseEntity);
        ECB.AddBuffer<AnimationDB>(databaseEntity);
        ECB.AddBuffer<AnimatorLayerDB>(databaseEntity);
        ECB.AddBuffer<LayerStateDB>(databaseEntity);
        ECB.AddBuffer<StateTransitionDB>(databaseEntity);
        ECB.AddBuffer<TransitionCondtionDB>(databaseEntity);
        ECB.AddBuffer<AnimatorParametersDB>(databaseEntity);
        // animators

        NativeArray<AnimatorDB> animatorTable = new NativeArray<AnimatorDB>(animatorControllerComponent.Value.Count, Allocator.Temp);
        for (int animatorIndex = 0; animatorIndex < animatorControllerComponent.Value.Count; animatorIndex++)
        {
            UnityEditor.Animations.AnimatorController animatorController = animatorControllerComponent.Value[animatorIndex] as UnityEditor.Animations.AnimatorController;
            var animatorItem = new AnimatorDB
            {
                Id = animatorIndex,
                Name = (FixedString32Bytes)animatorControllerComponent.Value[animatorIndex].name
            };
            animatorTable[animatorIndex] = animatorItem;
            ECB.AppendToBuffer(databaseEntity, animatorItem);

            // animation clips
            NativeArray<AnimationDB> animationsTable = new NativeArray<AnimationDB>(animatorController.animationClips.Length, Allocator.Temp);
            for (int animationIndex = 0; animationIndex < animatorController.animationClips.Length; animationIndex++)
            {
                AnimationClip animationClip = animatorController.animationClips[animationIndex];
                var animationItem = new AnimationDB
                {
                    Id = animationIndex,
                    Name = (FixedString32Bytes)animationClip.name,
                    Looped = animationClip.isLooping,
                    AnimatorId = animatorIndex
                };
                animationsTable[animationIndex] = animationItem;
                ECB.AppendToBuffer(databaseEntity, animationItem);

                // animation clip curve data
#pragma warning disable CS0618
                var curveData = AnimationUtility.GetAllCurves(animationClip, includeCurveData: true);
#pragma warning restore CS0618
                ECB.AddBuffer<AnimationCurveDB>(databaseEntity);
                NativeArray<AnimationCurveDB> animationCurveTable = new NativeArray<AnimationCurveDB>(curveData.Length, Allocator.Temp);
                for (int curveIndex = 0; curveIndex < curveData.Length; curveIndex++)
                {
                    AnimationClipCurveData animationClipCurveItem = curveData[curveIndex];
                    AnimationCurveDB cureItem = new AnimationCurveDB
                    {
                        Id = curveIndex,
                        AnimatorId = animatorIndex,
                        AnimationId = animationIndex,
                        Path = (FixedString512Bytes)animationClipCurveItem.path,
                        PropertyName = (FixedString32Bytes)animationClipCurveItem.propertyName
                    };
                    animationCurveTable[curveIndex] = cureItem;
                    ECB.AppendToBuffer(databaseEntity, cureItem);

                    // curve keys
                    ECB.AddBuffer<AnimationCurveKeyDB>(databaseEntity);
                    var keys = animationClipCurveItem.curve.keys;
                    NativeArray<AnimationCurveKeyDB> animationCurveKeyTable = new NativeArray<AnimationCurveKeyDB>(keys.Length, Allocator.Temp);
                    for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                    {
                        Keyframe keyFrameItem = keys[keyIndex];
                        AnimationCurveKeyDB curveKeyItem = new AnimationCurveKeyDB
                        {
                            Id = keyIndex,
                            AnimatorId = animatorIndex,
                            CurveId = curveIndex,
                            Time = keyFrameItem.time,
                            Value = keyFrameItem.value,
                        };
                        animationCurveKeyTable[keyIndex] = curveKeyItem;
                        ECB.AppendToBuffer(databaseEntity, curveKeyItem);
                    }
                    // disposing arrays
                    animationCurveKeyTable.Dispose();
                }
                // disposing arrays
                animationCurveTable.Dispose();
            }

            // animator parameters
            UnityEditor.Animations.AnimatorController controller = new UnityEditor.Animations.AnimatorController();
            var parameters = animatorController.parameters;
            NativeArray<AnimatorParametersDB> animatorParametersTable = new NativeArray<AnimatorParametersDB>(parameters.Length, Allocator.Temp);
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++ )
            {
                UnityEngine.AnimatorControllerParameter parameter = parameters[parameterIndex];
                var parameterItem = new AnimatorParametersDB
                {
                    Id = parameterIndex,
                    AnimatorId = animatorIndex,
                    ParameterName = (FixedString32Bytes)parameter.name,
                    Type = parameter.type,
                    DefaultBool = parameter.defaultBool,
                    DefaultFloat = parameter.defaultFloat,
                    DefaultInt = parameter.defaultInt
                };
                animatorParametersTable[parameterIndex] = parameterItem;
                ECB.AppendToBuffer(databaseEntity, parameterItem);
            }

            // animator layers
            var layers = animatorController.layers;
            NativeArray<AnimatorLayerDB> animatorLayersTable = new NativeArray<AnimatorLayerDB>(layers.Length, Allocator.Temp);
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                UnityEditor.Animations.AnimatorControllerLayer layer = layers[layerIndex];
                var layerItem = new AnimatorLayerDB
                {
                    Id = layerIndex,
                    AnimatorId = animatorIndex,
                    DefaultWeight = layer.defaultWeight,
                };
                animatorLayersTable[layerIndex] = layerItem;
                ECB.AppendToBuffer(databaseEntity, layerItem);
                // TO DO avatar mask filters
                // state machine and states per layers
                var states = layer.stateMachine.states;
                NativeArray<LayerStateDB> layerStatesTable = new NativeArray<LayerStateDB>(states.Length, Allocator.Temp);
                NativeList<StateTransitionDB> transitionsTable = new NativeList<StateTransitionDB>(Allocator.Temp);
                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    UnityEditor.Animations.ChildAnimatorState childAnimatorState = states[stateIndex];
                    int relevantAnimationIndex = -1;
                    foreach (var animationClip in animationsTable)
                    {
                        if (animationClip.Name == (FixedString32Bytes)childAnimatorState.state.motion.name)
                        {
                            relevantAnimationIndex = animationClip.Id;
                            break;
                        }
                    }
                    bool defaultState = layer.stateMachine.defaultState == childAnimatorState.state;
                    var layerStatItem = new LayerStateDB
                    {
                        Id = stateIndex,
                        AnimatorId = animatorIndex,
                        LayerId = layerIndex,
                        AnimationClipId = relevantAnimationIndex,
                        DefaultState = defaultState,
                        StateName = (FixedString32Bytes)childAnimatorState.state.name,
                        Speed = childAnimatorState.state.speed
                    };
                    layerStatesTable[stateIndex] = layerStatItem;
                    ECB.AppendToBuffer(databaseEntity, layerStatItem);

                    // transitions
                    var transitions = childAnimatorState.state.transitions;
                    for (int transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
                    {
                        UnityEditor.Animations.AnimatorStateTransition animatorStateTransition = transitions[transitionIndex];
                        var stateTransitionItem = new StateTransitionDB
                        {
                            Id = transitionIndex,
                            AnimatorId = animatorIndex,
                            StateId = stateIndex,
                            DestinationStateId = -1,
                            DestinationStateName = (FixedString32Bytes)animatorStateTransition.destinationState.name,
                            HasExitTime = animatorStateTransition.hasExitTime,
                            ExitTime = animatorStateTransition.exitTime,
                            TransitionDuration = animatorStateTransition.duration,
                            TransitionOffset = animatorStateTransition.offset
                        };
                        transitionsTable.Add(stateTransitionItem);

                        // transition conditions
                        var conditions = animatorStateTransition.conditions;
                        NativeArray<TransitionCondtionDB> transitionCondtionsTable = new NativeArray<TransitionCondtionDB>(conditions.Length, Allocator.Temp);
                        for (int condtionIndex = 0; condtionIndex < conditions.Length; condtionIndex++)
                        {
                            UnityEditor.Animations.AnimatorCondition animatorCondition = conditions[condtionIndex];
                            var transitionConditionItem = new TransitionCondtionDB
                            {
                                Id = condtionIndex,
                                AnimatorId = animatorIndex,
                                TransitionId = transitionIndex,
                                Mode = animatorCondition.mode,
                                Parameter = (FixedString32Bytes)animatorCondition.parameter,
                                Treshold = animatorCondition.threshold
                            };
                            transitionCondtionsTable[condtionIndex] = transitionConditionItem;
                            ECB.AppendToBuffer(databaseEntity, transitionConditionItem);
                        }

                        // disposing arrays
                        transitionCondtionsTable.Dispose();
                    }
                }
                // settings destination ids
                for (int i = 0; i < transitionsTable.Length; i++)
                {
                    var _transition = transitionsTable[i];
                    foreach (var _layer in layerStatesTable)
                    {
                        if (_transition.DestinationStateName == _layer.StateName)
                        {
                            _transition.DestinationStateId = _layer.Id;
                            transitionsTable[i] = _transition;
                            break;
                        }
                    }
                }
                // saving transition tabel to buffer
                foreach (var _transition in transitionsTable)
                {
                    ECB.AppendToBuffer(databaseEntity, _transition);
                }
                // disposing arrays and lists
                transitionsTable.Dispose();
                layerStatesTable.Dispose();
            }
            // disposing arrays
            animationsTable.Dispose();
            animatorLayersTable.Dispose();
            animatorParametersTable.Dispose();
        }
        // disposing arrays
        animatorTable.Dispose();
    }
}


public partial struct AnimatorDB : IBufferElementData
{
    public int Id;
    public FixedString32Bytes Name;
}

public partial struct AnimationDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public FixedString32Bytes Name;
    public bool Looped;
}

public partial struct AnimationCurveDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public int AnimationId;
    public FixedString512Bytes Path;
    public FixedString32Bytes PropertyName;
}

public partial struct AnimationCurveKeyDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public int CurveId;
    public float Time;
    public float Value;
}

public partial struct AnimatorLayerDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public float DefaultWeight;
}

public partial struct LayerStateDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public int LayerId;
    public bool DefaultState;
    public int AnimationClipId;
    public float Speed;
    public FixedString32Bytes StateName;
}

public partial struct StateTransitionDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public int StateId;
    public int DestinationStateId;
    public FixedString32Bytes DestinationStateName;
    public bool HasExitTime;
    public float ExitTime;
    public float TransitionDuration;
    public float TransitionOffset;
}

public partial struct TransitionCondtionDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public int TransitionId;
    public UnityEditor.Animations.AnimatorConditionMode Mode;
    public FixedString32Bytes Parameter;
    public float Treshold;
}

public partial struct AnimatorParametersDB : IBufferElementData
{
    public int Id;
    public int AnimatorId;
    public FixedString32Bytes ParameterName;
    public UnityEngine.AnimatorControllerParameterType Type;
    public float DefaultFloat;
    public int DefaultInt;
    public bool DefaultBool;
}