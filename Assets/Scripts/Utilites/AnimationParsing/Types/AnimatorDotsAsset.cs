using System.Collections.Generic;
using UnityEngine;

namespace ParseUtils
{
    [CreateAssetMenu(fileName = "New Dots Animator Asset", menuName = "Custom Assets/DOTS Animator")]
    public class AnimatorDotsAsset : ScriptableObject
    {
        public int AnimatorInstanceId;
        public string AnimatorName;
        public RuntimeAnimatorParsedObject RuntimeAnimatorParsedObject;
    }
    [System.Serializable]
    public class RuntimeAnimatorParsedObject
    {
        public int AssetInstanceId;
        public string AnimatorName;
        public List<AnimationBuffer> Animations;
        public List<AnimatorLayerBuffer> AnimatorLayers;
        public List<LayerStateBuffer> LayerStates;
        public List<StateTransitionBuffer> StateTransitions;
        public List<TransitionCondtion> TransitionCondtions;
        public List<AnimatorParameter> AnimatorParameters;
    }
}