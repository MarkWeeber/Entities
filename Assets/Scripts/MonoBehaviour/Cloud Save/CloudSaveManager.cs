using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

public class CloudSaveManager : SingletonBehaviour<CloudSaveManager>, SaveDataSender
{
    private const string key = "SaveData";

    [SerializeField] private SaveData saveData;
    [SerializeField] private float saveTime = 15.0f;

    public bool IsSignedIn;
    public bool SaveDataPersists;
    private LocalSaveManager localSaveManager;
    private float saveTimer;

    protected override void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
        localSaveManager = LocalSaveManager.Instance;
        localSaveManager.OnSaveDataSetEvent += OnSaveDataSetEvent;
        saveTimer = saveTime;
    }

    private async void Start()
    {
        await InitialyzeUnityServicesAsync();
        await FetchData();
        await UpdateWithLocalSaveManager();

    }

    private void OnDestroy()
    {
        SaveDataPersists = false;
    }

    private async void Update()
    {
        if (saveTime > 0f)
        {
            if (saveTimer < 0f)
            {
                saveTimer = saveTime;
                await ForceSaveSingleData(key, saveData);
            }
            else
            {
                saveTimer -= Time.deltaTime;
            }
        }
    }

    private void OnSaveDataSetEvent(SaveDataSender sender, SaveData data)
    {
        CloudSaveManager _sender = sender as CloudSaveManager;
        if (_sender != this)
        {
            saveData = data;
        }
    }

    private async Task FetchData()
    {
        if (IsSignedIn)
        {
            saveData = await RetrieveSpecificData<SaveData>(key);
            if (saveData != null)
            {
                SaveDataPersists = true;
                InfoUI.Instance.SendInformation("CLOUD FETCH SUCCESS", MessageType.SUCCESS);
            }
        }
    }

    private async Task UpdateWithLocalSaveManager()
    {
        if (SaveDataPersists && IsSignedIn)
        {
            if (!saveData.Equals(localSaveManager.SaveData))
            {
                DateTime cloudTime;
                DateTime localTime;
                bool cloudTimeValid = DateTime.TryParse(saveData.DateTime, out cloudTime);
                bool localTimeValid = DateTime.TryParse(localSaveManager.SaveData.DateTime, out localTime);
                if (!cloudTimeValid)
                {
                    cloudTime = DateTime.Now;
                    saveData.DateTime = cloudTime.ToString();
                }
                if (!localTimeValid)
                {
                    localTime = DateTime.Now;
                    localSaveManager.SaveData.DateTime = localTime.ToString();
                }
                if (cloudTime > localTime)
                {
                    localSaveManager.SetSaveData(this, saveData);
                }
                else if (cloudTime < localTime)
                {
                    saveData = localSaveManager.SaveData;
                    await ForceSaveSingleData(key, localSaveManager.SaveData);
                }
                else if(localTime == cloudTime)
                {
                    localSaveManager.CreateNewSaveData();
                    saveData = localSaveManager.SaveData;
                    await ForceSaveSingleData(key, localSaveManager.SaveData);
                }
            }
        }
        else if (localSaveManager.SaveDataPersists)
        {
            saveData = localSaveManager.SaveData;
            SaveDataPersists = true;
        }
    }

    private async Task ForceSaveSingleData(string key, object value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();
            oneElement.Add(key, value);
            Dictionary<string, string> result =
                await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);
            Debug.Log(
                $"Successfully saved {key}:{value} with updated write lock {result[key]}"
            );
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task InitialyzeUnityServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            IsSignedIn = true;
        }
    }

    private async Task ListKeys()
    {
        var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
        for (int i = 0; i < keys.Count; i++)
        {
            Debug.Log(keys[i].Key);
        }
        var keysCustom = await CloudSaveService.Instance.Data.Custom.ListAllKeysAsync("Logins_1");
        for (int i = 0; i < keysCustom.Count; i++)
        {
            Debug.Log(keysCustom[i].Key);
        }
    }

    private async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key }
            );

            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAs<T>();
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        return default;
    }
}

