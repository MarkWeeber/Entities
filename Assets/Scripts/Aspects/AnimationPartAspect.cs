using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CustomUtils;

public readonly partial struct AnimationPartAspect : IAspect
{
    private readonly Entity _entity;
    private readonly RefRW<LocalTransform> localTransform;
    private readonly DynamicBuffer<AnimationPartPositionBuffer> positions;
    private readonly DynamicBuffer<AnimationPartRotationBuffer> rotations;
    private readonly RefRW<AnimatorActorPartComponent> partComponent;
    public void Animate()
    {
        int currentAnimationId = partComponent.ValueRO.CurrentAnimationClipId;
        float currentAnimationTime = partComponent.ValueRO.CurrentAnimationTime;
        float3 setPosition = localTransform.ValueRO.Position;
        quaternion setRotation = localTransform.ValueRO.Rotation;
        float setScale = localTransform.ValueRO.Scale;
        
        // obtain first animation values
        ObtainAnimationValues(ref setPosition, ref setRotation, currentAnimationTime, currentAnimationId, partComponent.ValueRO.Method);

        // check if transition exists
        float transitionRate = partComponent.ValueRO.TransitionRate;
        if (transitionRate >= 0)
        {
            int nextAnimationId = partComponent.ValueRO.NextAnimationClipId;
            float nextAnimationTime = partComponent.ValueRO.NextAnimationTime;
            float3 nextPosition = localTransform.ValueRO.Position;
            quaternion nextRotation = localTransform.ValueRO.Rotation;
            ObtainAnimationValues(ref nextPosition, ref nextRotation, nextAnimationTime, nextAnimationId, partComponent.ValueRO.Method);
            switch (partComponent.ValueRO.Method)
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

        // setting values
        localTransform.ValueRW.Position = setPosition;
        localTransform.ValueRW.Rotation = setRotation;
        localTransform.ValueRW.Scale = setScale;

        //// debug info
        //partComponent.ValueRW.FirstPosition = firstPos;
        //partComponent.ValueRW.FirstRotation = firstRot;
        //partComponent.ValueRW.SecondPosition = secondPos;
        //partComponent.ValueRW.SecondRotation = secondRot;
        //partComponent.ValueRW.SetPosition = setPosition;
        //partComponent.ValueRW.SetRotation = setRotation;
        //partComponent.ValueRW.FirstPosFound = firstPosFound;
        //partComponent.ValueRW.SecondPosFound = secondPosFound;
        //partComponent.ValueRW.FirstRotFound = firstRotFound;
        //partComponent.ValueRW.SecondRotFound = secondRotFound;


    }

    private void ObtainAnimationValues(ref float3 position, ref quaternion rotation, float animationTime, int animationId, PartsAnimationMethod method)
    {
        bool firstPosFound = false;
        bool secondPosFound = false;
        float3 firstPos = float3.zero;
        float3 secondPos = float3.zero;
        float firstPosTime = 0f;
        float secondPosTime = 0f;
        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            if (pos.AnimationId == animationId)
            {
                if (pos.Time <= animationTime)
                {
                    firstPosFound = true;
                    firstPosTime = pos.Time;
                    firstPos = pos.Value;
                }
                if (pos.Time > animationTime)
                {
                    secondPosFound = true;
                    secondPosTime = pos.Time;
                    secondPos = pos.Value;
                    break;
                }
            }
        }
        bool firstRotFound = false;
        bool secondRotFound = false;
        quaternion firstRot = quaternion.identity;
        quaternion secondRot = quaternion.identity;
        float firstRotTime = 0f;
        float secondRotTime = 0f;
        for (int i = 0; i < rotations.Length; i++)
        {
            var rot = rotations[i];
            if (rot.AnimationId == animationId)
            {
                if (rot.Time <= animationTime)
                {
                    firstRotFound = true;
                    firstRotTime = rot.Time;
                    firstRot = rot.Value;
                }
                if (rot.Time > animationTime)
                {
                    secondRotFound = true;
                    secondRotTime = rot.Time;
                    secondRot = rot.Value;
                    break;
                }
            }
        }

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