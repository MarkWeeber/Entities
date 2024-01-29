using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KeyFrameStore))]
public class KeyFrameStoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        KeyFrameStore keyFrameStore = (KeyFrameStore)target;

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((KeyFrameStore)target), typeof(KeyFrameStore), false);
        GUI.enabled = true;

        if (GUILayout.Button("Reset Key Frames"))
        {
            keyFrameStore.ResetKeyFrames();
        }
        GUILayout.Label("Key Frame Percent");
        keyFrameStore.KeyFramePercent = EditorGUILayout.FloatField(keyFrameStore.KeyFramePercent, new GUIStyle(EditorStyles.numberField));
        if (GUILayout.Button("Store One Key Frame"))
        {
            keyFrameStore.StoreKeyFrame();
        }
        GUILayout.Label("Total Animation Duration Time");
        keyFrameStore.TotalTime = EditorGUILayout.FloatField(keyFrameStore.TotalTime, new GUIStyle(EditorStyles.numberField));

        GUIStyle style = new GUIStyle(EditorStyles.textField);
        GUILayout.Label("Save file path");
        keyFrameStore.SavePath = EditorGUILayout.TextField(keyFrameStore.SavePath, style);
        keyFrameStore.AnimationName = EditorGUILayout.TextField(keyFrameStore.AnimationName, style);
        if (GUILayout.Button("Complete Storing Key Frames"))
        {
            keyFrameStore.CompleteStoringKeyFrames();
        }
    }
}
