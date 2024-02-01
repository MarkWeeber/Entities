using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
public class AnimationParser : MonoBehaviour
{
    public AnimationClip AnimationClip;
    public AnimatorController AnimatorController;

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
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                AnimatorState animatorState = state.state;
                Motion motion = animatorState.motion;
                Debug.Log(motion.name);
                foreach (AnimatorStateTransition stateTransition in animatorState.transitions)
                {
                    Debug.Log($"transition name: {stateTransition.destinationState}");
                    foreach (AnimatorCondition animatorCondition in stateTransition.conditions)
                    {
                        Debug.Log($"parameter: {animatorCondition.parameter} mode: {animatorCondition.mode} treshold {animatorCondition.threshold} ");
                    }
                }
            }
        }
    }

}

