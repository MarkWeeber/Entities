using UnityEngine;

[CreateAssetMenu(fileName = "New Key Frames Asset", menuName = "Custom Assets/Key Frames Asset")]
public class KeyFramesAsset : ScriptableObject
{
    public string AnimationName;
    public bool Loop;
    [HideInInspector]
    public string Content;
    
}
