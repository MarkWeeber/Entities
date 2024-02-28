using Unity.Mathematics;

namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationRotationItem
    {
        public int AnimationId;
        public string Path;
        public quaternion Value;
        public float Time;
    }
}