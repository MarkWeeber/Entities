using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationParser))]
public class AnimationParseUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AnimationParser animationParser = (AnimationParser)target;
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AnimationParser)target), typeof(AnimationParser), false);
        GUI.enabled = true;
        animationParser.AnimationClip = EditorGUILayout.ObjectField("Object", animationParser.AnimationClip, typeof(AnimationClip), false) as AnimationClip;
        if (GUILayout.Button("CHECK ANIMATION"))
        {
            animationParser.Parse();
        }
    }


}
