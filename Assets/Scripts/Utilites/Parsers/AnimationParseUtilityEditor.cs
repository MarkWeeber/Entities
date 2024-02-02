using UnityEditor;
using UnityEditor.Animations;
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
        animationParser.AnimationClip = EditorGUILayout.ObjectField("Animation Clip", animationParser.AnimationClip, typeof(AnimationClip), false) as AnimationClip;
        if (GUILayout.Button("CHECK ANIMATION"))
        {
            animationParser.ParseAnimation();
        }
        animationParser.AnimatorController 
            = EditorGUILayout.ObjectField("Animator Controller", animationParser.AnimatorController, typeof(AnimatorController), false) as AnimatorController;
        if (GUILayout.Button("CHECK ANIMATOR CONTROLLER"))
        {
            animationParser.ParseAnimatorController();
        }
        animationParser.Animator
            = EditorGUILayout.ObjectField("Animator", animationParser.Animator, typeof(Animator), true) as Animator;
        if (GUILayout.Button("CHECK ANIMATOR"))
        {
            animationParser.ParseAnimator();
        }
    }


}
