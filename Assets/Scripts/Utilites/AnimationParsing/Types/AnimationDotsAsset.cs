using UnityEngine;
using Utils.Parser;
using Utils.Parser;

[CreateAssetMenu(fileName = "New Dots Animator Asset", menuName = "Custom Assets/DOTS Animator")]
public class AnimationDotsAsset : ScriptableObject
{
    [HideInInspector]
    public string Content;
    public AnimationDotsObject AnimationDOTSObject;
}
