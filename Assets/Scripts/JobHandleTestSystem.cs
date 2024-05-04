using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public partial struct JobHandleTestSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //var AJobHandle = new JobA().Schedule(state.Dependency);
        //var BJobHandle = new JobB().Schedule(AJobHandle);
        //var CJobHandle = new JobC().Schedule(BJobHandle);
        var parallelJobHandleA = new ParallelJobA().Schedule(5, 1, state.Dependency);
        var parallelJobHandleB = new ParallelJobB().Schedule(5, 1, parallelJobHandleA);
        var parallelJobHandleC = new ParallelJobC().Schedule(5, 1, parallelJobHandleB);
        state.Enabled = false;
    }

    [BurstCompile]
    private partial struct JobA : IJob
    {
        [BurstCompile]
        public void Execute()
        {
            //Debug.Log("A");
        }
    }

    [BurstCompile]
    private partial struct JobB : IJob
    {
        [BurstCompile]
        public void Execute()
        {
            //Debug.Log("B");
        }
    }

    [BurstCompile]
    private partial struct JobC : IJob
    {
        [BurstCompile]
        public void Execute()
        {
            //Debug.Log("C");
        }
    }

    [BurstCompile]
    private partial struct ParallelJobA : IJobParallelFor
    {
        [BurstCompile]
        public void Execute(int index)
        {
            //Debug.Log("A: " + index);
        }
    }

    [BurstCompile]
    private partial struct ParallelJobB : IJobParallelFor
    {
        [BurstCompile]
        public void Execute(int index)
        {
            //Debug.Log("B: " + index);
        }
    }

    [BurstCompile]
    private partial struct ParallelJobC : IJobParallelFor
    {
        [BurstCompile]
        public void Execute(int index)
        {
            //Debug.Log("C: " + index);
        }
    }
}