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