using System.Reflection;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct TransformInstructionSystem : ISystem
{
    private ComponentLookup<TransformInstructionController> _componentLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _componentLookup = state.GetComponentLookup<TransformInstructionController>(true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entities = SystemAPI.QueryBuilder()
            .WithAll<TransformInstructionBuffer, TransformInstructionController, LocalTransform>().Build();
        if (entities.CalculateEntityCount() < 0)
        {
            return;
        }
        _componentLookup.Update(ref state);
        state.Dependency = new TransformInstructionsJob
        {
            //ComponentLookup = _componentLookup,
            DeltatTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel(entities, state.Dependency);

    }

    [BurstCompile]
    private partial struct TransformInstructionsJob : IJobEntity
    {
        public float DeltatTime;
        [BurstCompile]
        private void Execute(
            in DynamicBuffer<TransformInstructionBuffer> instructions,
            RefRW<TransformInstructionController> controller,
            RefRW<LocalTransform> localTransform)
        {
            if (controller.ValueRO.Completed || instructions.Length < 1)
            {
                return;
            }
            var currentTime = controller.ValueRO.CurrentInstructionTime;
            var addedTime = currentTime + DeltatTime;
            controller.ValueRW.CurrentInstructionTime = addedTime;
            float3 position = localTransform.ValueRO.Position;
            quaternion rotation = localTransform.ValueRO.Rotation;
            float scale = localTransform.ValueRO.Scale;
            ApplyTransformInstructions(
                in instructions, controller, currentTime, addedTime, controller.ValueRO.Looped, ref position, ref rotation, ref scale);
            localTransform.ValueRW.Position = position;
            localTransform.ValueRW.Rotation = rotation;
            localTransform.ValueRW.Scale = scale;
        }

        [BurstCompile]
        private void ApplyTransformInstructions(
            in DynamicBuffer<TransformInstructionBuffer> instructions,
            RefRW<TransformInstructionController> controller,
            float fromTime,
            float toTime,
            bool looped,
            ref float3 addedPosition,
            ref quaternion appliedRotation,
            ref float appliedScale)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                var instruction = instructions[i];
                float duration = instruction.Duration;
                float endTime = instruction.EndTime;
                float startTime = endTime - duration;
                if (fromTime < endTime && toTime > startTime)
                {
                    float actualEnd = (toTime > endTime) ? duration : toTime - startTime;
                    float actualStart = (fromTime < startTime) ? 0f : fromTime - startTime;
                    float rate = math.abs(actualEnd - actualStart)
                        / duration;
                    float instructionCompletionRate
                        = math.abs(actualEnd)
                        / duration;
                    ApplyTransformInstruction(
                        instruction,
                        rate,
                        instructionCompletionRate,
                        ref addedPosition,
                        ref appliedRotation,
                        ref appliedScale);
                    if (i == instructions.Length - 1) // if it's last instruction
                    {
                        float overHeadTime = toTime - endTime;
                        if (overHeadTime > 0f)
                        {
                            if (!looped)
                            {
                                controller.ValueRW.Completed = true;
                                controller.ValueRW.CurrentInstructionTime = toTime;
                            }
                            else
                            {
                                controller.ValueRW.CurrentInstructionTime = 0f;
                                ApplyTransformInstructions(
                                    instructions,
                                    controller,
                                    0f,
                                    overHeadTime,
                                    looped,
                                    ref addedPosition,
                                    ref appliedRotation,
                                    ref appliedScale);
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private void ApplyTransformInstruction(
            TransformInstructionBuffer instruction,
            float rate,
            float completionRate,
            ref float3 addedPosition,
            ref quaternion appliedRotation,
            ref float appliedScale)
        {
            if (instruction.PositionAdded)
            {
                addedPosition += instruction.AddedPosition * rate;
            }
            if (instruction.RotationApplied)
            {
                var rotationValue = instruction.AppliedEulerRotation * rate;
                appliedRotation = math.mul(appliedRotation, quaternion.RotateX(math.radians(rotationValue.x)));
                appliedRotation = math.mul(appliedRotation, quaternion.RotateY(math.radians(rotationValue.y)));
                appliedRotation = math.mul(appliedRotation, quaternion.RotateZ(math.radians(rotationValue.z)));
            }
            if (instruction.ScalingApplied)
            {
                appliedScale += instruction.AddedScale * rate;
            }
        }
    }
}