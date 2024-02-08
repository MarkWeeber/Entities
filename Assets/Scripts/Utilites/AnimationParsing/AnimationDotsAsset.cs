using UnityEngine;
using Utils.Parse;
using Utils.Parser;

[CreateAssetMenu(fileName = "New Dots Animator Asset", menuName = "Custom Assets/DOTS Animator")]
public class AnimationDotsAsset : ScriptableObject
{
    [HideInInspector]
    public string Content;
    public AnimationDOTSObject AnimationDOTSObject;
}
