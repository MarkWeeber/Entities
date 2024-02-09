using UnityEngine;

[CreateAssetMenu(fileName = "New Dots Animator Asset", menuName = "Custom Assets/DOTS Animator")]
public class AnimatorDotsAsset : ScriptableObject
{
    public int AnimatorInstanceId;
    public string AnimatorName;
    public RuntimeAnimatorParsedObject RuntimeAnimatorParsedObject;
}
