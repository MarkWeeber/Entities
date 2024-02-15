using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct AnimatorActorParametersBuffer : IBufferElementData
{
    public FixedString32Bytes ParameterName;
    public AnimatorControllerParameterType Type;
    public float NumericValue;
    public bool BoolValue;
}