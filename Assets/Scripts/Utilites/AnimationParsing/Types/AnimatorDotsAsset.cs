using UnityEngine;
using Utils.Parser;

[CreateAssetMenu(fileName = "New Dots Animator Asset", menuName = "Custom Assets/DOTS Animator")]
public class AnimatorDotsAsset : ScriptableObject
{
    [HideInInspector]
    public string Content;
    public AnimatorDotsObject AnimatorDOTSObject;
}
