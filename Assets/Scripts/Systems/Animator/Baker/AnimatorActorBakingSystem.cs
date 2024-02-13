using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
public partial struct AnimatorActorBakingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonBuffer<AnimatorBuffer>(out DynamicBuffer<AnimatorBuffer> animatorDB))
        {
            NativeArray<AnimatorBuffer> animators = animatorDB.AsNativeArray();
            NativeArray<AnimatorLayerBuffer> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerBuffer>().AsNativeArray();
            NativeArray<AnimatorParametersBuffer> parameters = SystemAPI.GetSingletonBuffer<AnimatorParametersBuffer>().AsNativeArray();
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<AnimationPositionBuffer> positions = SystemAPI.GetSingletonBuffer<AnimationPositionBuffer>().AsNativeArray();
            NativeArray<AnimationRotationBuffer> rotations = SystemAPI.GetSingletonBuffer<AnimationRotationBuffer>().AsNativeArray();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            EntityQuery animatorActorEntities = SystemAPI.QueryBuilder()
                .WithAll<AnimatorActorComponent, AnimatorActorPartBufferComponent>()
                .Build();

            state.Dependency = new PrepareAnimatorActorJob
            {
                Animators = animators,
                Layers = layers,
                Parameters = parameters,
                States = states,
                Positions = positions,
                Rotations = rotations,
                ParallelWriter = parallelWriter,
            }.ScheduleParallel(animatorActorEntities, state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            positions.Dispose();
            states.Dispose();
            animators.Dispose();
            layers.Dispose();
            parameters.Dispose();
        }
    }
    [BurstCompile]
    private partial struct PrepareAnimatorActorJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimatorBuffer> Animators;
        [ReadOnly]
        public NativeArray<AnimatorLayerBuffer> Layers;
        [ReadOnly]
        public NativeArray<AnimatorParametersBuffer> Parameters;
        [ReadOnly]
        public NativeArray<LayerStateBuffer> States;
        [ReadOnly]
        public NativeArray<AnimationPositionBuffer> Positions;
        [ReadOnly]
        public NativeArray<AnimationRotationBuffer> Rotations;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            Entity entity,
            RefRW<AnimatorActorComponent> animatorActorComponent,
            DynamicBuffer<AnimatorActorPartBufferComponent> parts)
        {
            // getting animator id
            int animatorId = -1;
            foreach (var animator in Animators)
            {
                if (animator.Name == animatorActorComponent.ValueRO.AnimatorControllerName)
                {
                    animatorActorComponent.ValueRW.AnimatorId = animator.Id;
                    animatorId = animator.Id;
                    break;
                }
            }
            // adding necessary parameters
            ParallelWriter.AddBuffer<AnimatorActorParametersBuffer>(sortKey, entity);
            foreach (var parameter in Parameters)
            {
                if (parameter.AnimatorInstanceId == animatorId)
                {
                    float defaultValue = 0;
                    switch (parameter.Type)
                    {
                        case UnityEngine.AnimatorControllerParameterType.Float:
                            defaultValue = parameter.DefaultFloat;
                            break;
                        case UnityEngine.AnimatorControllerParameterType.Int:
                            defaultValue = parameter.DefaultInt;
                            break;
                        case UnityEngine.AnimatorControllerParameterType.Bool:
                            if (parameter.DefaultBool)
                            {
                                defaultValue = 1;
                            }
                            else
                            {
                                defaultValue = -1;
                            }
                            break;
                        case UnityEngine.AnimatorControllerParameterType.Trigger:
                            defaultValue = 0;
                            break;
                        default:
                            break;
                    }
                    var actorParameterItem = new AnimatorActorParametersBuffer
                    {
                        ParameterName = parameter.ParameterName,
                        Type = parameter.Type,
                        Value = defaultValue
                    };
                    ParallelWriter.AppendToBuffer(sortKey, entity, actorParameterItem);
                }
            }
            // adding layers states and transition infos
            ParallelWriter.AddBuffer<AnimatorActorLayerBuffer>(sortKey, entity);
            ParallelWriter.AddBuffer<AnimatorActorTransitionBuffer>(sortKey, entity);
            foreach (var layer in Layers)
            {
                if (layer.AnimatorInstanceId == animatorId)
                {
                    int defaultStateId = -1;
                    foreach (var state in States) // find relative default state and set it at start
                    {
                        if (state.AnimatorInstanceId == animatorId && layer.Id == state.LayerId && state.DefaultState)
                        {
                            defaultStateId = state.Id;
                            break;
                        }
                    }
                    var actorLayerItem = new AnimatorActorLayerBuffer
                    {
                        Id = layer.Id,
                        AnimationTime = 0f,
                        CurrentStateIndex = defaultStateId,
                    };
                    ParallelWriter.AppendToBuffer<AnimatorActorLayerBuffer>(sortKey, entity, actorLayerItem);
                    var transitionInfoItem = new AnimatorActorTransitionBuffer
                    {
                        Running = false,
                        CurrentStateIndex = defaultStateId,
                        NextStateIndex = defaultStateId,
                        LayerIndex = layer.Id,
                        ExitTimeDuration = 0f,
                        HasExitTime = false,
                        OffsetTimeDuration = 0f,
                        TransitionDuration = 0f,
                        TransitionTimer = 0f
                    };
                    ParallelWriter.AppendToBuffer<AnimatorActorTransitionBuffer>(sortKey, entity, transitionInfoItem);
                }
            }

            // adding buffer for each of parts
            NativeArray<AnimationPositionBuffer> animationPositions = new NativeArray<AnimationPositionBuffer>(Positions, Allocator.Temp);
            animationPositions.Sort(new CompareAnimationPositionTimeBuffer());
            NativeArray<AnimationRotationBuffer> animationRotations = new NativeArray<AnimationRotationBuffer>(Rotations, Allocator.Temp);
            animationRotations.Sort(new CompareAnimationRotationTimeBuffer());
            foreach (var part in parts)
            {
                Entity partEntity = part.Value;
                // positions
                ParallelWriter.AddBuffer<AnimationPartPositionBuffer>(sortKey, partEntity);
                for (int i = 0; i < animationPositions.Length; i++)
                {
                    var item = animationPositions[i];
                    if (item.Path == part.Path)
                    {
                        var partItem = new AnimationPartPositionBuffer
                        {
                            AnimationId = item.AnimationId,
                            Time = item.Time,
                            Value = item.Value,
                        };
                        ParallelWriter.AppendToBuffer(sortKey, partEntity, partItem);
                    }
                }
                // rotations
                ParallelWriter.AddBuffer<AnimationPartRotationBuffer>(sortKey, partEntity);
                for (int i = 0; i < animationRotations.Length; i++)
                {
                    var item = animationRotations[i];
                    if (item.Path == part.Path)
                    {
                        var partItem = new AnimationPartRotationBuffer
                        {
                            AnimationId = item.AnimationId,
                            Time = item.Time,
                            Value = item.Value,
                        };
                        ParallelWriter.AppendToBuffer(sortKey, partEntity, partItem);
                    }
                }
            }
            animationPositions.Dispose();
            animationRotations.Dispose();
        }
    }
}