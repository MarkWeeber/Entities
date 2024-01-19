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
    private int keyFrames = 0;

    private string content = "";
    private KeyFrameStorage keyFrameStorage;
    private KeyFrameList keyFrameStore;
    public void StoreKeyFrame()
    {
        Debug.Log("STORING");
        StoreAllChildrenTransformData();
    }

    public void ResetKeyFrames()
    {
        keyFrameStorage.Store = new List<KeyFrameList>();
        keyFrameStore.Keys = new List<KeyFrameComponent>();
        keyFrames = 0;
        content = "";
    }
    
    public void CompleteStoringKeyFrames()
    {
        Debug.Log("Gathered keyframe count: " +  keyFrames);
        savePath = SavePath;
        Debug.Log(savePath);
        SaveKeyFramesAsset();
    }

    private void StoreAllChildrenTransformData()
    {
        keyFrameStore.Keys.Clear();
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms)
        {
            content += t.name + " : " + t.localPosition;
            keyFrameStore.Keys.Add(new KeyFrameComponent
            {
                Name = (FixedString32Bytes)t.name,
                Position = t.localPosition,
                Rotation = new float4(t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w)
            });
        }
        Debug.Log(content);
        keyFrameStorage.Store.Add(keyFrameStore);
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