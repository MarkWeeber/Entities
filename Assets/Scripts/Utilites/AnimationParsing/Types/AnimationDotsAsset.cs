using System.Collections.Generic;
using UnityEngine;

namespace ParseUtils
{
    [CreateAssetMenu(fileName = "New Dots Animation Asset", menuName = "Custom Assets/DOTS Animation")]
    public class AnimationDotsAsset : ScriptableObject
    {
        public AnimationClipParsedObject AnimationClipParsedObject;
    }


    [System.Serializable]
    public class AnimationClipParsedObject
    {
        public int Id;
        public string AnimationName;
        public int AnimatorInstanceId;
        public float Length;
        public bool Looped;
        public List<AnimationPositionSerialized> Positions;
        public List<AnimationRotationSerialized> Rotations;
    }
}