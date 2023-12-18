using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

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
        UnityEngine.Debug.Log("Cleared");
    }
    
    [MenuItem("Local Save/Open Save Folder")]
    public static void OpenSaveFolder()
    {
        string path = Application.persistentDataPath;
        path = path.Replace("/", @"\");
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Arguments = path,
            FileName = "explorer.exe"
        };
        Process.Start(startInfo);
    }
}
