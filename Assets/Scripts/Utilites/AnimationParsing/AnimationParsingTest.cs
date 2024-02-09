using UnityEngine;
using Utils.Parser;

public class AnimationParsingTest : MonoBehaviour
{
    [SerializeField] private AnimatorDotsObject animatorDotsObject;
    private void Start()
    {
        for (int i = 0; i < animatorDotsObject.AnimatorState.Length; i++)
        {
            var fileId = animatorDotsObject.AnimatorState[i].fileID;
            Debug.Log(fileId);
        }
    }
}
