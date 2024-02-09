using System.Collections.Generic;
using UnityEngine;

public class AnimatorDotsParser : MonoBehaviour
{
    public RuntimeAnimatorController RuntimeAnimatorController;
}


[System.Serializable]
public class RuntimeAnimatorParsedObject
{
    public int AssetInstanceId;
    public string AnimatorName;
    public List<AnimationBuffer> AnimationBuffer;
    public List<AnimationCurveBuffer> AnimationCurveBuffer;
    public List<AnimationCurveKeyBuffer> AnimationCurveKeyBuffer;
    public List<AnimatorLayerBuffer> AnimatorLayerBuffer;
    public List<LayerStateBuffer> LayerStateBuffer;
    public List<StateTransitionBuffer> StateTransitionBuffer;
    public List<TransitionCondtionBuffer> TransitionCondtionBuffer;
    public List<AnimatorParametersBuffer> AnimatorParametersBuffer;
}
