using Unity.Mathematics;

namespace CustomUtils
{
    public static class CustomMath
    {
        public static float3 Lean(float3 from, float3 to, float rate)
        {
            return from + ((to - from) * rate);
        }

        public static float3 SmoothStep(float3 from, float3 to, float rate)
        {
            float3 direction = to - from;
            //float evaluate = rate * rate * rate * (rate * (6f * rate - 15f) + 10f);
            float evaluate = rate * rate * (3f - 2f * rate);
            return from + direction * evaluate;
        }

        public static quaternion Lean(quaternion from, quaternion to, float rate)
        {
            return new quaternion(from.value + ((to.value - from.value) * rate));
        }

        public static quaternion SmoothStep(quaternion from, quaternion to, float rate)
        {
            float4 direction = (to.value - from.value);
            //float evaluate = rate * rate * rate * (rate * (6f * rate - 15f) + 10f);
            float evaluate = rate * rate * (3f - 2f * rate);
            return new quaternion(from.value + (direction * evaluate));
        }
    }
}