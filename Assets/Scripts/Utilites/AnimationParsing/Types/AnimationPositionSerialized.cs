using Unity.Mathematics;

namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationPositionSerialized
    {
        public int AnimationId;
        public string Path;
        public float3 Value;
        public float Time;
    }
}