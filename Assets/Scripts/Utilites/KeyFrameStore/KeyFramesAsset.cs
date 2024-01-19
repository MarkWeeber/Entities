using UnityEngine;

[CreateAssetMenu(fileName = "New Key Frames Asset", menuName = "Custom Assets/Key Frames Asset")]
public class KeyFramesAsset : ScriptableObject
{
    [TextArea(10, 40)]
    public string Content;
}
