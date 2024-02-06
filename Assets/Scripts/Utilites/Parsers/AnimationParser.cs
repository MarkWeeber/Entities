using UnityEditor;
using UnityEngine;
public class AnimationParser : MonoBehaviour
{
    public AnimationClip AnimationClip;
    public UnityEditor.Animations.AnimatorController AnimatorController;
    public Animator Animator;

    public void ParseAnimation()
    {
        if (AnimationClip != null)
        {
            Debug.Log(AnimationClip.name);
            EditorCurveBinding [] editorCurveBindings = AnimationUtility.GetCurveBindings(AnimationClip);
            EditorCurveBinding[] editorCurveBindings2 = AnimationUtility.GetObjectReferenceCurveBindings(AnimationClip);
            foreach (EditorCurveBinding curveBinding in editorCurveBindings2)
            {
                //Debug.Log(curveBinding.path + " " + curveBinding.propertyName + " " + curveBinding.type);
            }
#pragma warning disable CS0618 // Тип или член устарел
            foreach (AnimationClipCurveData item in AnimationUtility.GetAllCurves(AnimationClip, includeCurveData: true))
            {
                Debug.Log("Path: " + item.path + " Propery: " + item.propertyName);
                foreach (Keyframe keyframe in item.curve.keys)
                {
                    //Debug.Log("time: " + keyframe.time + " value: " + keyframe.value);
                }
            }
#pragma warning restore CS0618 // Тип или член устарел
        }
    }

    public void ParseAnimatorController()
    {
        if (AnimatorController != null)
        {
            Debug.Log(AnimatorController.name);
            foreach (AnimatorControllerParameter parameter in AnimatorController.parameters)
            {
                Debug.Log($"name: {parameter.name} type: {parameter.type} defaultbool: {parameter.defaultBool}");
            }
            var stateMachine = AnimatorController.layers[0].stateMachine;
            foreach (UnityEditor.Animations.ChildAnimatorState state in stateMachine.states)
            {
                UnityEditor.Animations.AnimatorState animatorState = state.state;
                Motion motion = animatorState.motion;
                Debug.Log(motion.name);
                foreach (UnityEditor.Animations.AnimatorStateTransition stateTransition in animatorState.transitions)
                {
                    Debug.Log($"transition name: {stateTransition.destinationState}");
                    foreach (UnityEditor.Animations.AnimatorCondition animatorCondition in stateTransition.conditions)
                    {
                        Debug.Log($"parameter: {animatorCondition.parameter} mode: {animatorCondition.mode} treshold {animatorCondition.threshold} ");
                    }
                }
            }
        }
    }

    public void ParseAnimator()
    {
        if (Animator != null)
        {
            foreach (AnimatorControllerParameter animatorControllerParameter in Animator.parameters)
            {
                Debug.Log(
                    $"Params, Name: {animatorControllerParameter.name} Type: {animatorControllerParameter.type} Defaultbool: {animatorControllerParameter.defaultBool}");
            }
            foreach(AnimationClip animationClip in Animator.runtimeAnimatorController.animationClips)
            {
                ParseAnimation(animationClip);
            };
            ParseAnimatorController(Animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController);
        }
    }

    private void ParseAnimation(AnimationClip animationClip)
    {
        if (animationClip != null)
        {
            Debug.Log(animationClip.name);
            EditorCurveBinding[] editorCurveBindings = AnimationUtility.GetCurveBindings(animationClip);
            EditorCurveBinding[] editorCurveBindings2 = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
            foreach (EditorCurveBinding curveBinding in editorCurveBindings2)
            {
                Debug.Log(curveBinding.path + " " + curveBinding.propertyName + " " + curveBinding.type);
            }
#pragma warning disable CS0618 // Тип или член устарел
            foreach (AnimationClipCurveData item in AnimationUtility.GetAllCurves(animationClip, includeCurveData: true))
            {
                Debug.Log("Path: " + item.path + " Propery: " + item.propertyName);
                foreach (Keyframe keyframe in item.curve.keys)
                {
                    Debug.Log("time: " + keyframe.time + " value: " + keyframe.value);
                }
            }
#pragma warning restore CS0618 // Тип или член устарел
        }
    }

    private void ParseAnimatorController(UnityEditor.Animations.AnimatorController animatorController)
    {
        Debug.Log(animatorController.name);
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        foreach (UnityEditor.Animations.ChildAnimatorState state in stateMachine.states)
        {
            UnityEditor.Animations.AnimatorState animatorState = state.state;
            Motion motion = animatorState.motion;
            Debug.Log(motion.name);
            foreach (UnityEditor.Animations.AnimatorStateTransition stateTransition in animatorState.transitions)
            {
                Debug.Log($"transition name: {stateTransition.destinationState}");
                foreach (UnityEditor.Animations.AnimatorCondition animatorCondition in stateTransition.conditions)
                {
                    Debug.Log($"parameter: {animatorCondition.parameter} mode: {animatorCondition.mode} treshold {animatorCondition.threshold} ");
                }
            }
        }
    }

}

