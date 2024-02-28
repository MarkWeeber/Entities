using Unity.Mathematics;

namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationKeyItem
    {
        public int AnimationId;
        public int AnimatorInstanceId;
        public string Path;
        public float Time;
        public bool PositionEngaged;
        public float3 PositionValue;
        public bool RotationEngaged;
        public quaternion RotationValue;
    }
}