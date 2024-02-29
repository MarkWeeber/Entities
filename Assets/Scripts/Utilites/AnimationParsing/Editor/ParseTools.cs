using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ParseUtils
{
    public class ParseTools
    {
        public static RuntimeAnimatorParsedObject PrepareAnimatorAsset(RuntimeAnimatorController runtimeAnimatorController)
        {
            var animatorInstanceId = runtimeAnimatorController.GetInstanceID();
            var animationsTable = new List<AnimationItem>();
            var animatorParametersTable = new List<AnimatorParameter>();
            var animatorLayersTable = new List<AnimatorLayerItem>();
            var layerStatesTable = new List<LayerStateBuffer>();
            var transitionsTable = new List<StateTransitionBuffer>();
            var transitionCondtionsTable = new List<TransitionCondtion>();
            var animationParsedObjects = new List<AnimationClipParsedObject>();
            var paths = new List<string>();

            // animation clips
            for (int animationIndex = 0; animationIndex < runtimeAnimatorController.animationClips.Length; animationIndex++)
            {
                var animationClip = runtimeAnimatorController.animationClips[animationIndex];
                var animationItem = new AnimationItem
                {
                    AnimationInstanceId = animationClip.GetInstanceID(),
                    Looped = animationClip.isLooping,
                    AnimatorInstanceId = animatorInstanceId,
                    Length = animationClip.length,
                    Name = animationClip.name
                };
                animationsTable.Add(animationItem);
                PreparePathsFromAllAnimations(animationClip, paths);

                //animationParsedObjects.Add(AnimationParser.PrepareAnimation(animationClip, animatorInstanceId, paths));
                var parsedAnimationClipObject = AnimationParser.GetAnimationParsedObject(animationClip, animatorInstanceId, paths);
                animationParsedObjects.Add(parsedAnimationClipObject);
            }

            // animator parameters
            var animatorController = runtimeAnimatorController as AnimatorController;
            var parameters = animatorController.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var parameter = parameters[parameterIndex];
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
                var layer = layers[layerIndex];
                var layerId = layer.stateMachine.GetInstanceID();
                var layerItem = new AnimatorLayerItem
                {
                    Id = layerId,
                    AnimatorInstanceId = animatorInstanceId,
                    DefaultWeight = layer.defaultWeight,
                };
                animatorLayersTable.Add(layerItem);
                // TO DO avatar mask filters
                // state machine and states per layers
                var states = layer.stateMachine.states;
                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    var state = states[stateIndex].state;
                    int stateId = state.GetInstanceID();
                    bool defaultState = layer.stateMachine.defaultState == state;
                    int animationClipId = state.motion.GetInstanceID();
                    int animationBlobAssetIndex = -1;
                    for (int i = 0; i < animationParsedObjects.Count; i++)
                    {
                        if (animationParsedObjects[i].Id == animationClipId)
                        {
                            animationBlobAssetIndex = i;
                            break;
                        }
                    }
                    var layerStatItem = new LayerStateBuffer
                    {
                        Id = stateId,
                        AnimatorInstanceId = animatorInstanceId,
                        LayerId = layerId,
                        AnimationClipId = animationClipId,
                        DefaultState = defaultState,
                        Speed = state.speed,
                        AnimationLength = state.motion.averageDuration,
                        AnimationLooped = state.motion.isLooping,
                        AnimationBlobAssetIndex = animationBlobAssetIndex
                    };
                    layerStatesTable.Add(layerStatItem);

                    // transitions
                    var transitions = state.transitions;
                    for (int transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
                    {
                        AnimatorStateTransition animatorStateTransition = transitions[transitionIndex];
                        var transitionId = animatorStateTransition.GetInstanceID();
                        var destinationStateId = animatorStateTransition.destinationState.GetInstanceID();
                        var stateTransitionItem = new StateTransitionBuffer
                        {
                            Id = transitionId,
                            AnimatorInstanceId = animatorInstanceId,
                            StateId = stateId,
                            DestinationStateId = destinationStateId,
                            FixedDuration = animatorStateTransition.hasExitTime,
                            ExitTime = animatorStateTransition.exitTime,
                            TransitionDuration = animatorStateTransition.duration,
                            TransitionOffset = animatorStateTransition.offset
                        };
                        transitionsTable.Add(stateTransitionItem);

                        // transition conditions
                        var conditions = animatorStateTransition.conditions;
                        for (int condtionIndex = 0; condtionIndex < conditions.Length; condtionIndex++)
                        {
                            AnimatorCondition animatorCondition = conditions[condtionIndex];
                            var transitionConditionItem = new TransitionCondtion
                            {
                                Id = condtionIndex,
                                AnimatorInstanceId = animatorInstanceId,
                                TransitionId = transitionId,
                                Mode = (AnimatorTransitionConditionMode)animatorCondition.mode,
                                Parameter = animatorCondition.parameter,
                                Treshold = animatorCondition.threshold
                            };
                            transitionCondtionsTable.Add(transitionConditionItem);
                        }
                    }
                }
            }
            // result
            RuntimeAnimatorParsedObject result = new RuntimeAnimatorParsedObject
            {
                AssetInstanceId = animatorInstanceId,
                AnimatorName = runtimeAnimatorController.name,
                Animations = animationsTable,
                AnimatorLayers = animatorLayersTable,
                AnimatorParameters = animatorParametersTable,
                LayerStates = layerStatesTable,
                StateTransitions = transitionsTable,
                TransitionCondtions = transitionCondtionsTable,
                Paths = paths
            };
            return result;
        }
        
        private static void PreparePathsFromAllAnimations(AnimationClip animationClip, List<string> paths)
        {
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            for (int curveIndex = 0; curveIndex < bindings.Length; curveIndex++)
            {
                var path = bindings[curveIndex].path;
                if (!paths.Contains(path))
                {
                    paths.Add(path);
                }
            }
        }

        public static void SaveAnimatorAsset(RuntimeAnimatorController runtimeAnimatorController, RuntimeAnimatorParsedObject parsedObject)
        {
            var instanceId = runtimeAnimatorController.GetInstanceID();
            var asset = ScriptableObject.CreateInstance<AnimatorDotsAsset>();
            asset.AnimatorInstanceId = instanceId;
            asset.RuntimeAnimatorParsedObject = parsedObject;
            asset.AnimatorName = runtimeAnimatorController.name;
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            assetPath = assetPath.Replace(".controller", "DOTS_Controller.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }
}
