using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
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

        public static AnimationClipParsedObject GetAnimationParsedObject(AnimationClip animationClip, int animatorInstanceId, List<string> paths, int fps, PartsAnimationMethod partsAnimationMethod)
        {
            var result = new AnimationClipParsedObject();
            result.AnimationName = animationClip.name;
            result.Id = animationClip.GetInstanceID();
            result.AnimatorInstanceId = animatorInstanceId;
            result.Length = animationClip.length;
            result.Looped = animationClip.isLooping;
            result.PathData = new List<AnimationPathData>();
            result.FPS = fps;
            result.PartsAnimationMethod = partsAnimationMethod;
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            foreach (var path in paths)
            {
                var animationPathData = new AnimationPathData();
                animationPathData.Path = path;
                GetKeyFrames(animationClip, path, bindings, ref animationPathData, fps);
                ClearUnusedArrays(ref animationPathData);
                result.PathData.Add(animationPathData);
            }
            SaveAnimationAsset(animationClip, result);
            return result;
        }

        private static bool GetKeyFrames(
            AnimationClip animationClip,
            string path,
            EditorCurveBinding[] bidnings,
            ref AnimationPathData animationPathData,
            int fps)
        {
            bool ans = false;
            float length = animationClip.length;
            int samplesCount = (int)math.ceil(length * fps);
            animationPathData.Positions = new List<AnimationPositioItem>(new AnimationPositioItem[samplesCount + 1]);
            animationPathData.Rotations = new List<AnimationRotationItem>(new AnimationRotationItem[samplesCount + 1]);
            animationPathData.EulerRotations = new List<AnimationRotationItem>(new AnimationRotationItem[samplesCount + 1]);
            foreach (var curveBinding in bidnings)
            {
                if (curveBinding.path == path)
                {
                    var propertyName = curveBinding.propertyName;
                    var curve = AnimationUtility.GetEditorCurve(animationClip, curveBinding);
                    for (int i = 0; i <= samplesCount; i++)
                    {
                        float time = (length / samplesCount) * i;
                        float value = curve.Evaluate(time);
                        FillLists(
                            propertyName,
                            i,
                            value,
                            time,
                            ref animationPathData);
                    }
                }
            }
            return ans;
        }

        private static void FillLists(
            string propertyName,
            int index,
            float value,
            float time,
            ref AnimationPathData animationPathData
            )
        {
            if (propertyName == _posLocalx)
            {
                var item = animationPathData.Positions[index];
                item.Time = time;
                item.Value.x = value;
                animationPathData.Positions[index] = item;
                animationPathData.HasPosition = true;
            }
            if (propertyName == _posLocaly)
            {
                var item = animationPathData.Positions[index];
                item.Time = time;
                item.Value.y = value;
                animationPathData.Positions[index] = item;
                animationPathData.HasPosition = true;
            }
            if (propertyName == _posLocalz)
            {
                var item = animationPathData.Positions[index];
                item.Time = time;
                item.Value.z = value;
                animationPathData.Positions[index] = item;
                animationPathData.HasPosition = true;
            }
            if (propertyName == _rotLocalx)
            {
                var item = animationPathData.Rotations[index];
                item.Time = time;
                item.Value.value.x = value;
                animationPathData.Rotations[index] = item;
                animationPathData.HasRotation = true;
            }
            if (propertyName == _rotLocaly)
            {
                var item = animationPathData.Rotations[index];
                item.Time = time;
                item.Value.value.y = value;
                animationPathData.Rotations[index] = item;
                animationPathData.HasRotation = true;
            }
            if (propertyName == _rotLocalz)
            {
                var item = animationPathData.Rotations[index];
                item.Time = time;
                item.Value.value.z = value;
                animationPathData.Rotations[index] = item;
                animationPathData.HasRotation = true;
            }
            if (propertyName == _rotLocalw)
            {
                var item = animationPathData.Rotations[index];
                item.Time = time;
                item.Value.value.w = value;
                animationPathData.Rotations[index] = item;
                animationPathData.HasRotation = true;
            }
            if (propertyName == _rotLocalEulerx)
            {
                var item = animationPathData.EulerRotations[index];
                item.Time = time;
                item.Value.value.x = value;
                animationPathData.EulerRotations[index] = item;
                animationPathData.HasEulerRotation = true;
            }
            if (propertyName == _rotLocalEulery)
            {
                var item = animationPathData.EulerRotations[index];
                item.Time = time;
                item.Value.value.y = value;
                animationPathData.EulerRotations[index] = item;
                animationPathData.HasEulerRotation = true;
            }
            if (propertyName == _rotLocalEulerz)
            {
                var item = animationPathData.EulerRotations[index];
                item.Time = time;
                item.Value.value.z = value;
                animationPathData.EulerRotations[index] = item;
                animationPathData.HasEulerRotation = true;
            }
            if (propertyName == _rotLocalEulerw)
            {
                var item = animationPathData.EulerRotations[index];
                item.Time = time;
                item.Value.value.w = value;
                animationPathData.EulerRotations[index] = item;
                animationPathData.HasEulerRotation = true;
            }
        }

        private static void ClearUnusedArrays(ref AnimationPathData animationPathData)
        {
            if (!animationPathData.HasEulerRotation)
            {
                animationPathData.EulerRotations.Clear();
            }
            if (!animationPathData.HasRotation)
            {
                animationPathData.Rotations.Clear();
            }
            if (!animationPathData.HasPosition)
            {
                animationPathData.Positions.Clear();
            }
        }

        private static void SaveAnimationAsset(AnimationClip animationClip, AnimationClipParsedObject parsedObject)
        {
            var instanceId = animationClip.GetInstanceID();
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            assetPath = assetPath.Replace(".anim", "DOTS_Anim.asset");
            var asset = ScriptableObject.CreateInstance<AnimationDotsAsset>();
            asset.AnimationClipParsedObject = new AnimationClipParsedObject
            {
                PathData = parsedObject.PathData,
                AnimationName = parsedObject.AnimationName,
                Id = parsedObject.Id,
                AnimatorInstanceId = parsedObject.AnimatorInstanceId,
                Length = animationClip.length,
                Looped = animationClip.isLooping,
                FPS = parsedObject.FPS,
                PartsAnimationMethod = parsedObject.PartsAnimationMethod,
                EventsData = parsedObject.EventsData,
            };
            var loadedAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationDotsAsset)) as AnimationDotsAsset;
            if (loadedAsset != null)
            {
                loadedAsset.AnimationClipParsedObject = asset.AnimationClipParsedObject;
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(asset, assetPath);
            }
        }
    }
}