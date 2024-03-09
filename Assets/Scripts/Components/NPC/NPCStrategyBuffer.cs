using Unity.Entities;
using Unity.Collections;

[System.Serializable]
public struct NPCStrategyBuffer : IBufferElementData
{
    public bool Active;
    public float StrategyValue;
    public float MinWaitTime;
    public float MaxWaitTime;
    public float MinCastRadiousForNextDestination;
    public float MaxCastRadiousForNextDestination;
    public NPCStrategyType strategyType;
}

public enum NPCStrategyType
{
    Wander = 0,
    Attach = 1
}