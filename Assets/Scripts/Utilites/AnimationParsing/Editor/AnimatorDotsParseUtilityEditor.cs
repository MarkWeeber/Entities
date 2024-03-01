using Codice.Client.Common;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ParseUtils
{
    [CustomEditor(typeof(AnimatorDotsParser))]
    public class AnimatorDotsParseUtilityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AnimatorDotsParser animationParser = (AnimatorDotsParser)target;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AnimatorDotsParser)target), typeof(AnimatorDotsParser), false);
            GUI.enabled = true;

            animationParser.RuntimeAnimatorController
                = EditorGUILayout.ObjectField("Parse Animator", animationParser.RuntimeAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
            animationParser.FPS
                = EditorGUILayout.IntField("Animation FPS count", animationParser.FPS);
            if (GUILayout.Button("Parse Animator"))
            {
                RuntimeAnimatorParsedObject parsedObject = new RuntimeAnimatorParsedObject();
                parsedObject = ParseTools.PrepareAnimatorAsset(animationParser.RuntimeAnimatorController, animationParser.FPS);
                ParseTools.SaveAnimatorAsset(animationParser.RuntimeAnimatorController, parsedObject);
            }
        }
    }

}