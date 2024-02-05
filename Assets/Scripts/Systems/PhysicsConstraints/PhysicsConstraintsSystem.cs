using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PhysicsConstraintsSystem : ISystem
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
        EntityCommandBuffer.ParallelWriter parallelWriter = ecb.AsParallelWriter();
        EntityQuery entityQuery = SystemAPI.QueryBuilder()
            .WithAll<PhysicsConstraintComponent, LocalToWorld>()
            .Build();
        JobHandle jobHandle = new AddConstraintsJob
        {
            ParallelWriter = parallelWriter
        }.ScheduleParallel(entityQuery, state.Dependency);
        jobHandle.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public partial struct AddConstraintsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ParallelWriter;
        [BurstCompile]
        private void Execute(
            [ChunkIndexInQuery] int sortKey,
            Entity entity,
            RefRO<PhysicsConstraintComponent> physicsConstraintComponent,
            RefRO<LocalToWorld> localToWorld)
        {
            RigidTransform rigidTransform = new RigidTransform
            {
                pos = localToWorld.ValueRO.Position,
                rot = localToWorld.ValueRO.Rotation
            };
            BodyFrame bodyFrame = new BodyFrame(rigidTransform);
            PhysicsJoint joint = new PhysicsJoint
            {
                BodyAFromJoint = bodyFrame
            };
            FixedList128Bytes<Constraint> constraints = new FixedList128Bytes<Constraint>
            {
                //new Constraint
                //{
                //    ConstrainedAxes = physicsConstraintComponent.ValueRO.LinearConstrains,
                //    Type = ConstraintType.Linear,
                //    Min = 0,
                //    Max = 0,
                //    SpringFrequency = Constraint.DefaultSpringFrequency,
                //    SpringDamping = Constraint.DefaultSpringDamping,
                //    Target = localToWorld.ValueRO.Position
                //},
                new Constraint
                {
                    ConstrainedAxes = physicsConstraintComponent.ValueRO.AngularConstrains,
                    Type = ConstraintType.Angular,
                    Min = 0,
                    Max = 0,
                    SpringFrequency = Constraint.DefaultSpringFrequency,
                    SpringDamping = Constraint.DefaultSpringDamping
                }
            };
            joint.SetConstraints(constraints);
            ParallelWriter.AddComponent(sortKey, entity, joint);
            PhysicsConstrainedBodyPair pcbp = new PhysicsConstrainedBodyPair(entity, default, false);
            ParallelWriter.AddComponent(sortKey, entity, pcbp);
            ParallelWriter.SetComponentEnabled<PhysicsConstraintComponent>(sortKey, entity, false);

        }
    }
}