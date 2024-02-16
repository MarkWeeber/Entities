using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        bool firstPosFound = false;
        bool secondPosFound = false;
        float3 firstPos = float3.zero;
        float3 secondPos = float3.zero;
        float firstPosTime = 0f;
        float secondPosTime = 0f;
        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            if (pos.AnimationId == currentAnimationId)
            {
                if (pos.Time <= currentAnimationTime)
                {
                    firstPosFound = true;
                    firstPosTime = pos.Time;
                    firstPos = pos.Value;
                }
                if (pos.Time > currentAnimationTime)
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
            if (rot.AnimationId == currentAnimationId)
            {
                if (_entity.Index == 115)
                {
                    Debug.Log(rot.AnimationId + " = " + currentAnimationId);
                }
                if (rot.Time <= currentAnimationTime)
                {
                    firstRotFound = true;
                    firstRotTime = rot.Time;
                    firstRot = rot.Value;
                }
                if (rot.Time > currentAnimationTime)
                {
                    secondRotFound = true;
                    secondRotTime = rot.Time;
                    secondRot = rot.Value;
                    break;
                }
            }
        }

        float3 setPosition = localTransform.ValueRO.Position;
        quaternion setRotation = localTransform.ValueRO.Rotation;
        float setScale = localTransform.ValueRO.Scale;
        if (secondPosFound && firstPosFound)
        {
            float rate = (partComponent.ValueRO.CurrentAnimationTime - firstPosTime) / (secondPosTime - firstPosTime);
            setPosition = math.lerp(firstPos, secondPos, rate);

        }
        if (firstPosFound && !secondPosFound)
        {
            setPosition = firstPos;
        }
        if (secondRotFound && firstRotFound)
        {
            float rate = (partComponent.ValueRO.CurrentAnimationTime - firstRotTime) / (secondRotTime - firstRotTime);
            setRotation = math.slerp(firstRot, secondRot, rate);

        }
        if (firstRotFound && !secondRotFound)
        {
            setRotation = firstRot;
        }
        partComponent.ValueRW.FirstPosition = firstPos;
        partComponent.ValueRW.FirstRotation = firstRot;
        partComponent.ValueRW.SecondPosition = secondPos;
        partComponent.ValueRW.SecondRotation = secondRot;
        partComponent.ValueRW.SetPosition = setPosition;
        partComponent.ValueRW.SetRotation = setRotation;
        partComponent.ValueRW.FirstPosFound = firstPosFound;
        partComponent.ValueRW.SecondPosFound = secondPosFound;
        partComponent.ValueRW.FirstRotFound = firstRotFound;
        partComponent.ValueRW.SecondRotFound = secondRotFound;


        localTransform.ValueRW.Position = setPosition;
        localTransform.ValueRW.Rotation = setRotation;
        localTransform.ValueRW.Scale = setScale;
    }
}