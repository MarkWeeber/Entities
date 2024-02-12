using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(AnimatorDotsParser))]
public class AnimatorDotsParseUtilityEditor : Editor
{
    private const string _rotLocalx = "m_LocalRotation.x";
    private const string _rotLocaly = "m_LocalRotation.y";
    private const string _rotLocalz = "m_LocalRotation.z";
    private const string _rotLocalw = "m_LocalRotation.w";
    private const string _posLocalx = "m_LocalPosition.x";
    private const string _posLocaly = "m_LocalPosition.y";
    private const string _posLocalz = "m_LocalPosition.z";
    private const string _rotLocalEulerx = "localEulerAnglesRaw.x";
    private const string _rotLocalEulery = "localEulerAnglesRaw.y";
    private const string _rotLocalEulerz = "localEulerAnglesRaw.z";
    private const string _rotLocalEulerw = "localEulerAnglesRaw.w";

    private enum ObjectType
    {
        Animator = 0,
        Animation = 1,
    }

    public override void OnInspectorGUI()
    {
        AnimatorDotsParser animationParser = (AnimatorDotsParser)target;
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AnimatorDotsParser)target), typeof(AnimatorDotsParser), false);
        GUI.enabled = true;

        animationParser.RuntimeAnimatorController
            = EditorGUILayout.ObjectField("Parse Animator", animationParser.RuntimeAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
        if (GUILayout.Button("Parse Animator"))
        {
            RuntimeAnimatorParsedObject parsedObject = new RuntimeAnimatorParsedObject();
            parsedObject = Parse(animationParser.RuntimeAnimatorController);
            SaveAsset(animationParser.RuntimeAnimatorController, parsedObject);
        }
    }

    private RuntimeAnimatorParsedObject Parse(RuntimeAnimatorController runtimeAnimatorController)
    {
        var animatorInstanceId = runtimeAnimatorController.GetInstanceID();
        List<AnimationBuffer> animationsTable = new List<AnimationBuffer>();
        List<AnimationCurveBuffer> animationCurveTable = new List<AnimationCurveBuffer>();
        List<AnimationCurveKeyBuffer> animationCurveKeyTable = new List<AnimationCurveKeyBuffer>();
        List<AnimatorParameter> animatorParametersTable = new List<AnimatorParameter>();
        List<AnimatorLayerBuffer> animatorLayersTable = new List<AnimatorLayerBuffer>();
        List<LayerStateBuffer> layerStatesTable = new List<LayerStateBuffer>();
        List<StateTransitionBuffer> transitionsTable = new List<StateTransitionBuffer>();
        List<TransitionCondtion> transitionCondtionsTable = new List<TransitionCondtion>();

        // animation clips
        for (int animationIndex = 0; animationIndex < runtimeAnimatorController.animationClips.Length; animationIndex++)
        {
            AnimationClip animationClip = runtimeAnimatorController.animationClips[animationIndex];
            var animationId = animationClip.GetInstanceID();
            var animationItem = new AnimationBuffer
            {
                Id = animationId,
                Looped = animationClip.isLooping,
                AnimatorInstanceId = animatorInstanceId,
                Length = animationClip.length,
            };
            animationsTable.Add(animationItem);
            // animation clip data
            // bindings
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            for (int curveIndex = 0; curveIndex < bindings.Length; curveIndex++)
            {
                var curveBinding = bindings[curveIndex];
                AnimationCurveBuffer curveItem = new AnimationCurveBuffer
                {
                    Id = curveIndex,
                    AnimatorInstanceId = animatorInstanceId,
                    AnimationId = animationId,
                    Path = curveBinding.path,
                    PropertyName = curveBinding.propertyName
                };
                animationCurveTable.Add(curveItem);
                // keyframes
                var keyFrames = AnimationUtility.GetEditorCurve(animationClip, curveBinding).keys;
                for (int keyFrameIndex = 0; keyFrameIndex < keyFrames.Length; keyFrameIndex++)
                {
                    Keyframe keyFrameItem = keyFrames[keyFrameIndex];
                    AnimationCurveKeyBuffer curveKeyItem = new AnimationCurveKeyBuffer
                    {
                        Id = keyFrameIndex,
                        AnimatorInstanceId = animatorInstanceId,
                        CurveId = curveIndex,
                        Time = keyFrameItem.time,
                        Value = keyFrameItem.value,
                    };
                    animationCurveKeyTable.Add(curveKeyItem);
                }
            }
        }

        // animator parameters
        var animatorController = runtimeAnimatorController as AnimatorController;
        var parameters = animatorController.parameters;
        for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            UnityEngine.AnimatorControllerParameter parameter = parameters[parameterIndex];
            var parameterItem = new AnimatorParameter
            {
                Id = parameterIndex,
                AnimatorInstanceId = animatorInstanceId,
                ParameterName = parameter.name,
                Type = parameter.type,
                DefaultBool = parameter.defaultBool,
                DefaultFloat = parameter.defaultFloat,
                DefaultInt = parameter.defaultInt
            };
            animatorParametersTable.Add(parameterItem);
        }

        // animator layers
        var layers = animatorController.layers;
        for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            UnityEditor.Animations.AnimatorControllerLayer layer = layers[layerIndex];
            var layerItem = new AnimatorLayerBuffer
            {
                Id = layerIndex,
                AnimatorInstanceId = animatorInstanceId,
                DefaultWeight = layer.defaultWeight,
            };
            animatorLayersTable.Add(layerItem);
            // TO DO avatar mask filters
            // state machine and states per layers
            var states = layer.stateMachine.states;
            for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
            {
                UnityEditor.Animations.ChildAnimatorState childAnimatorState = states[stateIndex];
                int relevantAnimationIndex = -1;
                foreach (var animationClip in animationsTable)
                {
                    if (animationClip.Id == childAnimatorState.state.motion.GetInstanceID())
                    {
                        relevantAnimationIndex = animationClip.Id;
                        break;
                    }
                }
                bool defaultState = layer.stateMachine.defaultState == childAnimatorState.state;
                var layerStatItem = new LayerStateBuffer
                {
                    Id = stateIndex,
                    AnimatorInstanceId = animatorInstanceId,
                    LayerId = layerIndex,
                    AnimationClipId = relevantAnimationIndex,
                    DefaultState = defaultState,
                    Speed = childAnimatorState.state.speed
                };
                layerStatesTable.Add(layerStatItem);

                // transitions
                var transitions = childAnimatorState.state.transitions;
                for (int transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
                {
                    UnityEditor.Animations.AnimatorStateTransition animatorStateTransition = transitions[transitionIndex];
                    var stateTransitionItem = new StateTransitionBuffer
                    {
                        Id = transitionIndex,
                        AnimatorInstanceId = animatorInstanceId,
                        StateId = stateIndex,
                        DestinationStateId = -1,
                        HasExitTime = animatorStateTransition.hasExitTime,
                        ExitTime = animatorStateTransition.exitTime,
                        TransitionDuration = animatorStateTransition.duration,
                        TransitionOffset = animatorStateTransition.offset
                    };
                    transitionsTable.Add(stateTransitionItem);

                    // transition conditions
                    var conditions = animatorStateTransition.conditions;
                    for (int condtionIndex = 0; condtionIndex < conditions.Length; condtionIndex++)
                    {
                        UnityEditor.Animations.AnimatorCondition animatorCondition = conditions[condtionIndex];
                        var transitionConditionItem = new TransitionCondtion
                        {
                            Id = condtionIndex,
                            AnimatorInstanceId = animatorInstanceId,
                            TransitionId = transitionIndex,
                            Mode = (AnimatorTransitionConditionMode)animatorCondition.mode,
                            Parameter = animatorCondition.parameter,
                            Treshold = animatorCondition.threshold
                        };
                        transitionCondtionsTable.Add(transitionConditionItem);
                    }
                }
            }
            // settings destination ids
            for (int i = 0; i < transitionsTable.Count; i++)
            {
                var _transition = transitionsTable[i];
                foreach (var _layer in layerStatesTable)
                {
                    if (_transition.DestinationStateId == _layer.Id)
                    {
                        _transition.DestinationStateId = _layer.Id;
                        transitionsTable[i] = _transition;
                        break;
                    }
                }
            }
        }
        // result
        RuntimeAnimatorParsedObject result = new RuntimeAnimatorParsedObject();
        result.AssetInstanceId = animatorInstanceId;
        result.AnimatorName = runtimeAnimatorController.name;
        result.Animations = animationsTable;
        result.AnimationKeys = PrepareAnimationKeys(animationCurveTable, animationCurveKeyTable);
        result.AnimatorLayers = animatorLayersTable;
        result.AnimatorParameters = animatorParametersTable;
        result.LayerStates = layerStatesTable;
        result.StateTransitions = transitionsTable;
        result.TransitionCondtions = transitionCondtionsTable;
        return result;
    }

    private List<AnimationKey> PrepareAnimationKeys(List<AnimationCurveBuffer> curves, List<AnimationCurveKeyBuffer> keys)
    {
        List<AnimationKey> result = new List<AnimationKey>();

        // animator instances
        var animatorIdsList = curves.Select(i => i.AnimatorInstanceId).Distinct().ToList();
        // animation instances
        var animationIdsList = curves.Select(i => i.AnimationId).Distinct().ToList();
        // animation paths
        var animationPathsList = curves.Select(i => i.Path).Distinct().ToList();
        // animation properties
        var animationPathPropertiesList = curves.Select(i => i.PropertyName).Distinct().ToList();
        // times
        var times = keys.Select(i => i.Time).Distinct().ToList();
        times = times.OrderBy(i => i).ToList();

        var selectedCurves = new List<AnimationCurveBuffer>();
        var selectedKeys = new List<AnimationCurveKeyBuffer>();
        var selectedCurveIds = new List<int>();
        float3 positionValue = float3.zero;
        float4 rotationValue = float4.zero;
        float4 rotationEulerValue = float4.zero;
        bool positionEngaged = false;
        bool rotationEngaged = false;
        bool rotationEulerEngaged = false;
        var animationKey = new AnimationKey();
        // animators
        foreach (var animatorId in animatorIdsList)
        {
            // animations
            foreach (var animationId in animationIdsList)
            {
                // paths
                foreach (var path in animationPathsList)
                {
                    // times
                    foreach (var time in times)
                    {
                        positionEngaged = false;
                        rotationEngaged = false;
                        rotationEulerEngaged = false;
                        positionValue = float3.zero;
                        rotationValue = new float4(0f, 0f, 0f, 1f);
                        rotationEulerValue = new float4(0f, 0f, 0f, 1f);
                        // now try to collect all properties per this time
                        // properties
                        foreach (var property in animationPathPropertiesList)
                        {
                            // selected curve Ids
                            selectedCurveIds.Clear();
                            selectedCurveIds = curves.Where(
                                i => i.AnimatorInstanceId == animatorId
                                && i.AnimationId == animationId
                                && i.Path == path
                                && i.PropertyName == property).ToList().Select(i => i.Id).ToList();
                            // curves per time and propery
                            foreach (var curveId in selectedCurveIds)
                            {
                                // selected keys
                                selectedKeys.Clear();
                                selectedKeys = keys.Where(
                                    i => i.Time == time
                                    && i.CurveId == curveId
                                    && i.AnimatorInstanceId == animatorId).ToList();
                                // keys
                                foreach (var key in selectedKeys)
                                {
                                    CollectPositionAndRotation(
                                        ref positionEngaged,
                                        ref rotationEngaged,
                                        ref rotationEulerEngaged,
                                        ref positionValue,
                                        ref rotationValue,
                                        ref rotationEulerValue,
                                        property,
                                        key.Value);
                                }
                            }
                        }
                        // check if properties collected
                        if (!positionEngaged && !rotationEngaged)
                        {
                            continue;
                        }
                        animationKey = new AnimationKey
                        {
                            AnimationId = animationId,
                            AnimatorInstanceId = animatorId,
                            PositionEngaged = positionEngaged,
                            RotationEngaged = rotationEngaged,
                            RotationEulerEngaged = rotationEulerEngaged,
                            PositionValue = positionValue,
                            RotationValue = rotationValue,
                            RotationEulerValue = rotationEulerValue,
                            Path = path.ToString(),
                            Time = time,
                        };
                        result.Add(animationKey);
                    }
                }
            }
        }
        return result;
    }

    private void CollectPositionAndRotation(
        ref bool positionEngaged,
        ref bool rotationEngaged,
        ref bool rotationEulerEngaged,
        ref float3 position,
        ref float4 rotation,
        ref float4 rotationEuler,
        string propertyName,
        float value)
    {
        if (propertyName == _posLocalx)
        {
            position.x = value;
            positionEngaged = true;
        }
        if (propertyName == _posLocaly)
        {
            position.y = value;
            positionEngaged = true;
        }
        if (propertyName == _posLocalz)
        {
            position.z = value;
            positionEngaged = true;
        }
        if (propertyName == _rotLocalx)
        {
            rotation.x = value;
            rotationEngaged = true;
        }
        if (propertyName == _rotLocaly)
        {
            rotation.y = value;
            rotationEngaged = true;
        }
        if (propertyName == _rotLocalz)
        {
            rotation.z = value;
            rotationEngaged = true;
        }
        if (propertyName == _rotLocalw)
        {
            rotation.w = value;
            rotationEngaged = true;
        }
        if (propertyName == _rotLocalEulerx)
        {
            rotationEuler.x = value;
            rotationEulerEngaged = true;
        }
        if (propertyName == _rotLocalEulery)
        {
            rotationEuler.y = value;
            rotationEulerEngaged = true;
        }
        if (propertyName == _rotLocalEulerz)
        {
            rotationEuler.z = value;
            rotationEulerEngaged = true;
        }
        if (propertyName == _rotLocalEulerw)
        {
            rotationEuler.w = value;
            rotationEulerEngaged = true;
        }
    }

    private void SaveAsset(RuntimeAnimatorController runtimeAnimatorController, RuntimeAnimatorParsedObject parsedObject)
    {
        var instanceId = runtimeAnimatorController.GetInstanceID();
        var asset = ScriptableObject.CreateInstance<AnimatorDotsAsset>();
        asset.AnimatorInstanceId = instanceId;
        asset.RuntimeAnimatorParsedObject = parsedObject;
        asset.AnimatorName = runtimeAnimatorController.name;
        var assetPath = AssetDatabase.GetAssetPath(instanceId);
        assetPath = assetPath.Replace(".controller", "DOTS.asset");
        AssetDatabase.CreateAsset(asset, assetPath);
    }
}

