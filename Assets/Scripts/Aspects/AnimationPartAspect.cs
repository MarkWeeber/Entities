using Unity.Entities;
using Unity.Transforms;

public readonly partial struct AnimationPartAspect : IAspect
{
    private readonly Entity _entity;
    private readonly RefRW<LocalTransform> localTransform;
    private readonly DynamicBuffer<AnimationPartPositionBuffer> positions;
    private readonly DynamicBuffer<AnimationPartRotationBuffer> rotations;
    private readonly RefRO<AnimatorActorPartComponent> partComponent;
    public void Animate()
    {

    }
}