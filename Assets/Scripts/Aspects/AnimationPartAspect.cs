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
    private readonly RefRO<AnimatorActorPartComponent> partComponent;
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
        for(int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            if (pos.AnimationId == partComponent.ValueRO.CurrentAnimationClipId)
            {
                if (!firstPosFound && pos.Time <= partComponent.ValueRO.CurrentAnimationTime)
                {
                    firstPos = pos.Value;
                    firstPosTime = pos.Time;
                    firstPosFound = true;
                }
                else
                {
                    if (firstPosTime < pos.Time)
                    {
                        firstPosTime = pos.Time;
                        firstPos = pos.Value;
                    }
                }
                if (!secondPosFound && pos.Time > partComponent.ValueRO.CurrentAnimationTime)
                {
                    secondPos = pos.Value;
                    secondPosTime = pos.Time;
                    secondPosFound = true;
                }
                else
                {
                    if (secondPosTime > pos.Time)
                    {
                        secondPos = pos.Value;
                        secondPosTime = pos.Time;
                    }
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
            if (rot.AnimationId == partComponent.ValueRO.CurrentAnimationClipId)
            {
                if (!firstRotFound && rot.Time <= partComponent.ValueRO.CurrentAnimationTime)
                {
                    firstRot = rot.Value;
                    firstRotTime = rot.Time;
                    firstRotFound = true;
                }
                else
                {
                    if (firstRotTime < rot.Time)
                    {
                        firstRot = rot.Value;
                        firstRotTime = rot.Time;
                    }
                }
                if (!secondRotFound && rot.Time > partComponent.ValueRO.CurrentAnimationTime)
                {
                    secondRot = rot.Value;
                    secondRotTime = rot.Time;
                    secondRotFound = true;
                }
                else
                {
                    if (secondRotTime > rot.Time)
                    {
                        secondRot = rot.Value;
                        secondRotTime = rot.Time;
                    }
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
        else if (firstPosFound)
        {
            setPosition = firstPos;
        }
        if (secondRotFound && firstRotFound)
        {
            float rate = (partComponent.ValueRO.CurrentAnimationTime - firstRotTime) / (secondRotTime - firstRotTime);
            setRotation = math.slerp(firstRot, secondRot, rate);
        }
        else if (firstRotFound)
        {
            setRotation = firstRot;
        }
        localTransform.ValueRW.Position = setPosition;
        localTransform.ValueRW.Rotation = setRotation;
        localTransform.ValueRW.Scale = setScale;
    }
}