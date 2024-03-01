//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using ParseUtils;
//using UnityEditor;

//namespace ParseUtils
//{
//    public class AnimationCurveBuilderTest : MonoBehaviour
//    {
//        [SerializeField] AnimationClip clip;

//        [SerializeField] AnimationCurve curve;

//        void Start()
//        {
//            curve = new AnimationCurve();
//            var keyframes = GetEditorKeyFramesFirst(clip);
//            foreach (var keyfrmae in keyframes)
//            {
//                curve.AddKey(keyfrmae);
//            }
//        }

//        public static Keyframe[] GetEditorKeyFramesFirst(AnimationClip animationClip)
//        {
//            var bindings = AnimationUtility.GetCurveBindings(animationClip);
//            var keyFrames = AnimationUtility.GetEditorCurve(animationClip, bindings[0]).keys;
//            return keyFrames;
//        }
//    }
//}