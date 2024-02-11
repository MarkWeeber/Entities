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
    private ComponentLookup<AnimatorActorComponent> animatorActorComponentLookup;
    private BufferLookup<AnimatorActorLayerBuffer> animatorLayerBufferLookup;
    private BufferLookup<AnimatorActorTransitionBuffer> animatorTransitionBufferLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false;
        state.RequireForUpdate<AnimatorBuffer>();
        state.RequireForUpdate<AnimatorActorComponent>();
        animatorActorComponentLookup = state.GetComponentLookup<AnimatorActorComponent>(true);
        animatorLayerBufferLookup = state.GetBufferLookup<AnimatorActorLayerBuffer>(true);
        animatorTransitionBufferLookup = state.GetBufferLookup<AnimatorActorTransitionBuffer>(true);
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
            NativeArray<StateTransitionBuffer> transitions = SystemAPI.GetSingletonBuffer<StateTransitionBuffer>().AsNativeArray();
            NativeArray<TransitionCondtionBuffer> conditions = SystemAPI.GetSingletonBuffer<TransitionCondtionBuffer>().AsNativeArray();
            
            EntityQuery actorPartsQuery = SystemAPI.QueryBuilder()
                .WithAll<AnimatorActorPartComponent, LocalTransform>()
                .Build();

            animatorActorComponentLookup.Update(ref state);
            animatorLayerBufferLookup.Update(ref state);
            animatorTransitionBufferLookup.Update(ref state);

            float deltaTime = SystemAPI.Time.DeltaTime;

            state.Dependency = new AnimateActorJob
            {
                AnimatorActorComponentLookup = animatorActorComponentLookup,
                AnimatorLayerBufferLookup = animatorLayerBufferLookup,
                AnimatorTransitionBufferLookup = animatorTransitionBufferLookup,
                Animators = animators,
                Animations = animations,
                Layers = layers,
                States = states,
                Transitions = transitions,
                Conditions = conditions,
                DeltaTime = deltaTime,
            }.ScheduleParallel(actorPartsQuery, state.Dependency);
            state.Dependency.Complete();

            animators.Dispose();
            animations.Dispose();
            layers.Dispose();
            states.Dispose();
            transitions.Dispose();
            conditions.Dispose();
        }
    }

    [BurstCompile]
    private partial struct AnimateActorJob : IJobEntity
    {
        [ReadOnly]
        public ComponentLookup<AnimatorActorComponent> AnimatorActorComponentLookup;
        [ReadOnly]
        public BufferLookup<AnimatorActorLayerBuffer> AnimatorLayerBufferLookup;
        [ReadOnly]
        public BufferLookup<AnimatorActorTransitionBuffer> AnimatorTransitionBufferLookup;
        [ReadOnly]
        public NativeArray<AnimatorBuffer> Animators;
        [ReadOnly]
        public NativeArray<AnimationBuffer> Animations;
        [ReadOnly]
        public NativeArray<AnimatorLayerBuffer> Layers;
        [ReadOnly]
        public NativeArray<LayerStateBuffer> States;
        [ReadOnly]
        public NativeArray<StateTransitionBuffer> Transitions;
        [ReadOnly]
        public NativeArray<TransitionCondtionBuffer> Conditions;
        public float DeltaTime;
        [BurstCompile]
        private void Execute([ChunkIndexInQuery] int sortKey, Entity entity, RefRW<LocalTransform> localTransform, RefRO<AnimatorActorPartComponent> animatorActorPartComponent)
        {
            Entity rootEntity = animatorActorPartComponent.ValueRO.RootEntity;
            FixedString512Bytes path = animatorActorPartComponent.ValueRO.Path;
            // getting necessary buffers from root entity
            RefRO<AnimatorActorComponent> animatorActorComponent = new RefRO<AnimatorActorComponent>();
            DynamicBuffer<AnimatorActorLayerBuffer> animatorActorLayerBuffers = new DynamicBuffer<AnimatorActorLayerBuffer>();
            DynamicBuffer<AnimatorActorTransitionBuffer> animatorActorTransitionBuffers = new DynamicBuffer<AnimatorActorTransitionBuffer>();
            if (AnimatorActorComponentLookup.HasComponent(rootEntity))
            {
                animatorActorComponent = AnimatorActorComponentLookup.GetRefRO(rootEntity);
            }
            if (AnimatorLayerBufferLookup.HasBuffer(rootEntity))
            {
                animatorActorLayerBuffers = AnimatorLayerBufferLookup[rootEntity];
            }
            if (AnimatorTransitionBufferLookup.HasBuffer(rootEntity))
            {
                animatorActorTransitionBuffers = AnimatorTransitionBufferLookup[rootEntity];
            }

            NativeArray<AnimatorActorLayerBuffer> animatorActorLayers = animatorActorLayerBuffers.AsNativeArray();
            NativeArray<AnimatorActorTransitionBuffer> animatorActorTransitions = animatorActorTransitionBuffers.AsNativeArray();
            // get animator id
            int animatorInstanceId = animatorActorComponent.ValueRO.AnimatorId;

            // animating each layer
            for (int i = 0; i < animatorActorLayerBuffers.Length; i++)
            {
                var layer = animatorActorLayerBuffers[i];
                float currentAnimationTime = layer.AnimationTime;
                // get current state id
                int currentStateIndex = layer.CurrentStateIndex;
                // get state's animation id
                int animationClipId = -1;
                foreach (var state in States)
                {
                    if (state.AnimatorInstanceId == animatorInstanceId && state.LayerId == layer.Id) // check corresponding layer data that matches our actor
                    {
                        animationClipId = state.AnimationClipId;
                        break;
                    }
                }
                // get necessary curves ids
                float fromTime = -1f;
                float toTime = -1f;
                bool positionEngaged = false;
                bool rotationEngaged = false;
                quaternion fromRotation = quaternion.identity;
                quaternion toRotation = quaternion.identity;
                float3 fromPosition = float3.zero;
                float3 toPosition = float3.zero;
                int3 positionCurveIds = int3.zero;
                int4 rotationCurveIds = int4.zero;
                bool3 positionApplyMatrix = new bool3(false, false, false);
                bool4 rotationApplyMatrix = new bool4(false, false, false, false);
                
                // key frames

                // apply transitioning values
                if (toTime < 0f)
                {
                    toPosition = fromPosition;
                    toRotation = fromRotation;
                    float excessTime = currentAnimationTime - fromTime;
                    layer.AnimationTime = excessTime - DeltaTime;
                }
                float rate = (currentAnimationTime - fromTime) / (toTime - fromTime);
                float3 newPosition = math.lerp(fromPosition, toPosition, rate);
                quaternion newRotation = math.slerp(fromRotation, toRotation, rate);
                if (positionEngaged)
                {
                    //localTransform.ValueRW.Position = newPosition;
                }
                if (rotationEngaged)
                {
                    //localTransform.ValueRW.Rotation = newRotation;
                }
                layer.AnimationTime += DeltaTime;
            }
        }

        [BurstCompile]
        private void SavePositioning(AnimationCurveKeyBuffer value, ref float3 position, int3 type)
        {
            if (value.CurveId == type.x)
            {
                position.x = value.Value;
            }
            if (value.CurveId == type.y)
            {
                position.y = value.Value;
            }
            if (value.CurveId == type.z)
            {
                position.z = value.Value;
            }
        }

        [BurstCompile]
        private void SaveRotation(AnimationCurveKeyBuffer value, ref quaternion rotation, int4 type)
        {
            if (value.CurveId == type.x)
            {
                rotation.value.x = value.Value;
            }
            if (value.CurveId == type.y)
            {
                rotation.value.y = value.Value;
            }
            if (value.CurveId == type.z)
            {
                rotation.value.z = value.Value;
            }
            if (value.CurveId == type.w)
            {
                rotation.value.w = value.Value;
            }
        }
    }

    [BurstCompile]
    private partial struct AnimatorActorLayerTimer : IJobEntity
    {
        private void Execute()
        {

        }
    }
}