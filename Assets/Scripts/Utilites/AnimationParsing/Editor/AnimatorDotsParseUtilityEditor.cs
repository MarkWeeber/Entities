using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(AnimatorDotsParser))]
public class AnimatorDotsParseUtilityEditor : Editor
{
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
        List<AnimatorParametersBuffer> animatorParametersTable = new List<AnimatorParametersBuffer>();
        List<AnimatorLayerBuffer> animatorLayersTable = new List<AnimatorLayerBuffer>();
        List<LayerStateBuffer> layerStatesTable = new List<LayerStateBuffer>();
        List<StateTransitionBuffer> transitionsTable = new List<StateTransitionBuffer>();
        List<TransitionCondtionBuffer> transitionCondtionsTable = new List<TransitionCondtionBuffer>();

        // animation clips
        for (int animationIndex = 0; animationIndex < runtimeAnimatorController.animationClips.Length; animationIndex++)
        {
            AnimationClip animationClip = runtimeAnimatorController.animationClips[animationIndex];
            var animationItem = new AnimationBuffer
            {
                Id = animationIndex,
                Name = (FixedString32Bytes)animationClip.name,
                Looped = animationClip.isLooping,
                AnimatorInstanceId = animatorInstanceId
            };
            animationsTable.Add(animationItem);

            // animation clip curve data
#pragma warning disable CS0618
            var curveData = AnimationUtility.GetAllCurves(animationClip, includeCurveData: true);
#pragma warning restore CS0618
            for (int curveIndex = 0; curveIndex < curveData.Length; curveIndex++)
            {
                AnimationClipCurveData animationClipCurveItem = curveData[curveIndex];
                AnimationCurveBuffer cureItem = new AnimationCurveBuffer
                {
                    Id = curveIndex,
                    AnimatorInstanceId = animatorInstanceId,
                    AnimationId = animationIndex,
                    Path = (FixedString512Bytes)animationClipCurveItem.path,
                    PropertyName = (FixedString32Bytes)animationClipCurveItem.propertyName
                };
                animationCurveTable.Add(cureItem);

                // curve keys
                var keys = animationClipCurveItem.curve.keys;
                for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                {
                    Keyframe keyFrameItem = keys[keyIndex];
                    AnimationCurveKeyBuffer curveKeyItem = new AnimationCurveKeyBuffer
                    {
                        Id = keyIndex,
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
            var parameterItem = new AnimatorParametersBuffer
            {
                Id = parameterIndex,
                AnimatorInstanceId = animatorInstanceId,
                ParameterName = (FixedString32Bytes)parameter.name,
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
                    if (animationClip.Name == (FixedString32Bytes)childAnimatorState.state.motion.name)
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
                    StateName = (FixedString32Bytes)childAnimatorState.state.name,
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
                        DestinationStateName = (FixedString32Bytes)animatorStateTransition.destinationState.name,
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
                        var transitionConditionItem = new TransitionCondtionBuffer
                        {
                            Id = condtionIndex,
                            AnimatorInstanceId = animatorInstanceId,
                            TransitionId = transitionIndex,
                            Mode = (AnimatorTransitionConditionMode)animatorCondition.mode,
                            Parameter = (FixedString32Bytes)animatorCondition.parameter,
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
                    if (_transition.DestinationStateName == _layer.StateName)
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
        result.AnimationBuffer = animationsTable;
        result.AnimationCurveBuffer = animationCurveTable;
        result.AnimationCurveKeyBuffer = animationCurveKeyTable;
        result.AnimatorLayerBuffer = animatorLayersTable;
        result.AnimatorParametersBuffer = animatorParametersTable;
        result.LayerStateBuffer = layerStatesTable;
        result.StateTransitionBuffer = transitionsTable;
        result.TransitionCondtionBuffer = transitionCondtionsTable;
        return result;
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

