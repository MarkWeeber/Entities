using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct AnimatorParameterBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public FixedString32Bytes ParameterName;
    public AnimatorControllerParameterType Type;
    public float DefaultFloat;
    public int DefaultInt;
    public bool DefaultBool;
}