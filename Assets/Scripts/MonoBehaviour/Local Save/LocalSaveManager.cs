using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalSaveManager : SingletonBehaviour<LocalSaveManager>, SaveDataSender
{
    private const string saveFileNameEncrypted = "save.dat";
    private const string saveFileNameSimple = "save.txt";
    private const string passPhrase = "f92ksa0-1";
    [SerializeField] private float saveTime = 4.0f;
    
    [SerializeField] private SaveData saveData;
    public SaveData SaveData { get => saveData; }

    public event Action<SaveDataSender, SaveData> OnSaveDataSetEvent;
    public event Action OnSystemBaseUpdate;
    public bool SaveDataPersists;

    private float saveTimer;
    private string pathEncrypted;
    private string pathSimple;

    protected override async void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
        pathEncrypted = Application.persistentDataPath + "/" + saveFileNameEncrypted;
        pathSimple = Application.persistentDataPath + "/" + saveFileNameSimple;
        await RetreiveSaveDataLocal();
        saveTimer = saveTime;
    }

    private void OnApplicationQuit()
    {
        OnExit();
    }

    private void OnDestroy()
    {
        OnExit();
    }

    private async void Update()
    {
        if (saveTime > 0f)
        {
            if (saveTimer < 0f)
            {
                await Save(false);
                saveTimer = saveTime;
            }
            else
            {
                saveTimer -= Time.deltaTime;
            }
        }
    }

    private async void OnExit()
    {
        await Save();
        SaveDataPersists = false;
        if (OnSaveDataSetEvent != null)
        {
            foreach (Action<SaveDataSender, SaveData> _delegate in OnSaveDataSetEvent.GetInvocationList())
            {
                OnSaveDataSetEvent -= _delegate;
            }
        }
    }

    private async Task Save(bool clearDelegates = true)
    {
        if (OnSystemBaseUpdate != null)
        {
            OnSystemBaseUpdate.Invoke();
            if (clearDelegates)
            {
                foreach (Action _delegate in OnSystemBaseUpdate.GetInvocationList())
                {
                    OnSystemBaseUpdate -= _delegate;
                }
            }
        }
        await SaveDataLocal();
    }

    private async Task RetreiveSaveDataLocal()
    {
        string ecryptedText = string.Empty;
        if (File.Exists(pathEncrypted))
        {
            StreamReader streamReaderEncrypted = File.OpenText(pathEncrypted);
            ecryptedText = await streamReaderEncrypted.ReadToEndAsync();
            streamReaderEncrypted.Close();
            try
            {
                string jsonText = StringCipher.Decrypt(ecryptedText, passPhrase);
                saveData = JsonUtility.FromJson<SaveData>(jsonText);
                InfoUI.Instance.SendInformation("SAVE FILE LOAD SUCCESS", MessageType.SUCCESS);
            }
            catch (Exception e)
            {
                await CreateNewSaveData();
                InfoUI.Instance.SendInformation("COULD NOT PARSE", MessageType.WARNING);
                Debug.LogWarning(e.Message);
            }
        }
        else
        {
            InfoUI.Instance.SendInformation("SAVE FILE NOT FOUND, CREATING DEFAULT VALUES", MessageType.WARNING);
            await CreateNewSaveData();
        }
        SaveDataPersists = true;
    }

    private async Task SaveDataLocal()
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
            await streamWriterEncrypted.WriteAsync(ecryptedText);
            streamWriterEncrypted.Close();
        }
        catch (Exception e)
        {
            InfoUI.Instance.SendInformation("COULD NOT SAVE", MessageType.ERROR);
            InfoUI.Instance.SendInformation(e.Message, MessageType.ERROR);
        }
    }

    public async Task CreateNewSaveData()
    {
        saveData = new SaveData()
        {
            DateTime = "01.01.1900 00:00:00",
            CoinsCollected = 0,
            CurrentHealth = 100f
        };
        await SaveDataLocal();
        OnSaveDataSetEvent?.Invoke(this, saveData);
    }

    public void SetSaveData(SaveDataSender sender, SaveData _saveData)
    {
        saveData = _saveData;
        saveData.DateTime = DateTime.Now.ToString();
        OnSaveDataSetEvent?.Invoke(sender, saveData);
    }
}