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
        public int FPS;
        public List<AnimationItem> Animations;
        public List<AnimatorLayerItem> AnimatorLayers;
        public List<LayerStateBuffer> LayerStates;
        public List<StateTransitionBuffer> StateTransitions;
        public List<AnyStateTransitionBuffer> AnyStateTransitions;
        public List<TransitionCondtion> TransitionCondtions;
        public List<AnimatorParameter> AnimatorParameters;
        public List<string> Paths;
    }
}