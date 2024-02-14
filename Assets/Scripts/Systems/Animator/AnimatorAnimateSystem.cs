using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorActorComponent>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery acrtorsQuery = SystemAPI.QueryBuilder()
            .WithAll<
             AnimationBuffer,
             AnimatorActorParametersBuffer,
             AnimatorActorPartBufferComponent,
             AnimatorActorLayerBuffer,
             LayerStateBuffer,
             StateTransitionBuffer,
             TransitionCondtionBuffer>()
            .Build();

        if (acrtorsQuery.CalculateEntityCount() < 1)
        {
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        float deltaTime = SystemAPI.Time.DeltaTime;

        state.Dependency = new ActorAnimateJob
        {
            ParallelWriter = parallelWriter,
            DeltaTime = deltaTime
        }.ScheduleParallel(acrtorsQuery, state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct ActorAnimateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter; // for setting AnimatorActorPartComponent
        public float DeltaTime;

        [BurstCompile]
        public void Execute(
            [ChunkIndexInQuery] int sortKey,
            in DynamicBuffer<AnimationBuffer> animations,
            in DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            in DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            ref DynamicBuffer<AnimatorActorLayerBuffer> layers,
            in DynamicBuffer<LayerStateBuffer> states,
            in DynamicBuffer<StateTransitionBuffer> transitions,
            in DynamicBuffer<TransitionCondtionBuffer> conditions
            )
        {
            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var layer = layers[layerIndex];
                ProcessLayer(
                    sortKey,
                    ref layer,
                    in animations,
                    in parameters,
                    in parts,
                    in states,
                    in transitions,
                    in conditions,
                    DeltaTime);
                layers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void ProcessLayer(
            int sortKey,
            ref AnimatorActorLayerBuffer layer,
            in DynamicBuffer<AnimationBuffer> animations,
            in DynamicBuffer<AnimatorActorParametersBuffer> parameters,
            in DynamicBuffer<AnimatorActorPartBufferComponent> parts,
            in DynamicBuffer<LayerStateBuffer> states,
            in DynamicBuffer<StateTransitionBuffer> transitions,
            in DynamicBuffer<TransitionCondtionBuffer> conditions,
            float deltaTime
            )
        {
            float defaultWeight = layer.DefaultWeight;
            int currentStateId = layer.CurrentStateId;
            float currentStateSpeed = layer.CurrentStateSpeed;
            int currentAnimationId = layer.CurrentAnimationId;
            float currentAnimationTime = layer.CurrentAnimationTime;
            bool currentAnimationIsLooped = layer.CurrentAnimationIsLooped;
            bool isInTransition = layer.IsInTransition;
            float transitionTimer = layer.TransitionTimer;
            float exitPercentage = layer.ExitPercentage;
            bool fixedDuration = layer.FixedDuration;
            float durationTime = layer.DurationTime;
            float transitionAnimationTime = layer.TransitionAnimationTime;
            float offsetPercentage = layer.OffsetPercentage;
            int nextStateId = layer.NextStateId;
            float nextStateSpeed = layer.NextStateSpeed;
            int nextAnimationId = layer.NextAnimationId;
            float nextAnimationTime = layer.NextAnimationTime;
            bool nextAnimationIsLooped = layer.NextAnimationIsLooped;

            // if newly created then set current animation info
            if (currentAnimationId == 0)
            {
                foreach (var state in states)
                {
                    if (state.Id == currentStateId && state.LayerId == layer.Id)
                    {
                        currentAnimationId = state.AnimationClipId;
                        break;
                    }
                }
            }

            // check parameters conditions matching
            bool transitionFound = false;
            var transition = new StateTransitionBuffer();
            foreach (var _transition in transitions)
            {
                if (_transition.StateId == currentStateId)
                {
                    bool allConditionsMet = false;
                    foreach (var condition in conditions)
                    {
                        if (condition.TransitionId == _transition.Id)
                        {

                        }
                    }
                }
                if (transitionFound)
                {
                    break;
                }
            }

            // check if already in transition

        }
    }



    //[BurstCompile]
    //private void ProcessLayer(
    //    int sortKey,
    //    ref AnimatorActorLayerBuffer layer,
    //    NativeArray<AnimatorActorPartBufferComponent> actorParts,
    //    NativeArray<AnimatorActorParametersBuffer> actorParameters,
    //    float deltaTime)
    //{
    //    // finding animation clip id
    //    int currentAnimationId = -1;
    //    bool animationFound = false;
    //    foreach (var state in States)
    //    {
    //        if (state.AnimatorInstanceId == _animatorInstatnceId && state.Id == layer.CurrentStateId)
    //        {
    //            currentAnimationId = state.AnimationClipId;
    //            break;
    //        }
    //    }
    //    AnimationBuffer animationClip = new AnimationBuffer();
    //    foreach (var animation in Animations)
    //    {
    //        if (animation.AnimatorInstanceId == _animatorInstatnceId && animation.Id == currentAnimationId)
    //        {
    //            animationClip = animation;
    //            animationFound = true;
    //            break;
    //        }
    //    }
    //    if (!animationFound) // animation clip somehow not found, returning it
    //    {
    //        return;
    //    }

    //    // check parameters for conditions



    //    // animation found let's animate
    //    // manage timers
    //    var animationDuration = animationClip.Length;
    //    var looped = animationClip.Looped;
    //    var currentTimer = layer.CurrentAnimationTime;
    //    var loopEnded = false;
    //    if (currentTimer > animationDuration)
    //    {
    //        if (looped)
    //        {
    //            currentTimer = currentTimer % animationDuration;
    //        }
    //        else
    //        {
    //            currentTimer = animationDuration;
    //            loopEnded = true;
    //        }
    //    }
    //    // loop over paths
    //    foreach (var part in actorParts)
    //    {
    //        // positions
    //        float3 firstPosition = float3.zero;
    //        float3 secondPosition = float3.zero;
    //        bool firstPositionFound = false;
    //        bool secondPositionFound = false;
    //        float firstPositionTime = -1f;
    //        float secondPositionTime = -1f;
    //        foreach (var position in Positions)
    //        {
    //            if (position.AnimationId == currentAnimationId && part.Path == position.Path)
    //            {
    //                if (currentTimer <= position.Time)
    //                {
    //                    if (!firstPositionFound)
    //                    {
    //                        firstPosition = position.Value;
    //                        firstPositionTime = position.Time;
    //                        firstPositionFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (firstPositionTime > position.Time)
    //                        {
    //                            firstPosition = position.Value;
    //                            firstPositionTime = position.Time;
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (!secondPositionFound)
    //                    {
    //                        secondPosition = position.Value;
    //                        secondPositionTime = position.Time;
    //                        secondPositionFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (secondPositionTime < position.Time)
    //                        {
    //                            secondPosition = position.Value;
    //                            secondPositionTime = position.Time;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        // rotations
    //        quaternion firstRotation = quaternion.identity;
    //        quaternion secondRotation = quaternion.identity;
    //        bool firstRotationFound = false;
    //        bool secondRotationFound = false;
    //        float firstRotationTime = -1f;
    //        float secondRotationTime = -1f;
    //        foreach (var rotation in Rotations)
    //        {
    //            if (rotation.AnimationId == currentAnimationId && part.Path == rotation.Path)
    //            {
    //                if (currentTimer <= rotation.Time)
    //                {
    //                    if (!firstRotationFound)
    //                    {
    //                        firstRotation = rotation.Value;
    //                        firstRotationTime = rotation.Time;
    //                        firstRotationFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (firstRotationTime > rotation.Time)
    //                        {
    //                            firstRotation = rotation.Value;
    //                            firstRotationTime = rotation.Time;
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (!secondRotationFound)
    //                    {
    //                        secondRotation = rotation.Value;
    //                        secondRotationTime = rotation.Time;
    //                        secondRotationFound = true;
    //                    }
    //                    else
    //                    {
    //                        if (secondRotationTime < rotation.Time)
    //                        {
    //                            secondRotation = rotation.Value;
    //                            secondRotationTime = rotation.Time;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // get current data from local transform
    //        Entity partEntity = part.Value;
    //        RefRO<LocalTransform> partLocaltTransform = PartComponentLookup.GetRefRO(partEntity);
    //        float3 newPosition = partLocaltTransform.ValueRO.Position;
    //        quaternion newRotation = partLocaltTransform.ValueRO.Rotation;
    //        float scale = partLocaltTransform.ValueRO.Scale;

    //        // calculcate rates
    //        if (loopEnded)
    //        {
    //            if (firstPositionFound)
    //            {
    //                newPosition = firstPosition;
    //            }
    //            if (firstRotationFound)
    //            {
    //                newRotation = firstRotation;
    //            }
    //        }
    //        else
    //        {
    //            if (secondPositionFound)
    //            {
    //                float rate = (currentTimer - firstPositionTime) / (secondPositionTime - firstPositionTime);
    //                newPosition = math.lerp(firstPosition, secondPosition, rate);
    //            }
    //            if (secondRotationFound)
    //            {
    //                float rate = (currentTimer - firstRotationTime) / (secondRotationTime - firstRotationTime);
    //                newRotation = math.slerp(firstRotation, secondRotation, rate);
    //            }
    //        }

    //        // apply
    //        ParallelWriter.SetComponent(sortKey, partEntity, new LocalTransform
    //        {
    //            Position = newPosition,
    //            Rotation = newRotation,
    //            Scale = scale
    //        });
    //    }
    //    currentTimer += deltaTime;
    //    layer.CurrentAnimationTime = currentTimer;
    //}
}