using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KeyFrameStore))]
public class KeyFrameStoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        KeyFrameStore keyFrameStore = (KeyFrameStore)target;
        #region Script:
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((KeyFrameStore)target), typeof(KeyFrameStore), false);
        GUI.enabled = true;
        #endregion Script.

        #region buttons
        if (GUILayout.Button("Reset Key Frames"))
        {
            keyFrameStore.ResetKeyFrames();
        }
        GUILayout.Label("Frame Time");
        keyFrameStore.KeyFrameTime = EditorGUILayout.FloatField(keyFrameStore.KeyFrameTime, new GUIStyle(EditorStyles.numberField));
        if (GUILayout.Button("Store One Key Frame"))
        {
            keyFrameStore.StoreKeyFrame();
        }
        
        #region input textfield
        GUIStyle style = new GUIStyle(EditorStyles.textField);
        GUILayout.Label("Save file path");
        keyFrameStore.SavePath = EditorGUILayout.TextField(keyFrameStore.SavePath, style);
        #endregion

        if (GUILayout.Button("Complete Storing Key Frames"))
        {
            keyFrameStore.CompleteStoringKeyFrames();
        }
        #endregion
    }
}
