using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class KeyFrameStore : MonoBehaviour
{
    public string SavePath = "Assets/Animations/CustomKeyFrames/";
    private string savePath;
    public string AnimationName = "";
    public float TotalTime = 0f;
    private float totalTime = 0f;
    public float KeyFramePercent = 0f;
    private float keyFramePercent;
    private int keyFrames = 0;
    private KeyFrameStorage keyFrameStorage = new KeyFrameStorage { Store = new List<KeyFrameList>()};

    public void ResetKeyFrames()
    {
        keyFrameStorage.Store.Clear();
        keyFrames = 0;
    }

    public void StoreKeyFrame()
    {
        Debug.Log("STORING, with : " + KeyFramePercent.ToString() + " percent, and with total time: " + TotalTime.ToString());
        keyFramePercent = KeyFramePercent;
        totalTime = TotalTime;
        StoreAllChildrenTransformData();
    }

    public void CompleteStoringKeyFrames()
    {
        Debug.Log("Gathered keyframe count: " +  keyFrames);
        savePath = SavePath;
        SaveKeyFramesAsset();
    }

    private void StoreAllChildrenTransformData()
    {
        KeyFrameList _keyFrameStore = new KeyFrameList { Keys = new List<KeyFrameComponent>()};
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
        {
            _keyFrameStore.Keys.Add(new KeyFrameComponent
            {
                Time = keyFramePercent * totalTime,
                Name = (FixedString32Bytes)t.name,
                Position = t.localPosition,
                Rotation = new float4(t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w)
            });
        }
        keyFrameStorage.Store.Add(_keyFrameStore);
        keyFrames++;
    }

    private void SaveKeyFramesAsset()
    {
        KeyFramesAsset keyFramesAsset = ScriptableObject.CreateInstance<KeyFramesAsset>();
        keyFramesAsset.name = AnimationName;
        keyFramesAsset.AnimationName = AnimationName;
        keyFramesAsset.AnimationDuration = TotalTime;
        keyFramesAsset.Content = JsonUtility.ToJson(keyFrameStorage);
        AssetDatabase.CreateAsset(keyFramesAsset, savePath + AnimationName + ".asset");
    }
}

[System.Serializable]
public struct KeyFrameStorage
{
    public List<KeyFrameList> Store;
}

[System.Serializable]
public struct KeyFrameList
{
    public List<KeyFrameComponent> Keys;
}