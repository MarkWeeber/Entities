using Unity.Mathematics;

namespace CustomUtils
{
    public static class CustomMath
    {
        public static float3 Lean(float3 from, float3 to, float rate)
        {
            return from + ((to - from) * rate);
        }

        public static quaternion Lean(quaternion from, quaternion to, float rate)
        {
            return new quaternion(from.value + ((to.value - from.value) * rate));
        }
    }
}