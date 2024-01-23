using UnityEngine;

[CreateAssetMenu(fileName = "New Key Frames Asset", menuName = "Custom Assets/Key Frames Asset")]
public class KeyFramesAsset : ScriptableObject
{
    [HideInInspector]
    public string Content;
}
