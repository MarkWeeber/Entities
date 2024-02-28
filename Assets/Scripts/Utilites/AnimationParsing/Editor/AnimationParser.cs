using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ParseUtils
{
    public class AnimationParser
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

        public static AnimationClipParsedObject PrepareAnimation(AnimationClip animationClip, int animatorInstanceId, List<string> paths)
        {
            var result = new AnimationClipParsedObject();
            result.Positions = new List<AnimationPositioItem>();
            result.Rotations = new List<AnimationRotationItem>();
            var animationId = animationClip.GetInstanceID();
            result.Id = animationId;
            result.AnimationName = animationClip.name;
            result.AnimatorInstanceId = animatorInstanceId;
            var animationCurveTable = new List<AnimationCurveItem>();
            var animationCurveKeyTable = new List<AnimationCurveKeyItem>();
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            // bindings
            for (int curveIndex = 0; curveIndex < bindings.Length; curveIndex++)
            {
                var curveBinding = bindings[curveIndex];
                AnimationCurveItem curveItem = new AnimationCurveItem
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
                    AnimationCurveKeyItem curveKeyItem = new AnimationCurveKeyItem
                    {
                        Id = keyFrameIndex,
                        AnimatorInstanceId = animatorInstanceId,
                        CurveId = curveIndex,
                        Time = keyFrameItem.time,
                        Value = keyFrameItem.value,
                        AnimationId = animationId,
                    };
                    animationCurveKeyTable.Add(curveKeyItem);
                }
            }
            PrepareAnimationKeys(animationCurveTable, animationCurveKeyTable, result.Positions, result.Rotations);
            SaveAnimationAsset(animationClip, result);
            return result;
        }

        private static void PrepareAnimationKeys(
            List<AnimationCurveItem> curves,
            List<AnimationCurveKeyItem> keys,
            List<AnimationPositioItem> animationPositions,
            List<AnimationRotationItem> animationRotations)
        {
            List<AnimationKeyItem> result = new List<AnimationKeyItem>();
            List<AnimationKeyPreProcess> preProcess = new List<AnimationKeyPreProcess>();
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

            var selectedCurves = new List<AnimationCurveItem>();
            var selectedKeys = new List<AnimationCurveKeyItem>();
            var selectedCurveIds = new List<int>();
            float3 positionValue = float3.zero;
            float4 rotationValue = float4.zero;
            float4 rotationEulerValue = float4.zero;
            bool positionEngaged = false;
            bool rotationEngaged = false;
            bool rotationEulerEngaged = false;
            var animationKey = new AnimationKeyPreProcess();
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
                                        && i.AnimatorInstanceId == animatorId
                                        && i.AnimationId == animationId).ToList();
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
                            animationKey = new AnimationKeyPreProcess
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
                            preProcess.Add(animationKey);
                        }
                    }
                }
            }
            ProcessKeys(preProcess, animationPositions, animationRotations);
            animationPositions = animationPositions.OrderBy(i => i.Time).ToList();
            animationRotations = animationRotations.OrderBy(i => i.Time).ToList();
            //result = result.OrderBy(x => x.Time).ToList();
        }

        private static void CollectPositionAndRotation(
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

        private static void ProcessKeys(
    List<AnimationKeyPreProcess> animationKeyPreProcesses,
    List<AnimationPositioItem> animationPositions,
    List<AnimationRotationItem> animationRotations)
        {
            quaternion rotationValue = quaternion.identity;
            foreach (var preProcess in animationKeyPreProcesses)
            {
                rotationValue = quaternion.identity;
                if (preProcess.RotationEulerEngaged)
                {
                    rotationValue = quaternion.Euler(
                                    math.radians(preProcess.RotationEulerValue.x),
                                    math.radians(preProcess.RotationEulerValue.y),
                                    math.radians(preProcess.RotationEulerValue.z));
                    animationRotations.Add(new AnimationRotationItem
                    {
                        AnimationId = preProcess.AnimationId,
                        Path = preProcess.Path,
                        Time = preProcess.Time,
                        Value = rotationValue
                    });
                }
                if (preProcess.RotationEngaged)
                {
                    rotationValue = new quaternion(preProcess.RotationValue);
                    animationRotations.Add(new AnimationRotationItem
                    {
                        AnimationId = preProcess.AnimationId,
                        Path = preProcess.Path,
                        Time = preProcess.Time,
                        Value = rotationValue
                    });
                }
                if (preProcess.PositionEngaged)
                {
                    animationPositions.Add(new AnimationPositioItem
                    {
                        AnimationId = preProcess.AnimationId,
                        Path = preProcess.Path,
                        Time = preProcess.Time,
                        Value = preProcess.PositionValue
                    });
                }
            }
        }

        private static void SaveAnimationAsset(AnimationClip animationClip, AnimationClipParsedObject parsedObject)
        {
            var asset = ScriptableObject.CreateInstance<AnimationDotsAsset>();
            asset.AnimationClipParsedObject = new AnimationClipParsedObject
            {
                Rotations = parsedObject.Rotations,
                Positions = parsedObject.Positions,
                AnimationName = parsedObject.AnimationName,
                Id = parsedObject.Id,
                AnimatorInstanceId = parsedObject.AnimatorInstanceId,
                Length = animationClip.length,
                Looped = animationClip.isLooping
            };
            var instanceId = animationClip.GetInstanceID();
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            assetPath = assetPath.Replace(".anim", "DOTS_Anim.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        public static Keyframe[] GetEditorKeyFramesFirst(AnimationClip animationClip)
        {
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            var keyFrames = AnimationUtility.GetEditorCurve(animationClip, bindings[0]).keys;
            return keyFrames;
        }
    }
}