using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct AnimatorAnimateSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimatorBuffer>();
        state.RequireForUpdate<AnimatorActorComponent>();
        localTransformLookup = state.GetComponentLookup<LocalTransform>(true);

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
            NativeArray<AnimationBuffer> animations = SystemAPI.GetSingletonBuffer<AnimationBuffer>().AsNativeArray();
            NativeArray<AnimatorLayerBuffer> layers = SystemAPI.GetSingletonBuffer<AnimatorLayerBuffer>().AsNativeArray();
            NativeArray<LayerStateBuffer> states = SystemAPI.GetSingletonBuffer<LayerStateBuffer>().AsNativeArray();
            NativeArray<AnimationKeyBuffer> animationKeys = SystemAPI.GetSingletonBuffer<AnimationKeyBuffer>().AsNativeArray();
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();

            EntityQuery actorPartsQuery = SystemAPI.QueryBuilder()
                .WithAll<
                AnimatorActorComponent,
                AnimatorActorParametersBuffer,
                AnimatorActorPartBufferComponent,
                AnimatorActorTransitionBuffer,
                AnimatorActorLayerBuffer>()
                .Build();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
            localTransformLookup.Update(ref state);
            float deltaTime = SystemAPI.Time.DeltaTime;

            state.Dependency = new ActorAnimateJob
            {
                Animators = animators,
                Animations = animations,
                AnimationKeys = animationKeys,
                Conditions = conditions,
                Layers = layers,
                States = states,
                Transitions = transitions,
                LocalTransformLookup = localTransformLookup,
                ParallelWriter = parallelWriter,
                DeltaTime = deltaTime
            }.ScheduleParallel(actorPartsQuery, state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            animators.Dispose();
            animations.Dispose();
            layers.Dispose();
            states.Dispose();
            animationKeys.Dispose();
            transitions.Dispose();
            conditions.Dispose();
        }
    }

    [BurstCompile]
    private partial struct ActorAnimateJob : IJobEntity
    {
        [ReadOnly]
        public NativeArray<AnimatorBuffer> Animators;
        [ReadOnly]
        public NativeArray<AnimationBuffer> Animations;
        [ReadOnly]
        public NativeArray<AnimatorLayerBuffer> Layers;
        [ReadOnly]
        public NativeArray<LayerStateBuffer> States;
        [ReadOnly]
        public NativeArray<AnimationKeyBuffer> AnimationKeys;
        [ReadOnly]
        public NativeArray<StateTransitionBuffer> Transitions;
        [ReadOnly]
        public NativeArray<TransitionCondtionBuffer> Conditions;
        [ReadOnly]
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        public float DeltaTime;
        private int _animatorInstatnceId;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            RefRO<AnimatorActorComponent> animatorActor,
            DynamicBuffer<AnimatorActorParametersBuffer> actorParameters,
            DynamicBuffer<AnimatorActorPartBufferComponent> actorParts,
            DynamicBuffer<AnimatorActorTransitionBuffer> actorTransitions,
            DynamicBuffer<AnimatorActorLayerBuffer> actorLayers
        )
        {
            // getting animator id
            _animatorInstatnceId = animatorActor.ValueRO.AnimatorId;

            // loop each layer
            for (int layerIndex = 0; layerIndex < actorLayers.Length; layerIndex++)
            {
                var layer = actorLayers[layerIndex];
                AnimateLayer(ref layer);
                actorLayers[layerIndex] = layer;
            }
        }

        [BurstCompile]
        private void AnimateLayer(ref AnimatorActorLayerBuffer layer)
        {
            int animationClipId = -1;
            foreach (var state in States)
            {
                if (state.AnimatorInstanceId == _animatorInstatnceId && state.Id == layer.CurrentStateIndex)
                {
                    animationClipId = state.AnimationClipId;
                    break;
                }
            }
            AnimationBuffer animationClip = new AnimationBuffer();
            foreach (var animation in Animations)
            {
                if (animation.AnimatorInstanceId == _animatorInstatnceId && animation.Id == animationClipId)
                {
                    animationClip = animation;
                    break;
                }
            }
            if (animationClip.Equals(default(AnimationBuffer)))
            {
                return;
            }

            // animation found let's animate
            // manage timers
            var animationDuration = animationClip.Length;
            var looped = animationClip.Looped;
            var currentTimer = layer.AnimationTime;
            if (looped && currentTimer > animationDuration)
            {
                currentTimer = currentTimer % animationDuration;
            }
            // loop each animation key
            bool fistFound = false;
            bool secondFound = false;
            var firstKey = new AnimationKeyBuffer();
            var secondKey = new AnimationKeyBuffer();
            foreach (var animationKey in AnimationKeys)
            {
                if (animationKey.AnimatorInstanceId == _animatorInstatnceId && animationKey.AnimationId == animationClipId)
                {
                    if (!secondFound && animationKey.Time > currentTimer)
                    {
                        secondKey = animationKey;
                        secondFound = true;
                    }
                }
            }
        }
    }
}