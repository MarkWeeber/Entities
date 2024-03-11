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
                //foreach (var item in positions)
                //{
                //    Debug.Log("time: " + item.Time + " value:" + item.Value);
                //}
                //Debug.Log($"1st: {firstCurveIndex} 2nd: {secondCurveIndex} samplescount: {samplesCount}");
                float3 firstPosition = positions[firstCurveIndex].Value;
                float3 secondPosition = positions[secondCurveIndex].Value;
                //Debug.Log($"first: {firstPosition} second: {secondPosition}");
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

        [BurstCompile]
        private void ProcessLayerOld(AnimatorActorLayerBuffer layer, FixedString512Bytes path, RefRW<LocalTransform> localTransform)
        {
            float transitionRate = -1f;
            layer.TransitionRate = 0;
            if (layer.IsInTransition && layer.FirstOffsetTimer <= 0f)
            {
                transitionRate = (layer.TransitionDuration - layer.TransitionTimer) / layer.TransitionDuration;
                transitionRate = math.clamp(transitionRate, 0f, 1f);
                layer.TransitionRate = transitionRate;
            }
            int currentAnimationId = layer.CurrentAnimationId;
            int nextAnimationId = layer.NextAnimationId;
            bool isInTransition = layer.IsInTransition;
            int currentAnimIndex = 0;
            int nextAnimIndex = 0;
            bool currentFound = false;
            bool nextFound = false;
            for (int i = 0; i < AnimationBlob.Length; i++)
            {
                var animationBlob = AnimationBlob[i];
                if (animationBlob.Id == currentAnimationId)
                {
                    currentAnimIndex = i;
                    currentFound = true;
                }
                if (isInTransition && animationBlob.Id == nextAnimationId)
                {
                    nextAnimIndex = i;
                    nextFound = true;
                }
                if (currentFound)
                {
                    if (isInTransition)
                    {
                        if (nextFound)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //ref RotationsPool currentRotationsPool = ref AnimationBlob[currentAnimIndex].Rotations.Value;
            //ref RotationsPool nextRotationsPool = ref AnimationBlob[nextAnimIndex].Rotations.Value;
            //ref PositionsPool currentPositionsPool = ref AnimationBlob[currentAnimIndex].Position.Value;
            //ref PositionsPool nextPositionsPool = ref AnimationBlob[nextAnimIndex].Position.Value;
            //var currentAnimation = AnimationBlob[currentAnimIndex];
            //var nextAnimation = AnimationBlob[nextAnimIndex];
            //AnimateThisPart(
            //    layer,
            //    path,
            //    localTransform,
            //    currentAnimation,
            //    nextAnimation
            //    );
        }

        [BurstCompile]
        private void AnimateThisPart(
           AnimatorActorLayerBuffer layer,
           FixedString512Bytes path,
           RefRW<LocalTransform> localTransform,
           AnimationBlobBuffer currentAnimation,
           AnimationBlobBuffer nextAnimation
           )
        {
            int currentAnimationId = layer.CurrentAnimationId;
            float currentAnimationTime = layer.CurrentAnimationTime;
            float3 setPosition = localTransform.ValueRO.Position;
            quaternion setRotation = localTransform.ValueRO.Rotation;
            float setScale = localTransform.ValueRO.Scale;

            // obtain first animation values
            ObtainAnimationValuesOld(
                ref setPosition,
                ref setRotation,
                currentAnimationTime,
                currentAnimationId,
                path,
                layer.Method,
                currentAnimation
                );

            // check if transition exists
            float transitionRate = layer.TransitionRate;
            if (transitionRate >= 0)
            {
                int nextAnimationId = layer.NextAnimationId;
                float nextAnimationTime = layer.NextAnimationTime;
                float3 nextPosition = localTransform.ValueRO.Position;
                quaternion nextRotation = localTransform.ValueRO.Rotation;
                ObtainAnimationValuesOld(
                    ref nextPosition,
                    ref nextRotation,
                    nextAnimationTime,
                    nextAnimationId,
                    path,
                    layer.Method,
                    nextAnimation
                    );
                switch (layer.Method)
                {
                    case PartsAnimationMethod.Lerp:
                        setPosition = math.lerp(setPosition, nextPosition, transitionRate);
                        setRotation = math.slerp(setRotation, nextRotation, transitionRate);
                        break;
                    case PartsAnimationMethod.Lean:
                        setPosition = CustomMath.Lean(setPosition, nextPosition, transitionRate);
                        setRotation = CustomMath.Lean(setRotation, nextRotation, transitionRate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        setPosition = CustomMath.SmoothStep(setPosition, nextPosition, transitionRate);
                        setRotation = CustomMath.SmoothStep(setRotation, nextRotation, transitionRate);
                        break;
                    default:
                        break;
                }
            }
            localTransform.ValueRW.Position = setPosition;
            localTransform.ValueRW.Rotation = setRotation;
            localTransform.ValueRW.Scale = setScale;
        }

        [BurstCompile]
        private void ObtainAnimationValuesOld(
            ref float3 position,
            ref quaternion rotation,
            float animationTime,
            int animationId,
            FixedString512Bytes path,
            PartsAnimationMethod method,
            AnimationBlobBuffer animation
            )
        {
            bool firstPosFound = false;
            bool secondPosFound = false;
            float3 firstPos = float3.zero;
            float3 secondPos = float3.zero;
            float firstPosTime = 0f;
            float secondPosTime = 0f;
            //ref var positions = ref animation.Position.Value.Positions;
            //for (int i = 0; i < positions.Length; i++)
            //{
            //    var pos = positions[i];
            //    if (pos.Path == path && pos.AnimationId == animationId)
            //    {
            //        if (pos.Time <= animationTime)
            //        {
            //            firstPosFound = true;
            //            firstPosTime = pos.Time;
            //            firstPos = pos.Value;
            //        }
            //        if (pos.Time > animationTime)
            //        {
            //            secondPosFound = true;
            //            secondPosTime = pos.Time;
            //            secondPos = pos.Value;
            //            break;
            //        }
            //    }
            //}
            bool firstRotFound = false;
            bool secondRotFound = false;
            quaternion firstRot = quaternion.identity;
            quaternion secondRot = quaternion.identity;
            float firstRotTime = 0f;
            float secondRotTime = 0f;
            //ref var rotations = ref animation.Rotations.Value;
            //for (int i = 0; i < rotations.Length; i++)
            //{
            //    var rot = rotations[i];
            //    if (rot.Path == path && rot.AnimationId == animationId)
            //    {
            //        if (rot.Time <= animationTime)
            //        {
            //            firstRotFound = true;
            //            firstRotTime = rot.Time;
            //            firstRot = rot.Value;
            //        }
            //        if (rot.Time > animationTime)
            //        {
            //            secondRotFound = true;
            //            secondRotTime = rot.Time;
            //            secondRot = rot.Value;
            //            break;
            //        }
            //    }
            //}
            if (secondPosFound && firstPosFound)
            {
                float rate = (animationTime - firstPosTime) / (secondPosTime - firstPosTime);
                switch (method)
                {
                    case PartsAnimationMethod.Lerp:
                        position = math.lerp(firstPos, secondPos, rate);
                        break;
                    case PartsAnimationMethod.Lean:
                        position = CustomMath.Lean(firstPos, secondPos, rate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        position = CustomMath.SmoothStep(firstPos, secondPos, rate);
                        break;
                    default:
                        break;
                }
            }
            if (firstPosFound && !secondPosFound)
            {
                position = firstPos;
            }
            if (secondRotFound && firstRotFound)
            {
                float rate = (animationTime - firstRotTime) / (secondRotTime - firstRotTime);
                switch (method)
                {
                    case PartsAnimationMethod.Lerp:
                        rotation = math.slerp(firstRot, secondRot, rate);
                        break;
                    case PartsAnimationMethod.Lean:
                        rotation = CustomMath.Lean(firstRot, secondRot, rate);
                        break;
                    case PartsAnimationMethod.SmoothStep:
                        rotation = CustomMath.SmoothStep(firstRot, secondRot, rate);
                        break;
                    default:
                        break;
                }

            }
            if (firstRotFound && !secondRotFound)
            {
                rotation = firstRot;
            }
        }
    }
}