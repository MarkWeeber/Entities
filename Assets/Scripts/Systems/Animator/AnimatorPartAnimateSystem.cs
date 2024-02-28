using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using CustomUtils;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
//[UpdateAfter(typeof(AnimatorAnimateSystem))]
public partial struct AnimatorPartAnimateSystem : ISystem
{
    private BufferLookup<AnimatorActorLayerBuffer> layerLookup;
    private BufferLookup<AnimatorActorPartBufferComponent> partLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        layerLookup = state.GetBufferLookup<AnimatorActorLayerBuffer>(true);
        partLookup = state.GetBufferLookup<AnimatorActorPartBufferComponent>(true);
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
        partLookup.Update(ref state);
        state.Dependency = new AnimatePartJob
        {
            AnimationBlob = animationBlob,
            PartLookup = partLookup,
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
        [ReadOnly]
        public BufferLookup<AnimatorActorPartBufferComponent> PartLookup;
        [BurstCompile]
        private void Execute(RefRO<AnimatorPartComponent> animatorRoot, RefRW<LocalTransform> localTransform, Entity entity)
        {
            Entity rootEntity = animatorRoot.ValueRO.RootEntity;
            if (!LayerLookup.HasBuffer(rootEntity) || !PartLookup.HasBuffer(rootEntity))
            {
                return;
            }
            FixedString512Bytes currentPath = new FixedString512Bytes();
            foreach (var part in PartLookup[rootEntity])
            {
                if (part.Value == entity)
                {
                    currentPath = part.Path;
                    break;
                }
            }
            foreach (var layer in LayerLookup[rootEntity])
            {
                ProcessLayer(layer, currentPath, localTransform);
            }
        }

        [BurstCompile]
        private void ProcessLayer(AnimatorActorLayerBuffer layer, FixedString512Bytes path, RefRW<LocalTransform> localTransform)
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
            var currentAnimation = AnimationBlob[currentAnimIndex];
            var nextAnimation = AnimationBlob[nextAnimIndex];
            AnimateThisPart(
                layer,
                path,
                localTransform,
                currentAnimation,
                nextAnimation
                );
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
            ObtainAnimationValues(
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
                ObtainAnimationValues(
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
        private void ObtainAnimationValues(
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
            ref var positions = ref animation.Position.Value.Positions;
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
            ref var rotations = ref animation.Rotations.Value;
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