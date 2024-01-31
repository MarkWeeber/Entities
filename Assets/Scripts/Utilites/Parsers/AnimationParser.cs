using UnityEditor;
using UnityEngine;
public class AnimationParser : MonoBehaviour
{
    public AnimationClip AnimationClip;

    public void Parse()
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
            foreach (AnimationClipCurveData item in AnimationUtility.GetAllCurves(AnimationClip, includeCurveData: true))
            {
                Debug.Log("Path: " + item.path + " Propery: " + item.propertyName);
                foreach (Keyframe keyframe in item.curve.keys)
                {
                    //Debug.Log("time: " + keyframe.time + " value: " + keyframe.value);
                }
            }
        }
    }

}

