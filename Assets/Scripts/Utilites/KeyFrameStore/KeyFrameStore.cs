using UnityEngine;

public class KeyFrameStore : MonoBehaviour
{
    private int keyFrames;
    public void StoreKeyFrame()
    {
        Debug.Log("STORING");
        PrintAllChildrenTransforms();
    }

    public void ResetKeyFrames()
    {
        keyFrames = 0;
    }
    
    public void CompleteStoringKeyFrames()
    {
        Debug.Log("Gathered keyframe count: " +  keyFrames);
    }

    private void PrintAllChildrenTransforms()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
        {
            Debug.Log(t.name + " : " +t.localPosition);
        }
        keyFrames++;
    }
}
