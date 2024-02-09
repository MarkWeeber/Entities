using Unity.Collections;
using Unity.Entities;

[System.Serializable]
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