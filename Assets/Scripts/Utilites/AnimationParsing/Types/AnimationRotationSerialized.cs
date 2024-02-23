using Unity.Mathematics;

namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationRotationSerialized
    {
        public int AnimationId;
        public string Path;
        public quaternion Value;
        public float Time;
    }
}