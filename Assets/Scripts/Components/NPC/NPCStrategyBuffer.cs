using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Authoring;

[System.Serializable]
public struct NPCStrategyBuffer : IBufferElementData
{
    public bool Active;
    public float StrategyValue;
    public float StrategyMoveSpeedMultiplier;
    public float MinWaitTime;
    public float MaxWaitTime;
    public NPCStrategyType StrategyType;
    public PhysicsCategoryTags TargetCollider;
}

public enum NPCStrategyType
{
    NoStrategy = 0,
    LookForPlayer = 1,
    FleeForHealth = 2
}