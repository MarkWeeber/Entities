using Unity.Mathematics;

namespace CustomUtils
{
    public static class CustomMath
    {
        public static float3 Lean(float3 from, float3 to, float rate)
        {
            return (to - from) * rate;
        }

        public static quaternion Lean(quaternion from, quaternion to, float rate)
        {
            return new quaternion( (to.value - from.value) * rate );
        }
    }
}