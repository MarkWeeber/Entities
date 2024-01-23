using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class KeyFrameStore : MonoBehaviour
{
    public string SavePath = "Assets/Animations/CustomKeyFrames/example";
    private string savePath;
    public float KeyFrameTime = 0f;
    private float keyFrameTime;
    private int keyFrames = 0;
    private KeyFrameStorage keyFrameStorage = new KeyFrameStorage { Store = new List<KeyFrameList>()};

    public void ResetKeyFrames()
    {
        keyFrameStorage.Store.Clear();
        keyFrames = 0;
    }

    public void StoreKeyFrame()
    {
        Debug.Log("STORING, with time: " + KeyFrameTime.ToString());
        keyFrameTime = KeyFrameTime;
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
                Time = keyFrameTime,
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
        keyFramesAsset.name = "testNew";
        keyFramesAsset.Content = JsonUtility.ToJson(keyFrameStorage);
        Debug.Log(keyFramesAsset.Content);
        AssetDatabase.CreateAsset(keyFramesAsset, savePath + ".asset");
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