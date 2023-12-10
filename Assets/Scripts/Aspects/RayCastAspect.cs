using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

public readonly partial struct RayCastAspect : IAspect
{
    private readonly Entity entity1;
    private readonly RefRW<RayCastData> rayCastData;
    private readonly RefRO<LocalTransform> localToWorld;
    private readonly RefRO<RayCasterTag> rayCasterTag;

    public void RayCast(CollisionWorld collisionWorld, Entity entity)
    {
        RaycastInput raycastInput = new RaycastInput()
        {
            Start = localToWorld.ValueRO.Position + (rayCastData.ValueRO.StartRayOffest * localToWorld.ValueRO.Forward()),
            End = localToWorld.ValueRO.Position + (localToWorld.ValueRO.Forward() * rayCastData.ValueRO.RayDistance),
            Filter = rayCastData.ValueRO.CollisionFilter
        };
        RaycastHit rayCastHit = new RaycastHit();
        NativeList<RaycastHit> raycastHits = new NativeList<RaycastHit>(2, Allocator.Temp);
        if(collisionWorld.CastRay(raycastInput, ref raycastHits))
        {
            for(int i = 0; i < raycastHits.Length; i++)
            {
                if (raycastHits[i].Entity != entity1)
                {
                    rayCastHit = raycastHits[i];
                    break;
                }
            }
        }
        rayCastData.ValueRW.RaycastHit = rayCastHit;
        raycastHits.Dispose();
    }
}