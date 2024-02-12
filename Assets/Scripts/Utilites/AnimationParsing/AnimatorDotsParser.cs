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
    public List<AnimationBuffer> Animations;
    public List<AnimationKey> AnimationKeys;
    public List<AnimatorLayerBuffer> AnimatorLayers;
    public List<LayerStateBuffer> LayerStates;
    public List<StateTransitionBuffer> StateTransitions;
    public List<TransitionCondtion> TransitionCondtions;
    public List<AnimatorParameter> AnimatorParameters;
}
