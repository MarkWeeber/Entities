using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct AnimatorActorParametersComponent : IBufferElementData
{
    public FixedString32Bytes ParameterName;
    public AnimatorControllerParameterType Type;
    public float Value;
}