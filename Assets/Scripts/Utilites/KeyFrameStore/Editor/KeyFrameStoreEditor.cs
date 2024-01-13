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

        if (GUILayout.Button("Store Key Frames"))
        {
            keyFrameStore.StoreKeyFrame();
        }

        if (GUILayout.Button("Complete Storing Key Frames"))
        {
            keyFrameStore.CompleteStoringKeyFrames();
        }
        #endregion
    }

}
