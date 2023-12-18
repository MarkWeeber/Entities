using UnityEngine;
using UnityEditor;
using System.IO;

public class LocalSaveMenu
{

    [MenuItem("Local Save/Clear Persistent Data")]
    public static void ClearPersistentData()
    {
        foreach (var file in Directory.GetFiles(Application.persistentDataPath))
                {
                    FileInfo file_info = new FileInfo(file);
                    file_info.Delete();
                }
        Debug.Log("Cleared");
    }
}
