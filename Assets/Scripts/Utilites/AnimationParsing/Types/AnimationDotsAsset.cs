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
        public int FPS;
        public List<AnimationPathData> PathData;
        public List<AnimationEventData> EventsData;
    }

    [System.Serializable]
    public struct AnimationPathData
    {
        public string Path;
        public bool HasPosition;
        public bool HasRotation;
        public bool HasEulerRotation;
        public List<AnimationPositioItem> Positions;
        public List<AnimationRotationItem> Rotations;
        public List<AnimationRotationItem> EulerRotations;

    }

    [System.Serializable]
    public struct PathProperyItem
    {
        public int Index;
        public string Path;
        public string PropertyName;
    }

    [System.Serializable]
    public enum ProperyType
    {
        LocalRotX = 0,
        LocalRoY = 1,
        LocalRotZ = 2,
        LocalRotW = 3,
        LocalPosX = 4,
        LocalPosY = 5,
        LocalPosZ = 6,
        LocalEulerAnglesRawX = 7,
        LocalEulerAnglesRawY = 8,
        LocalEulerAnglesRawZ = 9,
        LocalEulerAnglesRawW = 10,
    }

    [System.Serializable]
    public struct AnimationEventData
    {
        public float Time;
        public string EventName;
    }
}