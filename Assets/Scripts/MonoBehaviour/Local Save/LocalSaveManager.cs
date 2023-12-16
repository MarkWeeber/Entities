using System;
using System.IO;
using UnityEngine;

public class LocalSaveManager : SingletonBehaviour<LocalSaveManager>
{
    private const string saveFileNameEncrypted = "save.dat";
    private const string saveFileNameSimple = "save.txt";
    private const string passPhrase = "f92ksa0-1";

    [SerializeField] private SaveData saveData;
    [SerializeField] private float saveTime = 4.0f;
    private float saveTimer;
    public SaveData SaveData { get => saveData; set => saveData = value; }
    public event Action OnDestroyEvent;

    private string pathEncrypted;
    private string pathSimple;

    protected override void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
        pathEncrypted = Application.persistentDataPath + "/" + saveFileNameEncrypted;
        pathSimple = Application.persistentDataPath + "/" + saveFileNameSimple;
    }

    private void Start()
    {
        RetreiveSaveDataLocal();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDestroy()
    {
        Save();
    }

    private void Update()
    {
        if (saveTime > 0f)
        {
            if (saveTimer <= 0f)
            {
                Save(false);
                saveTimer = saveTime;
            }
            else
            {
                saveTimer -= Time.deltaTime;
            }
        }
    }

    private void Save(bool clearDelegates = true)
    {
        if (OnDestroyEvent != null)
        {
            OnDestroyEvent.Invoke();
            if (clearDelegates)
            {
                foreach (Action _delegate in OnDestroyEvent.GetInvocationList())
                {
                    OnDestroyEvent -= _delegate;
                }
            }
        }
        SaveDataLocal();
    }

    public void RetreiveSaveDataLocal()
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
                InfoUI.Instance.SendInformation("SUCCESS", MessageType.SUCCESS);
            }
            catch (Exception e)
            {
                CreateNewSaveData();
                InfoUI.Instance.SendInformation("COULD NOT PARSE", MessageType.WARNING);
                InfoUI.Instance.SendInformation(e.Message, MessageType.WARNING);
                Debug.LogWarning("Could not parse save data!");
            }
        }
        else
        {
            InfoUI.Instance.SendInformation("FILE NOT FOUND", MessageType.WARNING);
            CreateNewSaveData();
            SaveDataLocal();
        }
    }

    public void SaveDataLocal()
    {
        try
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
        catch (Exception e)
        {
            InfoUI.Instance.SendInformation("COULD NOT SAVE", MessageType.ERROR);
            InfoUI.Instance.SendInformation(e.Message, MessageType.ERROR);
        }
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