using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalSaveManager : SingletonBehaviour<LocalSaveManager>
{
    private const string saveFileNameEncrypted = "save.dat";
    private const string saveFileNameSimple = "save.txt";
    private const string passPhrase = "f92ksa0-1";

    [SerializeField] private SaveData saveData;
    public SaveData SaveData { get => saveData; set => saveData = value; }

    public event Action OnDestroyEvent;

    private string pathEncrypted;
    private string pathSimple;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        pathEncrypted = Application.persistentDataPath + "/" + saveFileNameEncrypted;
        pathSimple = Application.persistentDataPath + "/" + saveFileNameSimple;
        RetreiveSaveDataLocalAsync();
    }

    private void OnDestroy()
    {
        if (OnDestroyEvent != null)
        {
            OnDestroyEvent.Invoke();
            foreach (Action _delegate in OnDestroyEvent.GetInvocationList())
            {
                OnDestroyEvent -= _delegate;
            }
        }
        SaveDataLocalAsync();
        Debug.Log("Saved");
    }

    public void RetreiveSaveDataLocalAsync()
    {
        string ecryptedText = string.Empty;
        if (File.Exists(pathEncrypted))
        {
            StreamReader streamReaderEncrypted = File.OpenText(pathEncrypted);
            ecryptedText = streamReaderEncrypted.ReadToEnd();
            streamReaderEncrypted.Close();
            try
            {
                string jsonText = StringCipher.Decrypt(ecryptedText, passPhrase);
                saveData = JsonUtility.FromJson<SaveData>(jsonText);
            }
            catch (System.Exception)
            {
                CreateNewSaveData();
                Debug.LogWarning("Could not parse save data!");
            }
        }
        else
        {
            CreateNewSaveData();
            SaveDataLocalAsync();
        }
    }

    public void SaveDataLocalAsync()
    {
        // simple
        string jsonText = JsonUtility.ToJson(saveData);
        StreamWriter streamWriterSimple = File.CreateText(pathSimple);
        streamWriterSimple.Write(jsonText);
        streamWriterSimple.Close();
        // encrypted
        string ecryptedText = StringCipher.Encrypt(jsonText, passPhrase);
        StreamWriter streamWriterEncrypted = File.CreateText(pathEncrypted);
        streamWriterEncrypted.Write(ecryptedText);
        streamWriterEncrypted.Close();
    }

    private void CreateNewSaveData()
    {
        saveData = new SaveData()
        {
            CoinsCollected = 0,
            CurrentHealth = 100f
        };
    }
}