using System;
using System.IO;
using UnityEngine;

public class LocalSaveManager : SingletonBehaviour<LocalSaveManager>
{
    private const string saveFileNameEncrypted = "save.dat";
    private const string saveFileNameSimple = "save.txt";
    private const string passPhrase = "f92ksa0-1";

    [SerializeField] private SaveData saveData;
    public SaveData SaveData { get => saveData; set => saveData = value; }
    [SerializeField] private float saveTime = 4.0f;
    private float saveTimer;
    public bool Active;
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
        Active = true;
    }

    private void OnApplicationQuit()
    {
        Save();
        Active = false;
    }

    private void OnDestroy()
    {
        Save();
        Active = false;
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

    private void RetreiveSaveDataLocal()
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
                SaveDataLocal();
                InfoUI.Instance.SendInformation("COULD NOT PARSE", MessageType.WARNING);
                Debug.LogWarning(e.Message);
            }
        }
        else
        {
            InfoUI.Instance.SendInformation("SAVE FILE NOT FOUND, CREATING DEFAULT VALUES", MessageType.WARNING);
            CreateNewSaveData();
            SaveDataLocal();
        }
    }

    private void SaveDataLocal()
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
            DateTime = "01.01.1900 00:00:00",
            CoinsCollected = 0,
            CurrentHealth = 100f
        };
    }

    public void SetSaveData(uint coinsCollected, float currentHealth)
    {
        saveData.DateTime = DateTime.Now.ToString();
        saveData.CoinsCollected = coinsCollected;
        saveData.CurrentHealth = currentHealth;
    }
}