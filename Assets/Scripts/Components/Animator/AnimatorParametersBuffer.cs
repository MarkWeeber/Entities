using Unity.Collections;
using Unity.Entities;

public partial struct AnimatorParametersBuffer : IBufferElementData
{
    public int Id;
    public int AnimatorInstanceId;
    public FixedString32Bytes ParameterName;
    public UnityEngine.AnimatorControllerParameterType Type;
    public float DefaultFloat;
    public int DefaultInt;
    public bool DefaultBool;
}

[System.Serializable]
public struct AnimatorParameter
{
    public int Id;
    public int AnimatorInstanceId;
    public string ParameterName;
    public UnityEngine.AnimatorControllerParameterType Type;
    public float DefaultFloat;
    public int DefaultInt;
    public bool DefaultBool;
}