using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[BurstCompile]
public partial struct AnimationSystem : ISystem
{
    private float timer;
    private BufferLookup<AnimationPartComponent> animationPartLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        timer = 0f;
        state.RequireForUpdate<AnimationBaseComponent>();
        animationPartLookup = state.GetBufferLookup<AnimationPartComponent>(isReadOnly: true);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        if (SystemAPI.TryGetSingleton<SystemControllerData>(out SystemControllerData systemControllerData))
        {
            if (!systemControllerData.Animation)
            {
                return;
            }
            else if (systemControllerData.AnimationRate > 0f)
            {
                if (timer < 0f)
                {
                    timer = systemControllerData.AnimationRate;
                }
                else
                {
                    timer -= deltaTime;
                    return;
                }
            }
        }
        else
        {
            return;
        }
        DynamicBuffer<AnimationBaseComponent> animationBaseBuffer = SystemAPI.GetSingletonBuffer<AnimationBaseComponent>();
        animationPartLookup.Update(ref state);


    }
    [BurstCompile]
    [WithAll(typeof(AnimationActorComponent))]
    [WithAll(typeof(AnimationPartComponent))]
    private partial struct AnimationsJob : IJobEntity
    {
        [ReadOnly]
        public DynamicBuffer<AnimationBaseComponent> AnimationBases;
        [ReadOnly]
        public BufferLookup<AnimationPartComponent> AnimationPartLookup;
        public float DeltaTime;
        private void Execute(Entity entiy)
        {
            if (AnimationPartLookup.TryGetBuffer(entiy, out DynamicBuffer<AnimationPartComponent> animationPartComponents))
            {
                for (int i = 0; i < animationPartComponents.Length; i++)
                {

                }
            }
        }
    }
}