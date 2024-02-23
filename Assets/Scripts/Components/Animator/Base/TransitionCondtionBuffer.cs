using Unity.Collections;
using Unity.Entities;

public partial struct TransitionCondtionBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public int TransitionId;
    public AnimatorTransitionConditionMode Mode;
    public FixedString32Bytes Parameter;
    public float Treshold;
}

[System.Serializable]
public struct TransitionCondtion
{
    public int Id;
    public int AnimatorInstanceId;
    public int TransitionId;
    public AnimatorTransitionConditionMode Mode;
    public string Parameter;
    public float Treshold;
}

[System.Serializable]
public enum AnimatorTransitionConditionMode
{
    //
    // Сводка:
    //     The condition is true when the parameter value is true.
    If = 1,
    //
    // Сводка:
    //     The condition is true when the parameter value is false.
    IfNot = 2,
    //
    // Сводка:
    //     The condition is true when parameter value is greater than the threshold.
    Greater = 3,
    //
    // Сводка:
    //     The condition is true when the parameter value is less than the threshold.
    Less = 4,
    //
    // Сводка:
    //     The condition is true when parameter value is equal to the threshold.
    Equals = 6,
    //
    // Сводка:
    //     The condition is true when the parameter value is not equal to the threshold.
    NotEqual = 7
}