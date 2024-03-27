using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using CustomUtils;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(AnimatorAnimateSystem))]
public partial struct AnimatorPartAnimateSystem : ISystem
{
    private BufferLookup<AnimatorActorLayerBuffer> layerLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        layerLookup = state.GetBufferLookup<AnimatorActorLayerBuffer>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery parts = SystemAPI.QueryBuilder().WithAll<AnimatorPartComponent, LocalTransform>().Build();
        if (parts.CalculateEntityCount() < 1)
        {
            return;
        }
        NativeArray<AnimationBlobBuffer> animationBlob = SystemAPI.GetSingletonBuffer<AnimationBlobBuffer>().AsNativeArray();
        layerLookup.Update(ref state);
        state.Dependency = new AnimatePartJob
        {
            AnimationBlob = animationBlob,
            LayerLookup = layerLookup
        }.ScheduleParallel(parts, state.Dependency);
        animationBlob.Dispose();
    }

    [BurstCompile]
    private partial struct AnimatePartJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimationBlobBuffer> AnimationBlob;
        [ReadOnly]
        public BufferLookup<AnimatorActorLayerBuffer> LayerLookup;
        [BurstCompile]
        private void Execute(RefRO<AnimatorPartComponent> animatorRoot, RefRW<LocalTransform> localTransform, Entity entity)
        {
            Entity rootEntity = animatorRoot.ValueRO.RootEntity;
            int partIndex = animatorRoot.ValueRO.PathAnimationBlobIndex;
            if (LayerLookup.HasBuffer(rootEntity))
            {
                foreach (var layer in LayerLookup[rootEntity])
                {
                    ProcessLayer(layer, localTransform, partIndex);
                }
            }
        }

        [BurstCompile]
        private void ProcessLayer(AnimatorActorLayerBuffer layer, RefRW<LocalTransform> localTransform, int partIndex)
        {
            bool activeTransition = false;
            float transitionRate = -1f;
            if (layer.IsInTransition)
            {
                if (layer.FirstOffsetTimer <= 0f)
                {
                    transitionRate = (layer.TransitionDuration - layer.TransitionTimer) / layer.TransitionDuration;
                    transitionRate = math.clamp(transitionRate, 0f, 1f);
                    activeTransition = true;
                }
            }
            int currentAnimationBlobIndex = layer.CurrentAnimationBlobIndex;
            if (currentAnimationBlobIndex < 0)
            {
                return;
            }
            float3 currentPos = localTransform.ValueRO.Position;
            quaternion currentRot = localTransform.ValueRO.Rotation;
            float currentScale = localTransform.ValueRO.Scale;
            float currentAnimationTime = layer.CurrentAnimationTime;
            // obtain current animation values
            ObtainAnimationValues(ref currentPos, ref currentRot, currentAnimationTime, layer.Method, currentAnimationBlobIndex, partIndex);
            // if is in active transition
            if (activeTransition)
            {
                float3 nextPos = localTransform.ValueRO.Position;
                quaternion nextRot = localTransform.ValueRO.Rotation;
                float nextScale = localTransform.ValueRO.Scale;
                float nextAnimationTime = layer.NextAnimationTime;
                int nextAnimationBlobIndex = layer.NextAnimationBlobIndex;
                ObtainAnimationValues(ref nextPos, ref nextRot, nextAnimationTime, layer.Method, nextAnimationBlobIndex, partIndex);
                // interpolate first values with second
                currentPos = InterPolate(currentPos, nextPos, transitionRate, layer.Method);
                currentRot = InterPolate(currentRot, nextRot, transitionRate, layer.Method);
            }
            // apply transforms
            localTransform.ValueRW.Position = currentPos;
            localTransform.ValueRW.Rotation = currentRot;
            localTransform.ValueRW.Scale = currentScale;
        }

        [BurstCompile]
        private void ObtainAnimationValues(
            ref float3 position, ref quaternion rotation, float animationTime, PartsAnimationMethod method, int animationIndex, int partIndex)
        {
            AnimationBlobBuffer animation = AnimationBlob[animationIndex];
            int fps = animation.FPS;
            ref PathDataPool pathDataPool = ref animation.PathData.Value;
            ref PathsPool pathsPool = ref pathDataPool.PathData[partIndex];
            int samplesCount = (int)math.ceil(animation.Length * fps);
            float timeRate = math.clamp((animationTime / animation.Length), 0f, 1f);
            int firstCurveIndex = (int)math.floor(timeRate * (samplesCount - 1));
            int secondCurveIndex = (int)math.ceil(timeRate * (samplesCount - 1));
            float transitionRate = animationTime - firstCurveIndex * (animation.Length / samplesCount);
            if (pathsPool.HasPositions)
            {
                ref var positions = ref pathsPool.Positions;
                if (positions.Length < 1)
                {
                    return;
                }
                float3 firstPosition = positions[firstCurveIndex].Value;
                float3 secondPosition = positions[secondCurveIndex].Value;
                position = InterPolate(firstPosition, secondPosition, transitionRate, method);
            }
            if (pathsPool.HasRotations)
            {
                ref var rotations = ref pathsPool.Rotations;
                if (rotations.Length < 1)
                {
                    return;
                }
                quaternion firstRotation = rotations[firstCurveIndex].Value;
                quaternion secondRotation = rotations[secondCurveIndex].Value;
                rotation = InterPolate(firstRotation, secondRotation, transitionRate, method);
            }
            if (pathsPool.HasEulerRotations)
            {
                ref var eulerRotations = ref pathsPool.EulerRotations;
                if (eulerRotations.Length < 1)
                {
                    return;
                }
                quaternion firstRotation = eulerRotations[firstCurveIndex].Value;
                quaternion secondRotation = eulerRotations[secondCurveIndex].Value;
                quaternion eulerRotation = InterPolate(firstRotation, secondRotation, transitionRate, method);
                rotation = quaternion.Euler(
                                        math.radians(eulerRotation.value.x),
                                        math.radians(eulerRotation.value.y),
                                        math.radians(eulerRotation.value.z));
            }
        }

        [BurstCompile]
        private float3 InterPolate(float3 first, float3 second, float rate, PartsAnimationMethod method)
        {
            float3 result = first;
            switch (method)
            {
                case PartsAnimationMethod.Lerp:
                    result = math.lerp(first, second, rate);
                    break;
                case PartsAnimationMethod.Lean:
                    result = CustomMath.Lean(first, second, rate);
                    break;
                case PartsAnimationMethod.SmoothStep:
                    result = CustomMath.SmoothStep(first, second, rate);
                    break;
                default:
                    break;
            }
            return result;
        }

        [BurstCompile]
        private quaternion InterPolate(quaternion first, quaternion second, float rate, PartsAnimationMethod method)
        {
            quaternion result = first;
            switch (method)
            {
                case PartsAnimationMethod.Lerp:
                    result = math.slerp(first, second, rate);
                    break;
                case PartsAnimationMethod.Lean:
                    result = CustomMath.Lean(first, second, rate);
                    break;
                case PartsAnimationMethod.SmoothStep:
                    result = CustomMath.SmoothStep(first, second, rate);
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}