using UnityEditor;
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
            animationParser.PartsAnimationMethod
                = (PartsAnimationMethod) EditorGUILayout.EnumPopup("Interpolation Method", animationParser.PartsAnimationMethod);
            if (GUILayout.Button("Parse Animator"))
            {
                RuntimeAnimatorParsedObject parsedObject = new RuntimeAnimatorParsedObject();
                parsedObject = ParseTools.PrepareAnimatorAsset(animationParser.RuntimeAnimatorController, animationParser.FPS, animationParser.PartsAnimationMethod);
                ParseTools.SaveAnimatorAsset(animationParser.RuntimeAnimatorController, parsedObject);
            }
        }
    }

}