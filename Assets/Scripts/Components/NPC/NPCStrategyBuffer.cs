using Unity.Entities;
using Unity.Collections;
using Unity.Physics.Authoring;

[System.Serializable]
public struct NPCStrategyBuffer : IBufferElementData
{
    public bool Active;
    public float StrategyValue;
    public float MinWaitTime;
    public float MaxWaitTime;
    public NPCStrategyType strategyType;
    public PhysicsCategoryTags TargetCollider;
}

public enum NPCStrategyType
{
    Wander = 0,
    Attack = 1,
    LookForHealth = 2
}